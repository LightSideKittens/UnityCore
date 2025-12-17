using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;

/// <summary>
/// Line Breaker — разбивает текст на строки.
/// Использует LineBreakAlgorithm (UAX #14) для определения break opportunities
/// и выполняет word wrapping.
/// </summary>
public sealed class LineBreaker
{
    private readonly LineBreakAlgorithm lineBreakAlgorithm;

    // DEBUG
    public static bool DebugLogging = false;

    // Internal buffer from ArrayPool
    private const int MinBreakOpportunitiesSize = 256;
    private bool[] breakOpportunities = ArrayPool<bool>.Shared.Rent(MinBreakOpportunitiesSize);

    // Temporary state during line breaking (uses external buffers from SharedTextBuffers)
    private TextLine[] tempLines;
    private int tempLineCount;
    private ShapedRun[] tempOrderedRuns;
    private int tempOrderedRunCount;
    private int searchStartRunIdx; // Optimization: skip fully processed runs

    // Temporary paragraph reference for BiDi reordering
    private BidiParagraph[] tempParagraphs;
    private int tempParagraphCount;

    public LineBreaker(LineBreakAlgorithm lineBreakAlgorithm)
    {
        this.lineBreakAlgorithm = lineBreakAlgorithm ?? throw new ArgumentNullException(nameof(lineBreakAlgorithm));
    }

    /// <summary>
    /// Создать LineBreaker с использованием статического UnicodeData.
    /// </summary>
    public LineBreaker()
    {
        lineBreakAlgorithm = new LineBreakAlgorithm();
    }


    /// <summary>
    /// Разбить текст на строки и записать результат в предоставленные буферы.
    /// </summary>
    /// <param name="glyphScale">Scale factor for margins (fontSize / shapingFontSize). Default 1.0 for no scaling.</param>
    public void BreakLines(
        ReadOnlySpan<int> codepoints,
        ReadOnlySpan<ShapedRun> runs,
        ReadOnlySpan<ShapedGlyph> glyphs,
        float maxWidth,
        BidiParagraph[] paragraphs,
        int paragraphCount,
        ref TextLine[] linesOut,
        ref int lineCount,
        ref ShapedRun[] orderedRunsOut,
        ref int orderedRunCount)
    {
        // Store references to output buffers
        tempLines = linesOut;
        tempLineCount = 0;
        tempOrderedRuns = orderedRunsOut;
        tempOrderedRunCount = 0;
        tempParagraphs = paragraphs;
        tempParagraphCount = paragraphCount;

        if (runs.IsEmpty)
        {
            lineCount = 0;
            orderedRunCount = 0;
            return;
        }

        Profiler.BeginSample("LineBreaker.GetBreakOpportunities");
        // Step 1: Get break opportunities
        GetBreakOpportunities(codepoints);
        Profiler.EndSample();

        Profiler.BeginSample("LineBreaker.WrapLines");
        // Step 2: Wrap lines
        WrapLines(codepoints, runs, glyphs, maxWidth);
        Profiler.EndSample();
        
        Profiler.BeginSample("LineBreaker.ReorderRunsPerLine");
        // Step 3: BiDi reorder runs within each line (UAX #9, rule L2)
        // Each line uses the baseLevel of its containing paragraph
        ReorderRunsPerLine();
        Profiler.EndSample();

        // Return potentially resized buffers
        linesOut = tempLines;
        orderedRunsOut = tempOrderedRuns;
        lineCount = tempLineCount;
        orderedRunCount = tempOrderedRunCount;
    }

