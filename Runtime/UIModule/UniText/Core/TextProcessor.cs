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

    public static TextProcessSettings Default => new()
    {
        layout = LayoutSettings.Default,
        fontSize = 36f,
        baseDirection = TextDirection.LeftToRight,
        enableWordWrap = true
    };
}

public sealed class TextProcessor
{
    private static BidiEngine BidiEngine => SharedPipelineComponents.BidiEngine;
    private static ScriptAnalyzer ScriptAnalyzer => SharedPipelineComponents.ScriptAnalyzer;
    private static IShapingEngine ShapingEngine => SharedPipelineComponents.ShapingEngine;
    private static LineBreaker LineBreaker => SharedPipelineComponents.LineBreaker;
    private static TextLayout Layout => SharedPipelineComponents.Layout;

    private UniTextFontProvider fontProvider;
    private int baseFontId;
    private float resultWidth;
    private float resultHeight;

    private bool hasValidShapingData;

    private float lastLayoutWidth = -1;
    private float lastLayoutFontSize = -1;
    private float lastLayoutMaxHeight = -1;
    private bool hasValidLayoutData;

    public static int processCallCount;
    public static int ensureShapingCallCount;
    public static int doFullShapingCallCount;

    public event Action Parsed;
    public event Action Shaped;

    public TextProcessor()
    {
        if (UnicodeData.Provider == null)
            throw new InvalidOperationException("UnicodeData not initialized.");
    }

    public void SetFontProvider(UniTextFontProvider provider, int defaultFontId = 0)
    {
        fontProvider = provider;
        baseFontId = defaultFontId;
    }


    /// <param name="text">Text to process</param>
    /// <param name="settings">Processing settings</param>
    public ReadOnlySpan<PositionedGlyph> Process(
        ReadOnlySpan<char> text,
        TextProcessSettings settings)
    {
        processCallCount++;
        var fullRebuild = !hasValidShapingData;

        var buf = CommonData.Current;

        var heightMatches = (float.IsInfinity(lastLayoutMaxHeight) && float.IsInfinity(settings.MaxHeight)) ||
                            Math.Abs(lastLayoutMaxHeight - settings.MaxHeight) < 0.001f;
        var canReuseLayout = !fullRebuild &&
                             hasValidLayoutData &&
                             Math.Abs(lastLayoutWidth - settings.MaxWidth) < 0.001f &&
                             Math.Abs(lastLayoutFontSize - settings.fontSize) < 0.001f &&
                             heightMatches &&
                             buf.positionedGlyphCount > 0;
        
        if (fullRebuild)
        {
            buf.Reset();
            hasValidShapingData = false;
            hasValidLayoutData = false;
        }
        else if (!canReuseLayout)
        {
            buf.lineCount = 0;
            buf.orderedRunCount = 0;
            buf.positionedGlyphCount = 0;
        }

        if (text.IsEmpty)
        {
            resultWidth = 0;
            resultHeight = 0;
            hasValidShapingData = false;
            hasValidLayoutData = false;
            return ReadOnlySpan<PositionedGlyph>.Empty;
        }

        fontProvider?.SetFontSize(settings.fontSize);

        if (fullRebuild)
            if (!DoFullShaping(text, settings))
                return ReadOnlySpan<PositionedGlyph>.Empty;

        if (!canReuseLayout)
        {
            var glyphScale = buf.GetGlyphScale(settings.fontSize);
            var effectiveMaxWidth =
                settings.enableWordWrap ? settings.MaxWidth / glyphScale : TextProcessSettings.FloatMax;
            BreakLines(effectiveMaxWidth);
            LayoutText(settings);
        }

        return buf.positionedGlyphs.AsSpan(0, buf.positionedGlyphCount);
    }


    public bool HasValidShapingData => hasValidShapingData;


    public void InvalidateShapingData()
    {
        hasValidShapingData = false;
        InvalidateLayoutData();
    }


