using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

public struct TextProcessSettings
{
    public const float FloatMax = 32767f;

    public LayoutSettings layout;
    public float fontSize;
    public TextDirection baseDirection;
    public bool enableWordWrap;

    public float MaxWidth
    {
        get => layout.maxWidth;
        set => layout.maxWidth = value;
    }

    public float MaxHeight
    {
        get => layout.maxHeight;
        set => layout.maxHeight = value;
    }

    public HorizontalAlignment HorizontalAlignment
    {
        get => layout.horizontalAlignment;
        set => layout.horizontalAlignment = value;
    }

    public VerticalAlignment VerticalAlignment
    {
        get => layout.verticalAlignment;
        set => layout.verticalAlignment = value;
    }

    public float LineSpacing
    {
        get => layout.lineSpacing;
        set => layout.lineSpacing = value;
    }
}

public sealed class TextProcessor
{
    private static BidiEngine BidiEngine => SharedPipelineComponents.BidiEngine;
    private static ScriptAnalyzer ScriptAnalyzer => SharedPipelineComponents.ScriptAnalyzer;
    private static IShapingEngine ShapingEngine => SharedPipelineComponents.ShapingEngine;
    private static LineBreaker LineBreaker => SharedPipelineComponents.LineBreaker;
    private static TextLayout Layout => SharedPipelineComponents.Layout;

    public readonly UniTextBuffers buf;
    private UniTextFontProvider fontProvider;
    private int baseFontId;
    private float resultWidth;
    private float resultHeight;

    private bool hasValidFirstPassData;

    private float lastLinesWidth = -1;
    private float lastLinesFontSize = -1;
    private bool lastLinesWordWrap;
    private bool hasValidLinesData;

    private float lastLayoutMaxHeight = -1;
    private HorizontalAlignment lastLayoutHAlign;
    private VerticalAlignment lastLayoutVAlign;
    private bool hasValidPositionedGlyphs;

    private TextProcessSettings lastSettings;

    private ReadOnlyMemory<char> lastText;

    public static int processCallCount;
    public static int ensureShapingCallCount;
    public static int doFullShapingCallCount;

    public event Action Parsed;
    public event Action Shaped;
    public event Action LayoutComplete;

    public TextProcessor(UniTextBuffers uniTextBuffers)
    {
        buf = uniTextBuffers ?? throw new ArgumentNullException(nameof(uniTextBuffers));
        if (UnicodeData.Provider == null)
            throw new InvalidOperationException("UnicodeData not initialized.");
    }

    public void SetFontProvider(UniTextFontProvider provider, int defaultFontId = 0)
    {
        fontProvider = provider;
        baseFontId = defaultFontId;
    }
    
    public bool HasValidFirstPassData => hasValidFirstPassData;
    
    public void InvalidateFirstPassData()
    {
        hasValidFirstPassData = false;
        InvalidateLayoutData();
    }

    public void InvalidateLayoutData()
    {
        hasValidLinesData = false;
        hasValidPositionedGlyphs = false;
        lastLinesWidth = -1;
        lastLinesFontSize = -1;
        lastLinesWordWrap = false;
        lastLayoutMaxHeight = -1;
    }

    public void InvalidatePositionedGlyphs()
    {
        hasValidPositionedGlyphs = false;
        lastLayoutMaxHeight = -1;
    }

