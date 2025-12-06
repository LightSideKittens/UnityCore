using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Line Breaker — разбивает текст на строки.
/// Использует LineBreakAlgorithm (UAX #14) для определения break opportunities
/// и выполняет word wrapping.
/// </summary>
public sealed class LineBreaker
{
    private readonly LineBreakAlgorithm lineBreakAlgorithm;

    // Mandatory line break characters (stable Unicode codepoints per UAX #14)
    private const int LineFeed = 0x000A;
    private const int VerticalTab = 0x000B;
    private const int FormFeed = 0x000C;
    private const int CarriageReturn = 0x000D;
    private const int NextLine = 0x0085;
    private const int LineSeparator = 0x2028;
    private const int ParagraphSeparator = 0x2029;

    // Internal buffers for break opportunities
    private bool[] breakOpportunities = new bool[257];

    // Temporary state during line breaking
    private TextLine[] tempLines;
    private int tempLineCount;
    private ShapedRun[] tempOrderedRuns;
    private int tempOrderedRunCount;

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
    public void BreakLines(
        ReadOnlySpan<int> codepoints,
        ReadOnlySpan<ShapedRun> runs,
        ReadOnlySpan<ShapedGlyph> glyphs,
        float maxWidth,
        BidiParagraph[] paragraphs,
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

        if (runs.IsEmpty)
        {
            lineCount = 0;
            orderedRunCount = 0;
            return;
        }

        // Step 1: Get break opportunities
        GetBreakOpportunities(codepoints);

        // Step 2: Wrap lines
        WrapLines(codepoints, runs, glyphs, maxWidth);

        // Step 3: BiDi reorder runs within each line (UAX #9, rule L2)
        // Each line uses the baseLevel of its containing paragraph
        ReorderRunsPerLine(paragraphs);

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
            breakOpportunities = new bool[Math.Max(requiredLength, breakOpportunities.Length * 2)];
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
        if (cp > ParagraphSeparator) return false;

        return cp == LineFeed ||
               cp == VerticalTab ||
               cp == FormFeed ||
               cp == CarriageReturn ||
               cp == NextLine ||
               cp == LineSeparator ||
               cp == ParagraphSeparator;
    }

    private void WrapLines(
        ReadOnlySpan<int> codepoints,
        ReadOnlySpan<ShapedRun> runs,
        ReadOnlySpan<ShapedGlyph> glyphs,
        float maxWidth)
    {
        if (runs.IsEmpty) return;

        int lineStartRun = 0;
        int lineStartGlyph = 0;
        float lineWidth = 0;

        int lastBreakRun = -1;
        int lastBreakGlyph = -1;
        float widthAtLastBreak = 0;
        int runsLen = runs.Length;

        for (int runIdx = 0; runIdx < runsLen; runIdx++)
        {
            var run = runs[runIdx];
            int runGlyphCount = run.glyphCount;
            int glyphStart = run.glyphStart;
            int rangeStart = run.range.start;

            for (int g = 0; g < runGlyphCount; g++)
            {
                var glyph = glyphs[glyphStart + g];
                int codepointIndex = rangeStart + glyph.cluster;

                // Check mandatory break first (cheaper than array lookup most of the time)
                int cp = (uint)codepointIndex < (uint)codepoints.Length ? codepoints[codepointIndex] : 0;
                if (IsMandatoryBreak(cp))
                {
                    CreateLineFromGlyphs(runs, glyphs, lineStartRun, lineStartGlyph, runIdx, g, lineWidth);

                    lineStartRun = runIdx;
                    lineStartGlyph = g + 1;
                    if (lineStartGlyph >= runGlyphCount)
                    {
                        lineStartRun = runIdx + 1;
                        lineStartGlyph = 0;
                    }
                    lineWidth = 0;
                    lastBreakRun = -1;
                    lastBreakGlyph = -1;
                    continue;
                }

                lineWidth += glyph.advanceX;

                if (CanBreakAfter(codepointIndex))
                {
                    lastBreakRun = runIdx;
                    lastBreakGlyph = g;
                    widthAtLastBreak = lineWidth;
                }

                if (lineWidth > maxWidth && lastBreakRun >= 0)
                {
                    CreateLineFromGlyphs(runs, glyphs, lineStartRun, lineStartGlyph,
                        lastBreakRun, lastBreakGlyph, widthAtLastBreak);

                    lineStartRun = lastBreakRun;
                    lineStartGlyph = lastBreakGlyph + 1;
                    int breakRunGlyphCount = runs[lastBreakRun].glyphCount;
                    if (lineStartGlyph >= breakRunGlyphCount)
                    {
                        lineStartRun = lastBreakRun + 1;
                        lineStartGlyph = 0;
                    }
                    lineWidth -= widthAtLastBreak;
                    lastBreakRun = -1;
                    lastBreakGlyph = -1;
                }
            }
        }

        // Last line
        if (lineStartRun < runsLen)
        {
            int lastRun = runsLen - 1;
            int lastGlyph = runs[lastRun].glyphCount - 1;
            if (lastGlyph >= 0 || lineStartRun < lastRun)
            {
                CreateLineFromGlyphs(runs, glyphs, lineStartRun, lineStartGlyph, lastRun, lastGlyph, lineWidth);
            }
        }
    }