    public void InvalidateLayoutData()
    {
        hasValidLayoutData = false;
        lastLayoutWidth = -1;
        lastLayoutFontSize = -1;
        lastLayoutMaxHeight = -1;
    }


    public void EnsureShaping(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        ensureShapingCallCount++;
        
        if (hasValidShapingData) return;
        
        var buf = CommonData.Current;
        buf.Reset();

        if (text.IsEmpty)
        {
            hasValidShapingData = false;
            return;
        }

        fontProvider?.SetFontSize(settings.fontSize);
        DoFullShaping(text, settings);
    }


    private bool DoFullShaping(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        doFullShapingCallCount++;
        var buf = CommonData.Current;

        buf.shapingFontSize = settings.fontSize;

        Profiler.BeginSample("TextProcessor.Parse");
        Parse(text);
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.Parsed?.Invoke()");
        Parsed?.Invoke();
        Profiler.EndSample();

        if (buf.codepointCount == 0)
        {
            hasValidShapingData = false;
            hasValidLayoutData = false;
            return false;
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

        Profiler.BeginSample("TextProcessor.Shaped?.Invoke()");
        Shaped?.Invoke();
        Profiler.EndSample();

        Profiler.BeginSample("TextProcessor.EnsureGlyphsInAtlas");
        EnsureGlyphsInAtlas();
        Profiler.EndSample();

        hasValidShapingData = true;
        return true;
    }


    public float GetUnwrappedWidth()
    {
        if (!hasValidShapingData) return 0;

        var buf = CommonData.Current;
        float total = 0;
        var count = buf.shapedRunCount;
        for (var i = 0; i < count; i++)
            total += buf.shapedRuns[i].width;
        return total;
    }


    public float GetMaxLineWidth()
    {
        if (!hasValidShapingData) return 0;

        var buf = CommonData.Current;
        var codepoints = buf.codepoints.AsSpan(0, buf.codepointCount);
        var glyphs = buf.shapedGlyphs.AsSpan(0, buf.shapedGlyphCount);
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


    public float GetHeightForWidth(float width, TextProcessSettings settings)
    {
        if (!hasValidShapingData) return 0;

        if (hasValidLayoutData && Math.Abs(lastLayoutWidth - settings.MaxWidth) < 0.001f) return resultHeight;
        
        var buf = CommonData.Current;
        var glyphScale = buf.GetGlyphScale(settings.fontSize);
        var effectiveMaxWidth = settings.enableWordWrap ? width / glyphScale : TextProcessSettings.FloatMax;

        BreakLines(effectiveMaxWidth);
        LayoutText(settings);

        lastLayoutWidth = settings.MaxWidth;
        lastLayoutFontSize = settings.fontSize;
        lastLayoutMaxHeight = settings.MaxHeight;
        hasValidLayoutData = true;

        return resultHeight;
    }


    /// <param name="minSize">Minimum font size</param>
    /// <param name="maxSize">Maximum font size</param>
    /// <param name="targetWidth">Target width constraint</param>
    /// <param name="targetHeight">Target height constraint</param>
    /// <param name="baseSettings">Base settings (fontSize will be modified during search)</param>
    /// <returns>Optimal font size that fits, or minSize if text doesn't fit even at minimum</returns>
    public float FindOptimalFontSize(
        float minSize,
        float maxSize,
        float targetWidth,
        float targetHeight,
        TextProcessSettings baseSettings)
    {
        if (!hasValidShapingData) return minSize;
        if (targetWidth <= 0 || targetHeight <= 0) return minSize;

        var buf = CommonData.Current;
        if (buf.shapingFontSize <= 0) return minSize;

        var unwrappedWidth = GetUnwrappedWidth();
        var maxGlyphScale = maxSize / buf.shapingFontSize;
        var scaledUnwrappedWidth = unwrappedWidth * maxGlyphScale;

        if (!baseSettings.enableWordWrap || scaledUnwrappedWidth <= targetWidth)
        {
            var lineCount = 1;
            var maxLineWidth = 0f;
            var currentLineWidth = 0f;
            var codepoints = buf.codepoints.AsSpan(0, buf.codepointCount);
            var glyphs = buf.shapedGlyphs.AsSpan(0, buf.shapedGlyphCount);
            var glyphIdx = 0;

            for (var i = 0; i < codepoints.Length; i++)
            {
                var cp = codepoints[i];
                if (UnicodeData.IsLineBreak(cp))
                {
                    if (currentLineWidth > maxLineWidth) maxLineWidth = currentLineWidth;
                    currentLineWidth = 0f;
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

            if (currentLineWidth > maxLineWidth) maxLineWidth = currentLineWidth;

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

            var optimalSize = Math.Min(widthLimitedSize, heightLimitedSize);

            hasValidLayoutData = false;
            lastLayoutWidth = -1;
            lastLayoutFontSize = -1;
            lastLayoutMaxHeight = -1;

            return Math.Clamp(optimalSize, minSize, maxSize);
        }

        hasValidLayoutData = false;
        lastLayoutWidth = -1;
        lastLayoutFontSize = -1;
        lastLayoutMaxHeight = -1;

        const float tolerance = 0.5f;
        var lo = minSize;
        var hi = maxSize;

        var minHeight = GetHeightForFontSize(lo, targetWidth, baseSettings, buf);
        if (minHeight > targetHeight)
            return minSize;

        var maxHeight = GetHeightForFontSize(hi, targetWidth, baseSettings, buf);
        if (maxHeight <= targetHeight)
            return maxSize;

        while (hi - lo > tolerance)
        {
            var mid = (lo + hi) * 0.5f;
            var height = GetHeightForFontSize(mid, targetWidth, baseSettings, buf);

            if (height <= targetHeight)
                lo = mid;
            else
                hi = mid;
        }

        return lo;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetHeightForFontSize(float fontSize, float targetWidth, TextProcessSettings baseSettings,
        CommonData buf)
    {
        var glyphScale = buf.GetGlyphScale(fontSize);
        var effectiveMaxWidth = baseSettings.enableWordWrap ? targetWidth / glyphScale : TextProcessSettings.FloatMax;

        buf.lineCount = 0;
        buf.orderedRunCount = 0;
        buf.positionedGlyphCount = 0;

        BreakLines(effectiveMaxWidth);

        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(fontSize, out var ascender, out var descender, out var lineHeight);
            return UniTextFontProvider.CalculateTextHeight(ascender, descender, buf.lineCount, lineHeight,
                baseSettings.LineSpacing);
        }

        return buf.lineCount * fontSize * 1.2f;
    }

    public float ResultWidth => resultWidth;
    public float ResultHeight => resultHeight;
    public Vector2 ResultSize => new(resultWidth, resultHeight);

    public ReadOnlySpan<PositionedGlyph> PositionedGlyphs
    {
        get
        {
            var b = CommonData.Current;
            return b.positionedGlyphs.AsSpan(0, b.positionedGlyphCount);
        }
    }

    public ReadOnlySpan<int> Codepoints
    {
        get
        {
            var b = CommonData.Current;
            return b.codepoints.AsSpan(0, b.codepointCount);
        }
    }

    private void Parse(ReadOnlySpan<char> text)
    {
        var buf = CommonData.Current;
        buf.codepointCount = 0;
        buf.EnsureCodepointCapacity(text.Length);

        var i = 0;
        while (i < text.Length) AddCharacter(text, ref i, buf);
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddCharacter(ReadOnlySpan<char> text, ref int i, CommonData buf)
    {
        var c = text[i];

        if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
        {
            var cp = char.ConvertToUtf32(c, text[i + 1]);
            AddCodepoint(cp, buf);
            i += 2;
        }
        else
        {
            AddCodepoint(c, buf);
            i++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddCodepoint(int cp, CommonData buf)
    {
        var count = buf.codepointCount;
        if (count >= buf.codepoints.Length)
            buf.EnsureCodepointCapacity(count + 1);
        buf.codepoints[count] = cp;
        buf.codepointCount = count + 1;
    }

    private void AnalyzeBidi(TextDirection requestedDirection)
    {
        var buf = CommonData.Current;
        var cpCount = buf.codepointCount;
        buf.EnsureBidiCapacity(cpCount);

        var direction = requestedDirection switch
        {
            TextDirection.RightToLeft => BidiParagraphDirection.RightToLeft,
            TextDirection.LeftToRight => BidiParagraphDirection.LeftToRight,
            _ => BidiParagraphDirection.Auto
        };

        var result = BidiEngine.ProcessPooled(buf.codepoints.AsSpan(0, cpCount), direction);

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
            result.ParagraphsSpan.CopyTo(buf.bidiParagraphs);
        }

        buf.bidiParagraphCount = paragraphCount;

        buf.baseDirection = result.Direction == BidiDirection.RightToLeft
            ? TextDirection.RightToLeft
            : TextDirection.LeftToRight;
    }

    private void AnalyzeScripts()
    {
        var buf = CommonData.Current;
        var cpCount = buf.codepointCount;
        buf.EnsureScriptCapacity(cpCount);
        ScriptAnalyzer.Analyze(buf.codepoints.AsSpan(0, cpCount), buf.scripts);
    }

    private void Itemize()
    {
        var buf = CommonData.Current;
        buf.runCount = 0;

        var cpCount = buf.codepointCount;
        if (cpCount == 0) return;

        var cpSpan = buf.codepoints.AsSpan(0, cpCount);
        var lvlSpan = buf.bidiLevels.AsSpan(0, cpCount);
        var scrSpan = buf.scripts.AsSpan(0, cpCount);
        var fp = fontProvider;
        var baseFont = baseFontId;

        if (fp == null)
        {
            ItemizeWithoutFontLookup(cpCount, lvlSpan, scrSpan, baseFont, buf);
            return;
        }

        var lastLookupCodepoint = -1;
        var lastLookupResult = baseFont;

        var runStart = 0;
        var currentLevel = lvlSpan[0];
        var currentScript = scrSpan[0];

        var cp0 = cpSpan[0];
        lastLookupCodepoint = cp0;
        lastLookupResult = fp.FindFontForCodepoint(cp0, baseFont);
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
                AddRun(runStart, i - runStart, currentLevel, currentScript, currentFontId, buf);
                runStart = i;
                currentLevel = level;
                currentScript = script;
                currentFontId = fontId;
            }
        }

        AddRun(runStart, cpCount - runStart, currentLevel, currentScript, currentFontId, buf);
    }

    private static void ItemizeWithoutFontLookup(int cpCount, Span<byte> lvlSpan, Span<UnicodeScript> scrSpan,
        int fontId, CommonData buf)
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
                AddRun(runStart, i - runStart, currentLevel, currentScript, fontId, buf);
                runStart = i;
                currentLevel = level;
                currentScript = script;
            }
        }

        AddRun(runStart, cpCount - runStart, currentLevel, currentScript, fontId, buf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddRun(int start, int length, byte bidiLevel, UnicodeScript script, int fontId, CommonData buf)
    {
        var count = buf.runCount;
        if (count >= buf.runs.Length)
            buf.EnsureRunCapacity(count + 1);

        buf.runs[count] = new TextRun
        {
            range = new TextRange(start, length),
            bidiLevel = bidiLevel,
            script = script,
            fontId = fontId
        };
        buf.runCount = count + 1;
    }

    private void Shape()
    {
        var buf = CommonData.Current;
        buf.shapedRunCount = 0;
        buf.shapedGlyphCount = 0;

        var cpCount = buf.codepointCount;
        var runCnt = buf.runCount;
        var cp = buf.codepoints.AsSpan(0, cpCount);
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

            var glyphStart = buf.shapedGlyphCount;
            AddShapedGlyphs(result.Glyphs, buf);

            AddShapedRun(new ShapedRun
            {
                range = run.range,
                glyphStart = glyphStart,
                glyphCount = result.Glyphs.Length,
                width = result.TotalAdvance,
                direction = run.Direction,
                bidiLevel = run.bidiLevel,
                fontId = run.fontId
            }, buf);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddShapedGlyphs(ReadOnlySpan<ShapedGlyph> glyphs, CommonData buf)
    {
        var count = buf.shapedGlyphCount;
        var required = count + glyphs.Length;
        if (buf.shapedGlyphs.Length < required)
            buf.EnsureShapedGlyphCapacity(required);

        glyphs.CopyTo(buf.shapedGlyphs.AsSpan(count));
        buf.shapedGlyphCount = required;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddShapedRun(ShapedRun run, CommonData buf)
    {
        var count = buf.shapedRunCount;
        if (count >= buf.shapedRuns.Length)
            buf.EnsureShapedRunCapacity(count + 1);

        buf.shapedRuns[count] = run;
        buf.shapedRunCount = count + 1;
    }


    private void EnsureGlyphsInAtlas()
    {
        if (fontProvider == null)
            return;

        var buf = CommonData.Current;
        fontProvider.EnsureGlyphsInAtlas(
            buf.shapedRuns.AsSpan(0, buf.shapedRunCount),
            buf.shapedGlyphs.AsSpan(0, buf.shapedGlyphCount));
    }

    private void BreakLines(float maxWidth)
    {
        Profiler.BeginSample("TextProcessor.BreakLines");
        var buf = CommonData.Current;
        buf.lineCount = 0;
        buf.orderedRunCount = 0;

        var linesArr = buf.lines;
        var orderedRunsArr = buf.orderedRuns;
        var lineCnt = buf.lineCount;
        var orderedRunCnt = buf.orderedRunCount;

        LineBreaker.BreakLines(
            buf.codepoints.AsSpan(0, buf.codepointCount),
            buf.shapedRuns.AsSpan(0, buf.shapedRunCount),
            buf.shapedGlyphs.AsSpan(0, buf.shapedGlyphCount),
            maxWidth,
            buf.bidiParagraphs,
            buf.bidiParagraphCount,
            ref linesArr, ref lineCnt,
            ref orderedRunsArr, ref orderedRunCnt);

        buf.lines = linesArr;
        buf.orderedRuns = orderedRunsArr;
        buf.lineCount = lineCnt;
        buf.orderedRunCount = orderedRunCnt;
        Profiler.EndSample();
    }

    private void LayoutText(TextProcessSettings settings)
    {
        Profiler.BeginSample("TextProcessor.LayoutText");
        var buf = CommonData.Current;
        buf.positionedGlyphCount = 0;
        buf.EnsurePositionedGlyphCapacity(buf.shapedGlyphCount);

        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(settings.fontSize, out var ascender, out var descender, out var lineHeight);
            var scale = fontProvider.GetScale(baseFontId, settings.fontSize);
            Layout.SetFontMetrics(ascender, descender, lineHeight, scale, buf.GetGlyphScale(settings.fontSize));
        }

        Layout.SetLayoutSettings(settings.layout);

        var glyphCnt = buf.positionedGlyphCount;
        Layout.Layout(
            buf.lines.AsSpan(0, buf.lineCount),
            buf.orderedRuns.AsSpan(0, buf.orderedRunCount),
            buf.shapedGlyphs.AsSpan(0, buf.shapedGlyphCount),
            buf.positionedGlyphs, ref glyphCnt,
            out resultWidth, out resultHeight);
        buf.positionedGlyphCount = glyphCnt;
        Profiler.EndSample();
    }
}