    public void EnsureFirstPass(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        ensureShapingCallCount++;

        if (hasValidFirstPassData) return;

        Profiler.BeginSample("TextProcessor.EnsureShaping");

        buf.Reset();

        if (text.IsEmpty)
        {
            hasValidFirstPassData = false;
            Profiler.EndSample();
            return;
        }

        fontProvider.SetFontSize(settings.fontSize);
        DoFirstPass(text, settings);

        Profiler.EndSample();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanReuseLines(float width, float fontSize, bool wordWrap)
    {
        return hasValidLinesData &&
               Math.Abs(lastLinesWidth - width) < 0.001f &&
               Math.Abs(lastLinesFontSize - fontSize) < 0.001f &&
               lastLinesWordWrap == wordWrap;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanReusePositions(float maxHeight, HorizontalAlignment hAlign, VerticalAlignment vAlign)
    {
        if (!hasValidPositionedGlyphs) return false;

        var heightMatches = (float.IsInfinity(lastLayoutMaxHeight) && float.IsInfinity(maxHeight)) ||
                            Math.Abs(lastLayoutMaxHeight - maxHeight) < 0.001f;

        return heightMatches && lastLayoutHAlign == hAlign && lastLayoutVAlign == vAlign;
    }
    
    public void EnsureLines(float width, float fontSize, bool wordWrap)
    {
        if (!hasValidFirstPassData) return;
        if (CanReuseLines(width, fontSize, wordWrap)) return;

        Profiler.BeginSample("TextProcessor.EnsureLines");
        EnsureLinesInternal(width, fontSize, wordWrap, buf.cpWidths.Span);
        Profiler.EndSample();
    }
    
    public void EnsurePositions(TextProcessSettings settings)
    {
        if (!hasValidLinesData) return;
        if (CanReusePositions(settings.MaxHeight, settings.HorizontalAlignment, settings.VerticalAlignment)) return;

        Profiler.BeginSample("TextProcessor.EnsurePositions");

        lastSettings = settings;
        buf.positionedGlyphs.count = 0;
        LayoutText(settings);

        lastLayoutMaxHeight = settings.MaxHeight;
        lastLayoutHAlign = settings.HorizontalAlignment;
        lastLayoutVAlign = settings.VerticalAlignment;
        hasValidPositionedGlyphs = true;

        LayoutComplete?.Invoke();

        Profiler.EndSample();
    }
    
    private void DoFirstPass(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        doFullShapingCallCount++;
        
        buf.shapingFontSize = settings.fontSize;

        Profiler.BeginSample("TextProcessor.Parse");
        Parse(text);
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.ComputeBreakOpportunities");
        ComputeBreakOpportunities();
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.Parsed?.Invoke()");
        Parsed?.Invoke();
        Profiler.EndSample();

        if (buf.codepoints.count == 0)
        {
            hasValidFirstPassData = false;
            hasValidLinesData = false;
            hasValidPositionedGlyphs = false;
            return;
        }

        Profiler.BeginSample("TextProcessor.AnalyzeBidi");
        AnalyzeBidi(settings.baseDirection);
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.AnalyzeScripts");
        AnalyzeScripts();
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.Itemize");
        Itemize();
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.Shape");
        Shape();
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.ComputeCpWidths");
        ComputeCpWidths();
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.Shaped?.Invoke()");
        Shaped?.Invoke();
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.EnsureGlyphsInAtlas");
        EnsureGlyphsInAtlas();
        Profiler.EndSample();

        hasValidFirstPassData = true;
    }
    
    public float GetUnwrappedWidth()
    {
        if (!hasValidFirstPassData) return 0;

        float total = 0;
        var count = buf.shapedRuns.count;
        for (var i = 0; i < count; i++)
            total += buf.shapedRuns[i].width;
        return total;
    }
    
    public float GetPreferredWidth(float fontSize)
    {
        if (!hasValidFirstPassData) return 0;
        var glyphScale = buf.GetGlyphScale(fontSize);
        return Mathf.Ceil(GetMaxLineWidth() * glyphScale);
    }
    
    public float GetPreferredHeight(float fontSize, float lineSpacing = 0f)
    {
        if (!hasValidLinesData) return 0;

        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(fontSize, out var ascender, out var descender, out var lineHeight);
            return UniTextFontProvider.CalculateTextHeight(ascender, descender, buf.lines.count, lineHeight, lineSpacing);
        }

        return buf.lines.count * fontSize * 1.2f;
    }
    
    public float GetMaxLineWidth()
    {
        if (!hasValidFirstPassData) return 0;

        var codepoints = buf.codepoints.Span;
        var glyphs = buf.shapedGlyphs.Span;
        var margins = buf.startMargins;

        var maxWidth = 0f;
        var currentWidth = 0f;
        var lastCluster = -1;
        var lineStartCluster = 0;

        for (var i = 0; i < glyphs.Length; i++)
        {
            var cluster = glyphs[i].cluster;

            if (cluster != lastCluster && cluster < codepoints.Length)
            {
                var cp = codepoints[cluster];
                if (UnicodeData.IsLineBreak(cp))
                {
                    var lineMargin = lineStartCluster < margins.Length ? margins[lineStartCluster] : 0;
                    var totalLineWidth = currentWidth + lineMargin;
                    if (totalLineWidth > maxWidth) maxWidth = totalLineWidth;

                    currentWidth = 0f;
                    lineStartCluster = cluster + 1;
                    lastCluster = cluster;
                    continue;
                }
            }

            currentWidth += glyphs[i].advanceX;
            lastCluster = cluster;
        }

        var lastLineMargin = lineStartCluster < margins.Length ? margins[lineStartCluster] : 0;
        var lastLineWidth = currentWidth + lastLineMargin;
        if (lastLineWidth > maxWidth) maxWidth = lastLineWidth;

        return maxWidth > 0 ? maxWidth : GetUnwrappedWidth();
    }
    
    public float FindOptimalFontSize(
        float minSize,
        float maxSize,
        float targetWidth,
        float targetHeight,
        TextProcessSettings baseSettings)
    {
        if (!hasValidFirstPassData) return minSize;
        if (targetWidth <= 0 || targetHeight <= 0) return minSize;
        
        if (buf.shapingFontSize <= 0) return minSize;

        var unwrappedWidth = GetUnwrappedWidth();
        var maxGlyphScale = maxSize / buf.shapingFontSize;
        var scaledUnwrappedWidth = unwrappedWidth * maxGlyphScale;

        if (!baseSettings.enableWordWrap || scaledUnwrappedWidth <= targetWidth)
        {
            var lineCount = 1;
            var maxLineWidth = 0f;
            var currentLineWidth = 0f;
            var codepoints = buf.codepoints.Span;
            var glyphs = buf.shapedGlyphs.Span;
            var margins = buf.startMargins;
            var glyphIdx = 0;
            var lineStartIdx = 0;

            for (var i = 0; i < codepoints.Length; i++)
            {
                var cp = codepoints[i];
                if (UnicodeData.IsLineBreak(cp))
                {
                    var lineMargin = lineStartIdx < margins.Length ? margins[lineStartIdx] : 0;
                    var totalLineWidth = currentLineWidth + lineMargin;
                    if (totalLineWidth > maxLineWidth) maxLineWidth = totalLineWidth;
                    currentLineWidth = 0f;
                    lineStartIdx = i + 1;
                    lineCount++;
                    if (glyphIdx < glyphs.Length && glyphs[glyphIdx].advanceX == 0f)
                        glyphIdx++;
                }
                else if (cp == '\r')
                {
                    if (glyphIdx < glyphs.Length && glyphs[glyphIdx].advanceX == 0f)
                        glyphIdx++;
                }
                else
                {
                    if (glyphIdx < glyphs.Length)
                    {
                        currentLineWidth += glyphs[glyphIdx].advanceX;
                        glyphIdx++;
                    }
                }
            }

            var lastLineMargin = lineStartIdx < margins.Length ? margins[lineStartIdx] : 0;
            var lastLineTotal = currentLineWidth + lastLineMargin;
            if (lastLineTotal > maxLineWidth) maxLineWidth = lastLineTotal;

            if (maxLineWidth <= 0f) maxLineWidth = 1f;

            var widthLimitedSize = targetWidth / maxLineWidth * buf.shapingFontSize;

            float lineHeightRatio, ascenderRatio, descenderRatio;
            if (fontProvider != null)
            {
                fontProvider.GetLineMetrics(1f, out var asc, out var desc, out var lh);
                lineHeightRatio = lh;
                ascenderRatio = asc;
                descenderRatio = desc;
            }
            else
            {
                lineHeightRatio = 1.2f;
                ascenderRatio = lineHeightRatio * 0.8f;
                descenderRatio = -lineHeightRatio * 0.2f;
            }

            var heightLimitedSize = targetHeight / (ascenderRatio - descenderRatio + (lineCount - 1) * lineHeightRatio);

            var optimalSize = Math.Clamp(Math.Min(widthLimitedSize, heightLimitedSize), minSize, maxSize);
            hasValidLinesData = false;
            hasValidPositionedGlyphs = false;
            return optimalSize;
        }

        const float tolerance = 0.5f;
        var lo = minSize;
        var hi = maxSize;

        var minHeight = GetHeightForFontSize(lo, targetWidth, baseSettings);
        if (minHeight > targetHeight)
            return minSize;

        var maxHeight = GetHeightForFontSize(hi, targetWidth, baseSettings);
        if (maxHeight <= targetHeight)
            return maxSize;

        while (hi - lo > tolerance)
        {
            var mid = (lo + hi) * 0.5f;
            var height = GetHeightForFontSize(mid, targetWidth, baseSettings);

            if (height <= targetHeight)
                lo = mid;
            else
                hi = mid;
        }

        if (Math.Abs(lastLinesFontSize - lo) > 0.001f)
            GetHeightForFontSize(lo, targetWidth, baseSettings);

        return lo;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetHeightForFontSize(float fontSize, float targetWidth, TextProcessSettings baseSettings)
    {
        var glyphScale = buf.GetGlyphScale(fontSize);
        var effectiveMaxWidth = baseSettings.enableWordWrap ? targetWidth / glyphScale : TextProcessSettings.FloatMax;

        buf.lines.count = 0;
        buf.orderedRuns.count = 0;
        buf.positionedGlyphs.count = 0;

        BreakLines(effectiveMaxWidth, buf.cpWidths.Span);

        lastLinesWidth = targetWidth;
        lastLinesFontSize = fontSize;
        hasValidLinesData = true;
        hasValidPositionedGlyphs = false;

        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(fontSize, out var ascender, out var descender, out var lineHeight);
            return UniTextFontProvider.CalculateTextHeight(ascender, descender, buf.lines.count, lineHeight,
                baseSettings.LineSpacing);
        }

        return buf.lines.count * fontSize * 1.2f;
    }

    public float ResultWidth => resultWidth;
    public float ResultHeight => resultHeight;

    public ReadOnlySpan<PositionedGlyph> PositionedGlyphs
    {
        get
        {
            var b = buf;
            return b.positionedGlyphs.Span;
        }
    }

    private void Parse(ReadOnlySpan<char> text)
    {
        buf.codepoints.count = 0;
        buf.EnsureCodepointCapacity(text.Length);

        var i = 0;
        while (i < text.Length) AddCharacter(text, ref i);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddCharacter(ReadOnlySpan<char> text, ref int i)
    {
        var c = text[i];

        if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
        {
            var cp = char.ConvertToUtf32(c, text[i + 1]);
            AddCodepoint(cp);
            i += 2;
        }
        else
        {
            AddCodepoint(c);
            i++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddCodepoint(int cp)
    {
        var count = buf.codepoints.count;
        if (count >= buf.codepoints.Capacity)
            buf.EnsureCodepointCapacity(count + 1);
        buf.codepoints[count] = cp;
        buf.codepoints.count = count + 1;
    }

    private void AnalyzeBidi(TextDirection requestedDirection)
    {
        var cpCount = buf.codepoints.count;
        buf.EnsureBidiCapacity(cpCount);

        var direction = requestedDirection switch
        {
            TextDirection.RightToLeft => BidiParagraphDirection.RightToLeft,
            TextDirection.LeftToRight => BidiParagraphDirection.LeftToRight,
            _ => BidiParagraphDirection.Auto
        };

        var result = BidiEngine.ProcessPooled(buf.codepoints.data.AsSpan(0, cpCount), direction);

        if (result.levels != null && result.levels.Length > 0)
        {
            var copyLen = Math.Min(result.levels.Length, cpCount);
            result.levels.AsSpan(0, copyLen).CopyTo(buf.bidiLevels);
        }
        else
        {
            buf.bidiLevels.AsSpan(0, cpCount).Fill(0);
        }

        var paragraphCount = result.paragraphCount;
        if (paragraphCount > 0)
        {
            buf.EnsureBidiParagraphCapacity(paragraphCount);
            result.ParagraphsSpan.CopyTo(buf.bidiParagraphs.data);
        }

        buf.bidiParagraphs.count = paragraphCount;

        buf.baseDirection = result.Direction == BidiDirection.RightToLeft
            ? TextDirection.RightToLeft
            : TextDirection.LeftToRight;
    }

    private void AnalyzeScripts()
    {
        var cpCount = buf.codepoints.count;
        buf.EnsureScriptCapacity(cpCount);
        ScriptAnalyzer.Analyze(buf.codepoints.data.AsSpan(0, cpCount), buf.scripts);
    }

    private void Itemize()
    {
        buf.runs.count = 0;

        var cpCount = buf.codepoints.count;
        if (cpCount == 0) return;

        var cpSpan = buf.codepoints.data.AsSpan(0, cpCount);
        var lvlSpan = buf.bidiLevels.AsSpan(0, cpCount);
        var scrSpan = buf.scripts.AsSpan(0, cpCount);
        var fp = fontProvider;
        var baseFont = baseFontId;

        if (fp == null)
        {
            ItemizeWithoutFontLookup(cpCount, lvlSpan, scrSpan, baseFont);
            return;
        }

        var lastLookupCodepoint = -1;

        var runStart = 0;
        var currentLevel = lvlSpan[0];
        var currentScript = scrSpan[0];

        var cp0 = cpSpan[0];
        lastLookupCodepoint = cp0;
        var lastLookupResult = fp.FindFontForCodepoint(cp0, baseFont);
        var currentFontId = lastLookupResult;

        for (var i = 1; i < cpCount; i++)
        {
            var level = lvlSpan[i];
            var script = scrSpan[i];

            int fontId;
            var cp = cpSpan[i];
            if (cp == lastLookupCodepoint)
            {
                fontId = lastLookupResult;
            }
            else
            {
                fontId = fp.FindFontForCodepoint(cp, baseFont);
                lastLookupCodepoint = cp;
                lastLookupResult = fontId;
            }

            if (level != currentLevel || script != currentScript || fontId != currentFontId)
            {
                AddRun(runStart, i - runStart, currentLevel, currentScript, currentFontId);
                runStart = i;
                currentLevel = level;
                currentScript = script;
                currentFontId = fontId;
            }
        }

        AddRun(runStart, cpCount - runStart, currentLevel, currentScript, currentFontId);
    }

    private void ItemizeWithoutFontLookup(int cpCount, Span<byte> lvlSpan, Span<UnicodeScript> scrSpan,
        int fontId)
    {
        var runStart = 0;
        var currentLevel = lvlSpan[0];
        var currentScript = scrSpan[0];

        for (var i = 1; i < cpCount; i++)
        {
            var level = lvlSpan[i];
            var script = scrSpan[i];

            if (level != currentLevel || script != currentScript)
            {
                AddRun(runStart, i - runStart, currentLevel, currentScript, fontId);
                runStart = i;
                currentLevel = level;
                currentScript = script;
            }
        }

        AddRun(runStart, cpCount - runStart, currentLevel, currentScript, fontId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddRun(int start, int length, byte bidiLevel, UnicodeScript script, int fontId)
    {
        var count = buf.runs.count;
        if (count >= buf.runs.Capacity)
            buf.EnsureRunCapacity(count + 1);

        buf.runs[count] = new TextRun
        {
            range = new TextRange(start, length),
            bidiLevel = bidiLevel,
            script = script,
            fontId = fontId
        };
        buf.runs.count = count + 1;
    }

    private void Shape()
    {
        buf.shapedRuns.count = 0;
        buf.shapedGlyphs.count = 0;

        var cpCount = buf.codepoints.count;
        var runCnt = buf.runs.count;
        var cp = buf.codepoints.data.AsSpan(0, cpCount);
        var runs = buf.runs;

        for (var i = 0; i < runCnt; i++)
        {
            ref readonly var run = ref runs[i];
            var runCodepoints = cp.Slice(run.range.start, run.range.length);

            var result = ShapingEngine.Shape(
                runCodepoints,
                fontProvider,
                run.fontId,
                run.script,
                run.Direction);

            var glyphStart = buf.shapedGlyphs.count;
            AddShapedGlyphs(result.Glyphs);

            AddShapedRun(new ShapedRun
            {
                range = run.range,
                glyphStart = glyphStart,
                glyphCount = result.Glyphs.Length,
                width = result.TotalAdvance,
                direction = run.Direction,
                bidiLevel = run.bidiLevel,
                fontId = run.fontId
            });
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddShapedGlyphs(ReadOnlySpan<ShapedGlyph> glyphs)
    {
        var count = buf.shapedGlyphs.count;
        var required = count + glyphs.Length;
        if (buf.shapedGlyphs.Capacity < required)
            buf.EnsureShapedGlyphCapacity(required);

        glyphs.CopyTo(buf.shapedGlyphs.data.AsSpan(count));
        buf.shapedGlyphs.count = required;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddShapedRun(ShapedRun run)
    {
        var count = buf.shapedRuns.count;
        if (count >= buf.shapedRuns.Capacity)
            buf.EnsureShapedRunCapacity(count + 1);

        buf.shapedRuns[count] = run;
        buf.shapedRuns.count = count + 1;
    }

    private void ComputeBreakOpportunities()
    {
        var cpCount = buf.codepoints.count;
        var requiredLength = cpCount + 1;
        buf.breakOpportunities.EnsureCapacity(requiredLength);
        buf.breakOpportunities.count = requiredLength;

        SharedPipelineComponents.LineBreakAlgorithm.GetBreakOpportunities(
            buf.codepoints.Span,
            buf.breakOpportunities.data);
    }

    private void ComputeCpWidths()
    {
        var cpCount = buf.codepoints.count;
        buf.cpWidths.EnsureCapacity(cpCount);
        buf.cpWidths.count = cpCount;

        var widths = buf.cpWidths.data;
        Array.Clear(widths, 0, cpCount);

        var runs = buf.shapedRuns.data;
        var runCount = buf.shapedRuns.count;
        var glyphs = buf.shapedGlyphs.data;

        for (var r = 0; r < runCount; r++)
        {
            ref readonly var run = ref runs[r];
            var rangeStart = run.range.start;
            var end = run.glyphStart + run.glyphCount;
            for (var g = run.glyphStart; g < end; g++)
            {
                var cpIdx = rangeStart + glyphs[g].cluster;
                if ((uint)cpIdx < (uint)cpCount)
                    widths[cpIdx] += glyphs[g].advanceX;
            }
        }
    }

    private void EnsureGlyphsInAtlas()
    {
        if (fontProvider == null)
            return;

        fontProvider.EnsureGlyphsInAtlas(
            buf.shapedRuns.Span,
            buf.shapedGlyphs.Span);
    }

    public void ForceRelayout(ReadOnlySpan<float> cpWidths)
    {
        if (!hasValidFirstPassData) return;

        InvalidateLayoutData();
        EnsureLinesInternal(lastSettings.MaxWidth, lastSettings.fontSize, lastSettings.enableWordWrap, cpWidths);
        EnsurePositions(lastSettings);
    }

    public void ForceReposition()
    {
        if (!hasValidFirstPassData || !hasValidLinesData) return;

        UpdateLineWidths();
        hasValidPositionedGlyphs = false;
        EnsurePositions(lastSettings);
    }

    private void UpdateLineWidths()
    {
        var lines = buf.lines.data;
        var lineCount = buf.lines.count;
        var runs = buf.orderedRuns.data;

        for (var i = 0; i < lineCount; i++)
        {
            ref var line = ref lines[i];
            var width = 0f;
            var end = line.runStart + line.runCount;
            for (var r = line.runStart; r < end; r++)
                width += runs[r].width;
            line.width = width;
        }
    }

    private void EnsureLinesInternal(float width, float fontSize, bool wordWrap, ReadOnlySpan<float> cpWidths)
    {
        buf.lines.count = 0;
        buf.orderedRuns.count = 0;
        buf.positionedGlyphs.count = 0;
        hasValidPositionedGlyphs = false;

        var glyphScale = buf.GetGlyphScale(fontSize);
        var effectiveMaxWidth = wordWrap ? width / glyphScale : TextProcessSettings.FloatMax;
        BreakLines(effectiveMaxWidth, cpWidths);

        lastLinesWidth = width;
        lastLinesFontSize = fontSize;
        lastLinesWordWrap = wordWrap;
        hasValidLinesData = true;
    }

    private void BreakLines(float maxWidth, ReadOnlySpan<float> cpWidths)
    {
        Profiler.BeginSample("TextProcessor.BreakLines");
        buf.lines.count = 0;
        buf.orderedRuns.count = 0;

        var linesArr = buf.lines.data;
        var orderedRunsArr = buf.orderedRuns.data;
        var lineCnt = buf.lines.count;
        var orderedRunCnt = buf.orderedRuns.count;

        LineBreaker.BreakLines(
            buf.codepoints.Span,
            buf.shapedRuns.Span,
            buf.shapedGlyphs.Span,
            cpWidths,
            buf.breakOpportunities.Span,
            maxWidth,
            buf.bidiParagraphs.Span,
            ref linesArr, ref lineCnt,
            ref orderedRunsArr, ref orderedRunCnt,
            buf.startMargins.AsSpan(0, buf.codepoints.count));

        buf.lines.data = linesArr;
        buf.orderedRuns.data = orderedRunsArr;
        buf.lines.count = lineCnt;
        buf.orderedRuns.count = orderedRunCnt;
        Profiler.EndSample();
    }

    private void LayoutText(TextProcessSettings settings)
    {
        Profiler.BeginSample("TextProcessor.LayoutText");
        buf.positionedGlyphs.count = 0;
        buf.EnsurePositionedGlyphCapacity(buf.shapedGlyphs.count);

        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(settings.fontSize, out var ascender, out var descender, out var lineHeight);
            Layout.SetFontMetrics(ascender, descender, lineHeight, buf.GetGlyphScale(settings.fontSize));
        }

        Layout.SetLayoutSettings(settings.layout);

        var glyphCnt = buf.positionedGlyphs.count;
        Layout.Layout(
            buf.lines.Span,
            buf.orderedRuns.Span,
            buf.shapedGlyphs.Span,
            buf.positionedGlyphs.data, ref glyphCnt,
            out resultWidth, out resultHeight);
        buf.positionedGlyphs.count = glyphCnt;
        Profiler.EndSample();
    }
}