    private void GetBreakOpportunities(ReadOnlySpan<int> codepoints)
    {
        int requiredLength = codepoints.Length + 1;
        if (breakOpportunities.Length < requiredLength)
        {
            // Return old buffer and rent larger one
            int newSize = Math.Max(requiredLength, breakOpportunities.Length * 2);
            ArrayPool<bool>.Shared.Return(breakOpportunities);
            breakOpportunities = ArrayPool<bool>.Shared.Rent(newSize);
        }

        lineBreakAlgorithm.GetBreakOpportunities(codepoints, breakOpportunities);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool CanBreakAfter(int index)
    {
        int breakIndex = index + 1;
        return (uint)breakIndex < (uint)breakOpportunities.Length && breakOpportunities[breakIndex];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsMandatoryBreak(int cp)
    {
        // Optimized: single comparison for most common case (not a break)
        // Most characters are > 0x2029, so this check is fast
        if (cp > UnicodeData.ParagraphSeparator) return false;

        return cp == UnicodeData.LineFeed ||
               cp == UnicodeData.VerticalTab ||
               cp == UnicodeData.FormFeed ||
               cp == UnicodeData.CarriageReturn ||
               cp == UnicodeData.NextLine ||
               cp == UnicodeData.LineSeparator ||
               cp == UnicodeData.ParagraphSeparator;
    }

    private void WrapLines(
        ReadOnlySpan<int> codepoints,
        ReadOnlySpan<ShapedRun> runs,
        ReadOnlySpan<ShapedGlyph> glyphs,
        float maxWidth)
    {
        if (runs.IsEmpty) return;

        // Reset run search position for new wrapping pass
        searchStartRunIdx = 0;

        // Build codepoint-to-width mapping for logical order iteration
        // For each codepoint, accumulate width from all glyphs that reference it
        int cpCount = codepoints.Length;

        // Use stackalloc for small texts (up to 16KB), ArrayPool for larger
        const int StackAllocThreshold = 4096;
        float[] rentedArray = null;
        Span<float> cpWidths = cpCount <= StackAllocThreshold
            ? stackalloc float[cpCount]
            : (rentedArray = ArrayPool<float>.Shared.Rent(cpCount)).AsSpan(0, cpCount);
        cpWidths.Clear();

        // Get margins from SharedTextBuffers.Current
        var margins = CommonData.Current.startMargins;

        try
        {
            for (int runIdx = 0; runIdx < runs.Length; runIdx++)
            {
                var run = runs[runIdx];
                int rangeStart = run.range.start;
                for (int g = 0; g < run.glyphCount; g++)
                {
                    var glyph = glyphs[run.glyphStart + g];
                    int cpIdx = rangeStart + glyph.cluster;
                    if ((uint)cpIdx < (uint)cpCount)
                        cpWidths[cpIdx] += glyph.advanceX;
                }
            }

            // Now iterate in logical order
            int lineStartCp = 0;
            float lineWidth = 0;
            int lastBreakCp = -1;      // последняя break opportunity которая помещается
            float widthAtLastBreak = 0;

            // Get margin for first line
            // Margins are in glyph space (same units as maxWidth), no scaling needed
            float rawMargin = (uint)lineStartCp < (uint)margins.Length ? margins[lineStartCp] : 0f;
            float effectiveMaxWidth = maxWidth - rawMargin;

            for (int cpIdx = 0; cpIdx < cpCount; cpIdx++)
            {
                int cp = codepoints[cpIdx];

                // Mandatory break
                if (IsMandatoryBreak(cp))
                {
                    CreateLineFromCodepoints(runs, glyphs, codepoints, lineStartCp, cpIdx, lineWidth, rawMargin);
                    lineStartCp = cpIdx + 1;
                    lineWidth = 0;
                    lastBreakCp = -1;
                    widthAtLastBreak = 0;
                    // Update margin for next line
                    rawMargin = (uint)lineStartCp < (uint)margins.Length ? margins[lineStartCp] : 0f;
                    effectiveMaxWidth = maxWidth - rawMargin;
                    continue;
                }

                // Сначала проверяем — если можно сделать break ДО добавления текущего символа, запоминаем
                // (т.е. break ПОСЛЕ предыдущего символа, если он был canBreak)
                // Это уже сделано на предыдущей итерации

                lineWidth += cpWidths[cpIdx];

                if (DebugLogging)
                {
                    UnityEngine.Debug.Log($"[LineBreaker] cpIdx={cpIdx} cp=U+{cp:X4} width={cpWidths[cpIdx]:F1} canBreak={CanBreakAfter(cpIdx)} lineWidth={lineWidth:F1} lastBreakCp={lastBreakCp} margin={rawMargin:F1}");
                }

                // Проверяем переполнение ПЕРЕД обновлением lastBreakCp
                while (lineWidth > effectiveMaxWidth)
                {
                    if (lastBreakCp >= 0 && lastBreakCp >= lineStartCp)
                    {
                        // Normal break: есть break opportunity — используем её
                        if (DebugLogging)
                        {
                            UnityEngine.Debug.Log($"[LineBreaker] BREAK at cpIdx={lastBreakCp}, lineWidth={lineWidth:F1} > effectiveMaxWidth={effectiveMaxWidth:F1}");
                        }
                        CreateLineFromCodepoints(runs, glyphs, codepoints, lineStartCp, lastBreakCp, widthAtLastBreak, rawMargin);
                        lineStartCp = lastBreakCp + 1;
                        lineWidth -= widthAtLastBreak;
                        lastBreakCp = -1;
                        widthAtLastBreak = 0;
                        // Update margin for next line
                        rawMargin = (uint)lineStartCp < (uint)margins.Length ? margins[lineStartCp] : 0f;
                        effectiveMaxWidth = maxWidth - rawMargin;
                    }
                    else if (cpIdx > lineStartCp)
                    {
                        // Emergency break: слово слишком длинное, нет break opportunity
                        float widthBeforeCurrent = lineWidth - cpWidths[cpIdx];
                        if (DebugLogging)
                        {
                            UnityEngine.Debug.Log($"[LineBreaker] EMERGENCY BREAK before cpIdx={cpIdx}, width={widthBeforeCurrent:F1}");
                        }
                        CreateLineFromCodepoints(runs, glyphs, codepoints, lineStartCp, cpIdx - 1, widthBeforeCurrent, rawMargin);
                        lineStartCp = cpIdx;
                        lineWidth = cpWidths[cpIdx];
                        lastBreakCp = -1;
                        widthAtLastBreak = 0;
                        // Update margin for next line
                        rawMargin = (uint)lineStartCp < (uint)margins.Length ? margins[lineStartCp] : 0f;
                        effectiveMaxWidth = maxWidth - rawMargin;
                    }
                    else
                    {
                        // Первый символ в строке уже не влезает — оставляем его
                        break;
                    }
                }

                // Обновляем lastBreakCp ПОСЛЕ проверки переполнения
                // Так lastBreakCp всегда указывает на символ который помещается
                if (CanBreakAfter(cpIdx))
                {
                    lastBreakCp = cpIdx;
                    widthAtLastBreak = lineWidth;
                }
            }

            // Last line
            if (lineStartCp < cpCount)
            {
                CreateLineFromCodepoints(runs, glyphs, codepoints, lineStartCp, cpCount - 1, lineWidth, rawMargin);
            }
        }
        finally
        {
            if (rentedArray != null)
                ArrayPool<float>.Shared.Return(rentedArray);
        }
    }

    private void CreateLineFromCodepoints(
        ReadOnlySpan<ShapedRun> runs,
        ReadOnlySpan<ShapedGlyph> glyphs,
        ReadOnlySpan<int> codepoints,
        int startCp, int endCp,
        float width, float startMargin = 0f)
    {
        if (startCp > endCp) return;

        if (DebugLogging)
        {
            UnityEngine.Debug.Log($"[LineBreaker.CreateLineFromCodepoints] Creating line for codepoints [{startCp}, {endCp}], margin={startMargin:F1}");
        }

        // Find runs that overlap with [startCp, endCp]
        int lineRunStart = tempOrderedRunCount;
        int lineRunCount = 0;

        // Start from last known position - runs are in logical order
        for (int runIdx = searchStartRunIdx; runIdx < runs.Length; runIdx++)
        {
            var run = runs[runIdx];
            int runStart = run.range.start;
            int runEnd = run.range.End - 1;

            // Run is completely before our range - skip (and advance start for next line)
            if (runEnd < startCp)
            {
                searchStartRunIdx = runIdx + 1;
                continue;
            }

            // Run is completely after our range - done with this line
            if (runStart > endCp)
                break;

            if (DebugLogging)
            {
                UnityEngine.Debug.Log($"  Checking run {runIdx}: range=[{runStart}, {runEnd}], glyphStart={run.glyphStart}, glyphCount={run.glyphCount}, dir={run.direction}");
            }

            // Find glyph range for this run that falls within [startCp, endCp]
            int glyphFirst = -1, glyphLast = -1;

            for (int g = 0; g < run.glyphCount; g++)
            {
                var glyph = glyphs[run.glyphStart + g];
                int cpIdx = runStart + glyph.cluster;
                bool inRange = cpIdx >= startCp && cpIdx <= endCp;

                if (DebugLogging)
                {
                    UnityEngine.Debug.Log($"    g={g}: cluster={glyph.cluster}, cpIdx={cpIdx}, glyphId={glyph.glyphId}, inRange={inRange}");
                }

                if (inRange)
                {
                    if (glyphFirst < 0) glyphFirst = g;
                    glyphLast = g;
                }
            }

            if (glyphFirst < 0) continue;

            int glyphCount = glyphLast - glyphFirst + 1;

            // Считаем реальную ширину ВСЕХ глифов в диапазоне [glyphFirst, glyphLast]
            // потому что именно они будут отрисованы в Layout
            float partialWidth = 0;
            for (int g = glyphFirst; g <= glyphLast; g++)
            {
                partialWidth += glyphs[run.glyphStart + g].advanceX;
            }

            if (DebugLogging)
            {
                UnityEngine.Debug.Log($"  Selected: glyphFirst={glyphFirst}, glyphLast={glyphLast}, newGlyphStart={run.glyphStart + glyphFirst}, count={glyphCount}");
            }

            EnsureOrderedRunCapacity(tempOrderedRunCount + 1);
            tempOrderedRuns[tempOrderedRunCount++] = new ShapedRun
            {
                range = run.range,
                glyphStart = run.glyphStart + glyphFirst,
                glyphCount = glyphCount,
                width = partialWidth,
                direction = run.direction,
                bidiLevel = run.bidiLevel,
                fontId = run.fontId
            };
            lineRunCount++;
        }

        // Вычисляем реальную ширину строки как сумму partialWidth всех runs
        // Это точнее чем cpWidths, т.к. учитывает все глифы которые будут отрисованы
        float actualLineWidth = 0;
        for (int i = lineRunStart; i < tempOrderedRunCount; i++)
        {
            actualLineWidth += tempOrderedRuns[i].width;
        }

        // DEBUG: проверяем что ширина не превышает ожидаемую
        if (DebugLogging)
        {
            UnityEngine.Debug.Log($"[LineBreaker] Created line [{startCp}, {endCp}]: width={width:F2}, actualWidth={actualLineWidth:F2}, margin={startMargin:F2}");
        }

        EnsureLineCapacity(tempLineCount + 1);
        tempLines[tempLineCount++] = new TextLine
        {
            range = new TextRange(startCp, endCp - startCp + 1),
            runStart = lineRunStart,
            runCount = lineRunCount,
            width = actualLineWidth,  // используем реальную ширину
            height = 0,
            baseline = 0,
            startMargin = startMargin
        };
    }

    private void ReorderRunsPerLine()
    {
        for (int i = 0; i < tempLineCount; i++)
        {
            var line = tempLines[i];

            // Find the paragraph that contains this line
            byte paragraphBaseLevel = FindParagraphBaseLevel(line.range.start);

            // UAX #9 Rule L2: Reorder runs based on BiDi levels
            ReorderRunsInLine(line.runStart, line.runCount, paragraphBaseLevel);

            // Store the paragraph base level in the line for layout alignment
            line.paragraphBaseLevel = paragraphBaseLevel;
            tempLines[i] = line;
        }
    }

    /// <summary>
    /// Find the base level of the paragraph containing the given codepoint index.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private byte FindParagraphBaseLevel(int codepointIndex)
    {
        if (tempParagraphs == null || tempParagraphCount == 0)
            return 0; // Default LTR

        // Most common case: single paragraph
        if (tempParagraphCount == 1)
            return tempParagraphs[0].baseLevel;

        // Multiple paragraphs: search
        for (int i = 0; i < tempParagraphCount; i++)
        {
            var para = tempParagraphs[i];
            if (codepointIndex >= para.startIndex && codepointIndex <= para.endIndex)
                return para.baseLevel;
        }

        return tempParagraphs[0].baseLevel;
    }

    /// <summary>
    /// UAX #9, Rule L2: Reorder runs based on BiDi levels.
    /// From the highest level found in the text to the lowest odd level on each line,
    /// including intermediate levels not actually present in the text,
    /// reverse any contiguous sequence of characters that are at that level or higher.
    /// </summary>
    private void ReorderRunsInLine(int start, int count, byte paragraphBaseLevel)
    {
        if (count <= 1) return;

        // Find the highest level and lowest odd level
        byte maxLevel = paragraphBaseLevel;
        byte minLevel = paragraphBaseLevel;

        for (int i = 0; i < count; i++)
        {
            byte level = tempOrderedRuns[start + i].bidiLevel;
            if (level > maxLevel) maxLevel = level;
            if (level < minLevel) minLevel = level;
        }

        // Determine the lowest odd level to process
        // For RTL paragraph (baseLevel=1), we start reversing from level 1
        // For LTR paragraph (baseLevel=0), we start from level 1 (first odd)
        byte lowestOddLevel = (minLevel & 1) == 1 ? minLevel : (byte)(minLevel + 1);
        if (lowestOddLevel > maxLevel) return; // No odd levels, no reordering needed

        // L2: Reverse sequences at each level from maxLevel down to lowestOddLevel
        for (byte level = maxLevel; level >= lowestOddLevel; level--)
        {
            int runStart = -1;

            for (int i = 0; i <= count; i++)
            {
                bool inSequence = i < count && tempOrderedRuns[start + i].bidiLevel >= level;

                if (inSequence && runStart < 0)
                {
                    runStart = i;
                }
                else if (!inSequence && runStart >= 0)
                {
                    ReverseRuns(start + runStart, i - runStart);
                    runStart = -1;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReverseRuns(int start, int count)
    {
        var arr = tempOrderedRuns;
        int end = start + count - 1;
        while (start < end)
        {
            (arr[start], arr[end]) = (arr[end], arr[start]);
            start++;
            end--;
        }
    }

    private void EnsureLineCapacity(int required)
    {
        if (tempLines != null && tempLines.Length >= required) return;

        int newSize = Math.Max(required, tempLines?.Length * 2 ?? 128);
        var newBuffer = ArrayPool<TextLine>.Shared.Rent(newSize);

        if (tempLines != null)
        {
            tempLines.AsSpan(0, tempLineCount).CopyTo(newBuffer);
            ArrayPool<TextLine>.Shared.Return(tempLines);
        }

        tempLines = newBuffer;
    }

    private void EnsureOrderedRunCapacity(int required)
    {
        if (tempOrderedRuns != null && tempOrderedRuns.Length >= required) return;

        int newSize = Math.Max(required, tempOrderedRuns?.Length * 2 ?? 512);
        var newBuffer = ArrayPool<ShapedRun>.Shared.Rent(newSize);

        if (tempOrderedRuns != null)
        {
            tempOrderedRuns.AsSpan(0, tempOrderedRunCount).CopyTo(newBuffer);
            ArrayPool<ShapedRun>.Shared.Return(tempOrderedRuns);
        }

        tempOrderedRuns = newBuffer;
    }
}