    private void CreateLineFromGlyphs(
        ReadOnlySpan<ShapedRun> runs,
        ReadOnlySpan<ShapedGlyph> glyphs,
        int startRun, int startGlyph,
        int endRun, int endGlyph,
        float width)
    {
        if (startRun > endRun || (startRun == endRun && startGlyph > endGlyph))
            return;

        var firstRun = runs[startRun];
        var lastRun = runs[endRun];

        int rangeStart = firstRun.range.start;
        if (startGlyph > 0 && startGlyph < firstRun.glyphCount)
        {
            var g = glyphs[firstRun.glyphStart + startGlyph];
            rangeStart = firstRun.range.start + g.cluster;
        }

        int rangeEnd = lastRun.range.End;
        if (endGlyph >= 0 && endGlyph < lastRun.glyphCount)
        {
            var g = glyphs[lastRun.glyphStart + endGlyph];
            rangeEnd = lastRun.range.start + g.cluster + 1;
        }

        int lineRunStart = tempOrderedRunCount;
        int lineRunCount = 0;

        for (int r = startRun; r <= endRun; r++)
        {
            var originalRun = runs[r];

            int runGlyphStart, runGlyphEnd;

            if (r == startRun && r == endRun)
            {
                runGlyphStart = startGlyph;
                runGlyphEnd = endGlyph;
            }
            else if (r == startRun)
            {
                runGlyphStart = startGlyph;
                runGlyphEnd = originalRun.glyphCount - 1;
            }
            else if (r == endRun)
            {
                runGlyphStart = 0;
                runGlyphEnd = endGlyph;
            }
            else
            {
                runGlyphStart = 0;
                runGlyphEnd = originalRun.glyphCount - 1;
            }

            int glyphCount = runGlyphEnd - runGlyphStart + 1;
            if (glyphCount <= 0) continue;

            float partialWidth = 0;
            for (int g = runGlyphStart; g <= runGlyphEnd; g++)
            {
                partialWidth += glyphs[originalRun.glyphStart + g].advanceX;
            }

            EnsureOrderedRunCapacity(tempOrderedRunCount + 1);
            tempOrderedRuns[tempOrderedRunCount++] = new ShapedRun
            {
                range = originalRun.range,
                glyphStart = originalRun.glyphStart + runGlyphStart,
                glyphCount = glyphCount,
                width = partialWidth,
                direction = originalRun.direction,
                bidiLevel = originalRun.bidiLevel,
                fontId = originalRun.fontId,
                attributeSnapshot = originalRun.attributeSnapshot
            };
            lineRunCount++;
        }

        EnsureLineCapacity(tempLineCount + 1);
        tempLines[tempLineCount++] = new TextLine
        {
            range = new TextRange(rangeStart, rangeEnd - rangeStart),
            runStart = lineRunStart,
            runCount = lineRunCount,
            width = width,
            height = 0,
            baseline = 0
        };
    }

    private void ReorderRunsPerLine(BidiParagraph[] paragraphs)
    {
        for (int i = 0; i < tempLineCount; i++)
        {
            var line = tempLines[i];

            // Find the paragraph that contains this line
            byte paragraphBaseLevel = FindParagraphBaseLevel(line.range.start, paragraphs);

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
    private static byte FindParagraphBaseLevel(int codepointIndex, BidiParagraph[] paragraphs)
    {
        if (paragraphs == null || paragraphs.Length == 0)
            return 0; // Default LTR

        // Most common case: single paragraph
        if (paragraphs.Length == 1)
            return paragraphs[0].baseLevel;

        // Multiple paragraphs: search
        for (int i = 0; i < paragraphs.Length; i++)
        {
            var para = paragraphs[i];
            if (codepointIndex >= para.startIndex && codepointIndex <= para.endIndex)
                return para.baseLevel;
        }

        return paragraphs[0].baseLevel;
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
            var temp = arr[start];
            arr[start] = arr[end];
            arr[end] = temp;
            start++;
            end--;
        }
    }

    private void EnsureLineCapacity(int required)
    {
        if (tempLines.Length >= required) return;
        Array.Resize(ref tempLines, Math.Max(required, tempLines.Length * 2));
    }

    private void EnsureOrderedRunCapacity(int required)
    {
        if (tempOrderedRuns.Length >= required) return;
        Array.Resize(ref tempOrderedRuns, Math.Max(required, tempOrderedRuns.Length * 2));
    }
}
