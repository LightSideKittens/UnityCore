using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

public struct TextProcessSettings
{
    /// <summary>
    /// Safe maximum value for UI dimensions. Using float.MaxValue causes integer overflow in Unity UI batching.
    /// This matches TMP_Math.FLOAT_MAX convention.
    /// </summary>
    public const float FloatMax = 32767f;

    public LayoutSettings layout;
    public float fontSize;
    public TextDirection baseDirection;
    public bool enableWordWrap;

    // Convenience accessors for common layout properties
    public float maxWidth { get => layout.maxWidth; set => layout.maxWidth = value; }
    public float maxHeight { get => layout.maxHeight; set => layout.maxHeight = value; }
    public HorizontalAlignment horizontalAlignment { get => layout.horizontalAlignment; set => layout.horizontalAlignment = value; }
    public VerticalAlignment verticalAlignment { get => layout.verticalAlignment; set => layout.verticalAlignment = value; }
    public float lineSpacing { get => layout.lineSpacing; set => layout.lineSpacing = value; }

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
    // Use shared static components to avoid per-instance allocations
    private BidiEngine BidiEngine => SharedPipelineComponents.BidiEngine;
    private ScriptAnalyzer ScriptAnalyzer => SharedPipelineComponents.ScriptAnalyzer;
    private IShapingEngine ShapingEngine => SharedPipelineComponents.ShapingEngine;
    private LineBreaker LineBreaker => SharedPipelineComponents.LineBreaker;
    private TextLayout Layout => SharedPipelineComponents.Layout;

    private UniTextFontProvider fontProvider;
    private int baseFontId;
    private float resultWidth;
    private float resultHeight;

    // Track if shaping data is valid for layout-only rebuilds
    private bool hasValidShapingData;

    // Track last layout parameters to skip redundant layout
    private float lastLayoutWidth = -1;
    private float lastLayoutFontSize = -1;
    private float lastLayoutMaxHeight = -1;
    private bool hasValidLayoutData;

    // DEBUG: Enable detailed logging for Arabic text issues
    public static bool DebugLogging = false;
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

    /// <summary>
    /// Process text with specified dirty flags to skip unnecessary steps.
    /// </summary>
    /// <param name="text">Text to process</param>
    /// <param name="settings">Processing settings</param>
    public ReadOnlySpan<PositionedGlyph> Process(
        ReadOnlySpan<char> text,
        TextProcessSettings settings)
    {
        // hasValidShapingData has priority - if shaping was done in EnsureShaping(), reuse it
        bool fullRebuild = !hasValidShapingData;

        var buf = CommonData.Current;

        // Check if layout can be reused BEFORE resetting buffers
        // Note: maxHeight check handles infinity (FloatMax) specially
        bool heightMatches = (float.IsInfinity(lastLayoutMaxHeight) && float.IsInfinity(settings.maxHeight)) ||
                             Math.Abs(lastLayoutMaxHeight - settings.maxHeight) < 0.001f;
        bool canReuseLayout = !fullRebuild &&
                              hasValidLayoutData &&
                              Math.Abs(lastLayoutWidth - settings.maxWidth) < 0.001f &&
                              Math.Abs(lastLayoutFontSize - settings.fontSize) < 0.001f &&
                              heightMatches &&
                              buf.positionedGlyphCount > 0;

        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log($"[Process] fullRebuild={fullRebuild}, canReuseLayout={canReuseLayout}, hasValidShapingData={hasValidShapingData}, hasValidLayoutData={hasValidLayoutData}");

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
        {
            if (!DoFullShaping(text, settings))
                return ReadOnlySpan<PositionedGlyph>.Empty;
        }

        if (!canReuseLayout)
        {
            float glyphScale = buf.GetGlyphScale(settings.fontSize);
            float effectiveMaxWidth = settings.enableWordWrap ? settings.maxWidth / glyphScale : TextProcessSettings.FloatMax;
            BreakLines(effectiveMaxWidth);
            LayoutText(settings);
        }

        return buf.positionedGlyphs.AsSpan(0, buf.positionedGlyphCount);
    }

    /// <summary>
    /// Check if shaping data is valid for layout-only rebuild.
    /// </summary>
    public bool HasValidShapingData => hasValidShapingData;

    /// <summary>
    /// Invalidate shaping data (call when text/font/direction changes).
    /// </summary>
    public void InvalidateShapingData()
    {
        hasValidShapingData = false;
        hasValidLayoutData = false;
        lastLayoutWidth = -1;
        lastLayoutFontSize = -1;
        lastLayoutMaxHeight = -1;
    }

    /// <summary>
    /// Ensure shaping is done without layout (for ILayoutElement.preferredWidth).
    /// This caches the shaping result for subsequent GetUnwrappedWidth() and GetHeightForWidth() calls.
    /// </summary>
    public void EnsureShaping(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log($"[EnsureShaping] hasValidShapingData={hasValidShapingData}");

        if (hasValidShapingData) return;

        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log("[EnsureShaping] DOING SHAPING");

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

    /// <summary>
    /// Full shaping pipeline: Parse → BiDi → Scripts → Itemize → Shape → Atlas.
    /// Returns false if no codepoints to process.
    /// </summary>
    private bool DoFullShaping(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        var buf = CommonData.Current;

        // Set shapingFontSize BEFORE Parsed event so modifiers can use it
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

    /// <summary>
    /// Get text width without word wrap (sum of all run widths).
    /// Call EnsureShaping() first.
    /// </summary>
    public float GetUnwrappedWidth()
    {
        if (!hasValidShapingData) return 0;

        var buf = CommonData.Current;
        float total = 0;
        int count = buf.shapedRunCount;
        for (int i = 0; i < count; i++)
            total += buf.shapedRuns[i].width;
        return total;
    }

    /// <summary>
    /// Get max line width (accounting for explicit line breaks).
    /// Returns the width of the widest line. Call EnsureShaping() first.
    /// </summary>
    public float GetMaxLineWidth()
    {
        if (!hasValidShapingData) return 0;

        var buf = CommonData.Current;
        var codepoints = buf.codepoints.AsSpan(0, buf.codepointCount);
        var glyphs = buf.shapedGlyphs.AsSpan(0, buf.shapedGlyphCount);
        var margins = buf.startMargins;

        float maxWidth = 0f;
        float currentWidth = 0f;
        int lastCluster = -1;
        int lineStartCluster = 0;

        // Iterate over glyphs, not codepoints - HarfBuzz may produce different glyph count
        for (int i = 0; i < glyphs.Length; i++)
        {
            int cluster = glyphs[i].cluster;

            // Check if this cluster's codepoint is a line break
            if (cluster != lastCluster && cluster < codepoints.Length)
            {
                int cp = codepoints[cluster];
                if (UnicodeData.IsLineBreak(cp))
                {
                    // Add margin to line width (for list markers, etc.)
                    float lineMargin = lineStartCluster < margins.Length ? margins[lineStartCluster] : 0;
                    float totalLineWidth = currentWidth + lineMargin;
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

        // Account for margin on last line
        float lastLineMargin = lineStartCluster < margins.Length ? margins[lineStartCluster] : 0;
        float lastLineWidth = currentWidth + lastLineMargin;
        if (lastLineWidth > maxWidth) maxWidth = lastLineWidth;

        return maxWidth > 0 ? maxWidth : GetUnwrappedWidth();
    }
    
    /// <summary>
    /// Get text height for a given width (performs line breaking and layout).
    /// Call EnsureShaping() first. Caches result for same width.
    /// </summary>
    public float GetHeightForWidth(float width, TextProcessSettings settings)
    {
        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log($"[GetHeightForWidth] width={width}, hasValidShapingData={hasValidShapingData}, hasValidLayoutData={hasValidLayoutData}");

        if (!hasValidShapingData) return 0;

        // Кэш: если layout уже сделан для этой ширины, вернуть результат
        if (hasValidLayoutData && Math.Abs(lastLayoutWidth - settings.maxWidth) < 0.001f)
        {
            if (CommonData.DebugPipelineLogging)
                UnityEngine.Debug.Log("[GetHeightForWidth] REUSING CACHED LAYOUT");
            return resultHeight;
        }

        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log("[GetHeightForWidth] DOING LAYOUT");

        var buf = CommonData.Current;
        float glyphScale = buf.GetGlyphScale(settings.fontSize);
        float effectiveMaxWidth = settings.enableWordWrap ? width / glyphScale : TextProcessSettings.FloatMax;

        BreakLines(effectiveMaxWidth);
        LayoutText(settings);

        // Cache layout for Process() to reuse
        lastLayoutWidth = settings.maxWidth;
        lastLayoutFontSize = settings.fontSize;
        lastLayoutMaxHeight = settings.maxHeight;
        hasValidLayoutData = true;

        return resultHeight;
    }

    /// <summary>
    /// Find optimal font size that fits text within target dimensions.
    /// Call EnsureShaping() first with base fontSize.
    /// Uses binary search - shaping is done once, only line breaking + layout per iteration.
    /// </summary>
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

        // Fast path: no word wrap or text fits without wrapping
        float unwrappedWidth = GetUnwrappedWidth();
        float maxGlyphScale = maxSize / buf.shapingFontSize;
        float scaledUnwrappedWidth = unwrappedWidth * maxGlyphScale;

        if (!baseSettings.enableWordWrap || scaledUnwrappedWidth <= targetWidth)
        {
            // Count explicit line breaks and find max line width
            // Use codepoints for accurate line counting (handles empty lines)
            int lineCount = 1;
            float maxLineWidth = 0f;
            float currentLineWidth = 0f;
            var codepoints = buf.codepoints.AsSpan(0, buf.codepointCount);
            var glyphs = buf.shapedGlyphs.AsSpan(0, buf.shapedGlyphCount);
            int glyphIdx = 0;

            for (int i = 0; i < codepoints.Length; i++)
            {
                int cp = codepoints[i];
                if (UnicodeData.IsLineBreak(cp))
                {
                    // Explicit line break - always count as new line
                    if (currentLineWidth > maxLineWidth) maxLineWidth = currentLineWidth;
                    currentLineWidth = 0f;
                    lineCount++;
                    // Skip corresponding zero-advance glyph if any
                    if (glyphIdx < glyphs.Length && glyphs[glyphIdx].advanceX == 0f)
                        glyphIdx++;
                }
                else if (cp == '\r')
                {
                    // Skip \r (part of \r\n on Windows, handled by \n)
                    if (glyphIdx < glyphs.Length && glyphs[glyphIdx].advanceX == 0f)
                        glyphIdx++;
                }
                else
                {
                    // Regular character - add glyph advance
                    if (glyphIdx < glyphs.Length)
                    {
                        currentLineWidth += glyphs[glyphIdx].advanceX;
                        glyphIdx++;
                    }
                }
            }
            // Don't forget last line
            if (currentLineWidth > maxLineWidth) maxLineWidth = currentLineWidth;

            // Fallback: if maxLineWidth is 0 (all empty lines), use minimal width
            if (maxLineWidth <= 0f) maxLineWidth = 1f;

            // Width: use max line width (not total width)
            float widthLimitedSize = (targetWidth / maxLineWidth) * buf.shapingFontSize;

            // Height: account for actual line count (include descender for accurate height)
            float lineHeightRatio, ascenderRatio, descenderRatio;
            if (fontProvider != null)
            {
                fontProvider.GetLineMetrics(1f, out float asc, out float desc, out float lh);
                lineHeightRatio = lh;
                ascenderRatio = asc;
                descenderRatio = desc; // negative value
            }
            else
            {
                lineHeightRatio = 1.2f;
                ascenderRatio = lineHeightRatio * 0.8f;
                descenderRatio = -lineHeightRatio * 0.2f;
            }
            float heightLimitedSize = targetHeight / ((ascenderRatio - descenderRatio) + (lineCount - 1) * lineHeightRatio);

            float optimalSize = Math.Min(widthLimitedSize, heightLimitedSize);

            if (DebugLogging)
                UnityEngine.Debug.Log($"[FindOptimalFontSize] FAST PATH: lineCount={lineCount}, maxLineWidth={maxLineWidth:F1}, widthLimited={widthLimitedSize:F1}, heightLimited={heightLimitedSize:F1}, optimal={optimalSize:F1}, clamped=[{minSize},{maxSize}]");

            // Invalidate layout cache - fontSize may have changed
            hasValidLayoutData = false;
            lastLayoutWidth = -1;
            lastLayoutFontSize = -1;
            lastLayoutMaxHeight = -1;

            return Math.Clamp(optimalSize, minSize, maxSize);
        }

        // Multi-line case - binary search
        // Invalidate layout cache since we'll be testing different sizes
        hasValidLayoutData = false;
        lastLayoutWidth = -1;
        lastLayoutFontSize = -1;
        lastLayoutMaxHeight = -1;

        const float tolerance = 0.5f;
        float lo = minSize;
        float hi = maxSize;

        // First check if even minSize fits
        float minHeight = GetHeightForFontSize(lo, targetWidth, baseSettings, buf);
        if (minHeight > targetHeight)
            return minSize; // Text doesn't fit even at minimum size

        // Check if maxSize fits
        float maxHeight = GetHeightForFontSize(hi, targetWidth, baseSettings, buf);
        if (maxHeight <= targetHeight)
            return maxSize; // Text fits at maximum size

        // Binary search
        while (hi - lo > tolerance)
        {
            float mid = (lo + hi) * 0.5f;
            float height = GetHeightForFontSize(mid, targetWidth, baseSettings, buf);

            if (height <= targetHeight)
                lo = mid; // Can increase
            else
                hi = mid; // Need to decrease
        }

        return lo;
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private float GetHeightForFontSize(float fontSize, float targetWidth, TextProcessSettings baseSettings, CommonData buf)
    {
        float glyphScale = buf.GetGlyphScale(fontSize);
        float effectiveMaxWidth = baseSettings.enableWordWrap ? targetWidth / glyphScale : TextProcessSettings.FloatMax;

        // Reset layout buffers for new calculation
        buf.lineCount = 0;
        buf.orderedRunCount = 0;
        buf.positionedGlyphCount = 0;

        BreakLines(effectiveMaxWidth);

        // Calculate height from line count and font metrics
        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(fontSize, out float ascender, out float descender, out float lineHeight);
            return UniTextFontProvider.CalculateTextHeight(ascender, descender, buf.lineCount, lineHeight, baseSettings.lineSpacing);
        }

        // Fallback: estimate based on line count
        return buf.lineCount * fontSize * 1.2f;
    }

    public float ResultWidth => resultWidth;
    public float ResultHeight => resultHeight;
    public Vector2 ResultSize => new(resultWidth, resultHeight);
    public ReadOnlySpan<PositionedGlyph> PositionedGlyphs { get { var b = CommonData.Current; return b.positionedGlyphs.AsSpan(0, b.positionedGlyphCount); } }
    public ReadOnlySpan<int> Codepoints { get { var b = CommonData.Current; return b.codepoints.AsSpan(0, b.codepointCount); } }

    private void Parse(ReadOnlySpan<char> text)
    {
        var buf = CommonData.Current;
        buf.codepointCount = 0;
        buf.EnsureCodepointCapacity(text.Length);

        int i = 0;
        while (i < text.Length)
        {
            AddCharacter(text, ref i, buf);
        }
    }
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddCharacter(ReadOnlySpan<char> text, ref int i, CommonData buf)
    {
        char c = text[i];

        if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
        {
            int cp = char.ConvertToUtf32(c, text[i + 1]);
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
        int count = buf.codepointCount;
        if (count >= buf.codepoints.Length)
            buf.EnsureCodepointCapacity(count + 1);
        buf.codepoints[count] = cp;
        buf.codepointCount = count + 1;
    }

    private void AnalyzeBidi(TextDirection requestedDirection)
    {
        var buf = CommonData.Current;
        int cpCount = buf.codepointCount;
        buf.EnsureBidiCapacity(cpCount);

        var direction = requestedDirection switch
        {
            TextDirection.RightToLeft => BidiParagraphDirection.RightToLeft,
            TextDirection.LeftToRight => BidiParagraphDirection.LeftToRight,
            _ => BidiParagraphDirection.Auto
        };

        // Use ProcessPooled for zero-allocation fast path - we copy immediately so pooled buffer is safe
        var result = BidiEngine.ProcessPooled(buf.codepoints.AsSpan(0, cpCount), direction);

        if (result.levels != null && result.levels.Length > 0)
        {
            int copyLen = Math.Min(result.levels.Length, cpCount);
            result.levels.AsSpan(0, copyLen).CopyTo(buf.bidiLevels);
        }
        else
        {
            buf.bidiLevels.AsSpan(0, cpCount).Fill(0);
        }

        // Copy paragraphs to our own buffer (pooled source will be reused)
        int paragraphCount = result.paragraphCount;
        if (paragraphCount > 0)
        {
            buf.EnsureBidiParagraphCapacity(paragraphCount);
            result.ParagraphsSpan.CopyTo(buf.bidiParagraphs);
        }
        buf.bidiParagraphCount = paragraphCount;

        buf.baseDirection = result.Direction == BidiDirection.RightToLeft
            ? TextDirection.RightToLeft
            : TextDirection.LeftToRight;

        // DEBUG: Log BiDi analysis results
        if (DebugLogging)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[TextProcessor.AnalyzeBidi] baseDirection={buf.baseDirection}, paragraphs={buf.bidiParagraphCount}, levels: ");
            for (int j = 0; j < cpCount && j < 30; j++)
            {
                sb.Append(buf.bidiLevels[j]);
                sb.Append(' ');
            }
            if (cpCount > 30) sb.Append("...");
            UnityEngine.Debug.Log(sb.ToString());
        }
    }

    private void AnalyzeScripts()
    {
        var buf = CommonData.Current;
        int cpCount = buf.codepointCount;
        buf.EnsureScriptCapacity(cpCount);
        ScriptAnalyzer.Analyze(buf.codepoints.AsSpan(0, cpCount), buf.scripts);

        // DEBUG: Log script analysis results
        if (DebugLogging)
        {
            var scriptCounts = new System.Collections.Generic.Dictionary<UnicodeScript, int>();
            for (int j = 0; j < cpCount; j++)
            {
                var script = buf.scripts[j];
                if (!scriptCounts.ContainsKey(script))
                    scriptCounts[script] = 0;
                scriptCounts[script]++;
            }
            var sb = new System.Text.StringBuilder();
            sb.Append("[TextProcessor.AnalyzeScripts] Scripts detected: ");
            foreach (var kvp in scriptCounts)
            {
                sb.Append($"{kvp.Key}={kvp.Value}, ");
            }
            UnityEngine.Debug.Log(sb.ToString());
        }
    }

    private void Itemize()
    {
        var buf = CommonData.Current;
        buf.runCount = 0;

        int cpCount = buf.codepointCount;
        if (cpCount == 0) return;

        var cpSpan = buf.codepoints.AsSpan(0, cpCount);
        var lvlSpan = buf.bidiLevels.AsSpan(0, cpCount);
        var scrSpan = buf.scripts.AsSpan(0, cpCount);
        var fp = fontProvider;
        int baseFont = baseFontId;

        if (fp == null)
        {
            // Fast path: no font provider, single run per bidi/script change
            ItemizeWithoutFontLookup(cpCount, lvlSpan, scrSpan, baseFont, buf);
            return;
        }

        // Cache last font lookup to avoid redundant calls for same codepoint range
        int lastLookupCodepoint = -1;
        int lastLookupResult = baseFont;

        int runStart = 0;
        byte currentLevel = lvlSpan[0];
        var currentScript = scrSpan[0];

        int cp0 = cpSpan[0];
        lastLookupCodepoint = cp0;
        lastLookupResult = fp.FindFontForCodepoint(cp0, baseFont);
        int currentFontId = lastLookupResult;

        for (int i = 1; i < cpCount; i++)
        {
            byte level = lvlSpan[i];
            var script = scrSpan[i];

            // Only lookup font if bidi/script changed or it's a different codepoint
            int fontId;
            int cp = cpSpan[i];
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

    private static void ItemizeWithoutFontLookup(int cpCount, Span<byte> lvlSpan, Span<UnicodeScript> scrSpan, int fontId, CommonData buf)
    {
        int runStart = 0;
        byte currentLevel = lvlSpan[0];
        var currentScript = scrSpan[0];

        for (int i = 1; i < cpCount; i++)
        {
            byte level = lvlSpan[i];
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
        int count = buf.runCount;
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

        int cpCount = buf.codepointCount;
        int runCnt = buf.runCount;
        var cp = buf.codepoints.AsSpan(0, cpCount);
        var runs = buf.runs;

        for (int i = 0; i < runCnt; i++)
        {
            ref readonly var run = ref runs[i];
            var runCodepoints = cp.Slice(run.range.start, run.range.length);

            var result = ShapingEngine.Shape(
                runCodepoints,
                fontProvider,
                run.fontId,
                run.script,
                run.Direction);

            // DEBUG: Log shaping result for each run
            if (DebugLogging)
            {
                var sb = new System.Text.StringBuilder();
                sb.Append($"[TextProcessor.Shape] Run[{i}] script={run.script}, dir={run.Direction}: ");
                sb.Append($"{runCodepoints.Length} codepoints -> {result.Glyphs.Length} glyphs, totalAdvance={result.TotalAdvance:F2}\n");
                sb.Append("  Glyphs: ");
                int maxShow = Math.Min(result.Glyphs.Length, 20);
                for (int g = 0; g < maxShow; g++)
                {
                    var glyph = result.Glyphs[g];
                    sb.Append($"[id={glyph.glyphId}, adv={glyph.advanceX:F1}] ");
                }
                if (result.Glyphs.Length > maxShow)
                    sb.Append("...");
                UnityEngine.Debug.Log(sb.ToString());
            }

            int glyphStart = buf.shapedGlyphCount;
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

        // DEBUG: Log total shaping summary
        if (DebugLogging)
        {
            UnityEngine.Debug.Log($"[TextProcessor.Shape] Total: {buf.shapedRunCount} shaped runs, {buf.shapedGlyphCount} glyphs");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddShapedGlyphs(ReadOnlySpan<ShapedGlyph> glyphs, CommonData buf)
    {
        int count = buf.shapedGlyphCount;
        int required = count + glyphs.Length;
        if (buf.shapedGlyphs.Length < required)
            buf.EnsureShapedGlyphCapacity(required);

        glyphs.CopyTo(buf.shapedGlyphs.AsSpan(count));
        buf.shapedGlyphCount = required;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddShapedRun(ShapedRun run, CommonData buf)
    {
        int count = buf.shapedRunCount;
        if (count >= buf.shapedRuns.Length)
            buf.EnsureShapedRunCapacity(count + 1);

        buf.shapedRuns[count] = run;
        buf.shapedRunCount = count + 1;
    }

    /// <summary>
    /// Ensure all glyph indices from shaping are in the font atlases.
    /// Must be called after Shape() and before rendering.
    /// </summary>
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
        int lineCnt = buf.lineCount;
        int orderedRunCnt = buf.orderedRunCount;

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
            fontProvider.GetLineMetrics(settings.fontSize, out float ascender, out float descender, out float lineHeight);
            float scale = fontProvider.GetScale(baseFontId, settings.fontSize);
            Layout.SetFontMetrics(ascender, descender, lineHeight, scale, buf.GetGlyphScale(settings.fontSize));
        }

        Layout.SetLayoutSettings(settings.layout);

        int glyphCnt = buf.positionedGlyphCount;
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
