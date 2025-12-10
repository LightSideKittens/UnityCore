using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct TextProcessSettings
{
    /// <summary>
    /// Safe maximum value for UI dimensions. Using float.MaxValue causes integer overflow in Unity UI batching.
    /// This matches TMP_Math.FLOAT_MAX convention.
    /// </summary>
    public const float FloatMax = 32767f;

    public float maxWidth;
    public float maxHeight;
    public float fontSize;
    public TextDirection baseDirection;
    public bool enableWordWrap;
    public HorizontalAlignment horizontalAlignment;
    public VerticalAlignment verticalAlignment;
    public float lineSpacing;

    public static TextProcessSettings Default => new()
    {
        maxWidth = FloatMax,
        maxHeight = FloatMax,
        fontSize = 36f,
        baseDirection = TextDirection.LeftToRight,
        enableWordWrap = true,
        horizontalAlignment = HorizontalAlignment.Left,
        verticalAlignment = VerticalAlignment.Top,
        lineSpacing = 0
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

    // DEBUG: Enable detailed logging for Arabic text issues
    public static bool DebugLogging = false;
    public event Action Parsed;

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
    /// <param name="dirtyFlags">Which parts need rebuilding.</param>
    public ReadOnlySpan<PositionedGlyph> Process(
        ReadOnlySpan<char> text,
        TextProcessSettings settings,
        UniText.DirtyFlags dirtyFlags = UniText.DirtyFlags.FullRebuild)
    {
        bool fullRebuild = (dirtyFlags & UniText.DirtyFlags.FullRebuild) != 0 ||
                           (dirtyFlags & UniText.DirtyFlags.FontSize) != 0 ||
                           !hasValidShapingData;

        if (fullRebuild)
        {
            // Full rebuild - reset everything
            SharedTextBuffers.Reset();
            hasValidShapingData = false;
        }
        else
        {
            // Partial rebuild - only reset layout buffers
            SharedTextBuffers.lineCount = 0;
            SharedTextBuffers.orderedRunCount = 0;
            SharedTextBuffers.positionedGlyphCount = 0;
        }

        resultWidth = 0;
        resultHeight = 0;

        if (text.IsEmpty)
        {
            hasValidShapingData = false;
            return ReadOnlySpan<PositionedGlyph>.Empty;
        }

        fontProvider?.SetFontSize(settings.fontSize);

        if (fullRebuild)
        {
            // Full pipeline
            Parse(text);
            Parsed?.Invoke();
            
            if (SharedTextBuffers.codepointCount == 0)
            {
                hasValidShapingData = false;
                return ReadOnlySpan<PositionedGlyph>.Empty;
            }
            
            AnalyzeBidi(settings.baseDirection);
            AnalyzeScripts();

            Itemize();
            Shape();

            EnsureGlyphsInAtlas();

            hasValidShapingData = true;
        }
        // else: use existing shaping data from SharedTextBuffers

        // Always do layout (depends on width/height/alignment)
        BreakLines(settings.enableWordWrap ? settings.maxWidth : TextProcessSettings.FloatMax);
        LayoutText(settings);

        return SharedTextBuffers.positionedGlyphs.AsSpan(0, SharedTextBuffers.positionedGlyphCount);
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
    }

    public float ResultWidth => resultWidth;
    public float ResultHeight => resultHeight;
    public Vector2 ResultSize => new(resultWidth, resultHeight);
    public ReadOnlySpan<PositionedGlyph> PositionedGlyphs => SharedTextBuffers.positionedGlyphs.AsSpan(0, SharedTextBuffers.positionedGlyphCount);
    public ReadOnlySpan<int> Codepoints => SharedTextBuffers.codepoints.AsSpan(0, SharedTextBuffers.codepointCount);

    private void Parse(ReadOnlySpan<char> text)
    {
        SharedTextBuffers.codepointCount = 0;

        SharedTextBuffers.EnsureCodepointCapacity(text.Length);

        int i = 0;
        while (i < text.Length)
        {
            AddCharacter(text, ref i);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddCharacter(ReadOnlySpan<char> text, ref int i)
    {
        char c = text[i];

        if (char.IsHighSurrogate(c) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
        {
            int cp = char.ConvertToUtf32(c, text[i + 1]);
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
    private static void AddCodepoint(int cp)
    {
        int count = SharedTextBuffers.codepointCount;
        if (count >= SharedTextBuffers.codepoints.Length)
            SharedTextBuffers.EnsureCodepointCapacity(count + 1);
        SharedTextBuffers.codepoints[count] = cp;
        SharedTextBuffers.codepointCount = count + 1;
    }

    private void AnalyzeBidi(TextDirection requestedDirection)
    {
        int cpCount = SharedTextBuffers.codepointCount;
        SharedTextBuffers.EnsureBidiCapacity(cpCount);

        var direction = requestedDirection switch
        {
            TextDirection.RightToLeft => BidiParagraphDirection.RightToLeft,
            TextDirection.LeftToRight => BidiParagraphDirection.LeftToRight,
            _ => BidiParagraphDirection.Auto
        };

        // Use ProcessPooled for zero-allocation fast path - we copy immediately so pooled buffer is safe
        var result = BidiEngine.ProcessPooled(SharedTextBuffers.codepoints.AsSpan(0, cpCount), direction);

        if (result.levels != null && result.levels.Length > 0)
        {
            int copyLen = Math.Min(result.levels.Length, cpCount);
            result.levels.AsSpan(0, copyLen).CopyTo(SharedTextBuffers.bidiLevels);
        }
        else
        {
            SharedTextBuffers.bidiLevels.AsSpan(0, cpCount).Fill(0);
        }

        SharedTextBuffers.bidiParagraphs = result.paragraphs ?? Array.Empty<BidiParagraph>();
        SharedTextBuffers.baseDirection = result.Direction == BidiDirection.RightToLeft
            ? TextDirection.RightToLeft
            : TextDirection.LeftToRight;

        // DEBUG: Log BiDi analysis results
        if (DebugLogging)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[TextProcessor.AnalyzeBidi] baseDirection={SharedTextBuffers.baseDirection}, paragraphs={SharedTextBuffers.bidiParagraphs.Length}, levels: ");
            for (int j = 0; j < cpCount && j < 30; j++)
            {
                sb.Append(SharedTextBuffers.bidiLevels[j]);
                sb.Append(' ');
            }
            if (cpCount > 30) sb.Append("...");
            UnityEngine.Debug.Log(sb.ToString());
        }
    }

    private void AnalyzeScripts()
    {
        int cpCount = SharedTextBuffers.codepointCount;
        SharedTextBuffers.EnsureScriptCapacity(cpCount);
        ScriptAnalyzer.Analyze(SharedTextBuffers.codepoints.AsSpan(0, cpCount), SharedTextBuffers.scripts);

        // DEBUG: Log script analysis results
        if (DebugLogging)
        {
            var scriptCounts = new System.Collections.Generic.Dictionary<UnicodeScript, int>();
            for (int j = 0; j < cpCount; j++)
            {
                var script = SharedTextBuffers.scripts[j];
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
        SharedTextBuffers.runCount = 0;

        int cpCount = SharedTextBuffers.codepointCount;
        if (cpCount == 0) return;

        var cpSpan = SharedTextBuffers.codepoints.AsSpan(0, cpCount);
        var lvlSpan = SharedTextBuffers.bidiLevels.AsSpan(0, cpCount);
        var scrSpan = SharedTextBuffers.scripts.AsSpan(0, cpCount);
        var fp = fontProvider;
        int baseFont = baseFontId;

        if (fp == null)
        {
            // Fast path: no font provider, single run per bidi/script change
            ItemizeWithoutFontLookup(cpCount, lvlSpan, scrSpan, baseFont);
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
                AddRun(runStart, i - runStart, currentLevel, currentScript, currentFontId);
                runStart = i;
                currentLevel = level;
                currentScript = script;
                currentFontId = fontId;
            }
        }

        AddRun(runStart, cpCount - runStart, currentLevel, currentScript, currentFontId);
    }

    private static void ItemizeWithoutFontLookup(int cpCount, Span<byte> lvlSpan, Span<UnicodeScript> scrSpan, int fontId)
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
                AddRun(runStart, i - runStart, currentLevel, currentScript, fontId);
                runStart = i;
                currentLevel = level;
                currentScript = script;
            }
        }

        AddRun(runStart, cpCount - runStart, currentLevel, currentScript, fontId);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddRun(int start, int length, byte bidiLevel, UnicodeScript script, int fontId)
    {
        int count = SharedTextBuffers.runCount;
        if (count >= SharedTextBuffers.runs.Length)
            SharedTextBuffers.EnsureRunCapacity(count + 1);

        SharedTextBuffers.runs[count] = new TextRun
        {
            range = new TextRange(start, length),
            bidiLevel = bidiLevel,
            script = script,
            fontId = fontId
        };
        SharedTextBuffers.runCount = count + 1;
    }

    private void Shape()
    {
        SharedTextBuffers.shapedRunCount = 0;
        SharedTextBuffers.shapedGlyphCount = 0;

        int cpCount = SharedTextBuffers.codepointCount;
        int runCnt = SharedTextBuffers.runCount;
        var cp = SharedTextBuffers.codepoints.AsSpan(0, cpCount);
        var runs = SharedTextBuffers.runs;

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

            int glyphStart = SharedTextBuffers.shapedGlyphCount;
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

        // DEBUG: Log total shaping summary
        if (DebugLogging)
        {
            UnityEngine.Debug.Log($"[TextProcessor.Shape] Total: {SharedTextBuffers.shapedRunCount} shaped runs, {SharedTextBuffers.shapedGlyphCount} glyphs");
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddShapedGlyphs(ReadOnlySpan<ShapedGlyph> glyphs)
    {
        int count = SharedTextBuffers.shapedGlyphCount;
        int required = count + glyphs.Length;
        if (SharedTextBuffers.shapedGlyphs.Length < required)
            SharedTextBuffers.EnsureShapedGlyphCapacity(required);

        glyphs.CopyTo(SharedTextBuffers.shapedGlyphs.AsSpan(count));
        SharedTextBuffers.shapedGlyphCount = required;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void AddShapedRun(ShapedRun run)
    {
        int count = SharedTextBuffers.shapedRunCount;
        if (count >= SharedTextBuffers.shapedRuns.Length)
            SharedTextBuffers.EnsureShapedRunCapacity(count + 1);

        SharedTextBuffers.shapedRuns[count] = run;
        SharedTextBuffers.shapedRunCount = count + 1;
    }

    /// <summary>
    /// Ensure all glyph indices from shaping are in the font atlases.
    /// Must be called after Shape() and before rendering.
    /// </summary>
    private void EnsureGlyphsInAtlas()
    {
        if (fontProvider == null)
            return;

        fontProvider.EnsureGlyphsInAtlas(
            SharedTextBuffers.shapedRuns.AsSpan(0, SharedTextBuffers.shapedRunCount),
            SharedTextBuffers.shapedGlyphs.AsSpan(0, SharedTextBuffers.shapedGlyphCount));
    }

    private void BreakLines(float maxWidth)
    {
        SharedTextBuffers.lineCount = 0;
        SharedTextBuffers.orderedRunCount = 0;

        var linesArr = SharedTextBuffers.lines;
        var orderedRunsArr = SharedTextBuffers.orderedRuns;
        int lineCnt = SharedTextBuffers.lineCount;
        int orderedRunCnt = SharedTextBuffers.orderedRunCount;

        LineBreaker.BreakLines(
            SharedTextBuffers.codepoints.AsSpan(0, SharedTextBuffers.codepointCount),
            SharedTextBuffers.shapedRuns.AsSpan(0, SharedTextBuffers.shapedRunCount),
            SharedTextBuffers.shapedGlyphs.AsSpan(0, SharedTextBuffers.shapedGlyphCount),
            maxWidth,
            SharedTextBuffers.bidiParagraphs,
            ref linesArr, ref lineCnt,
            ref orderedRunsArr, ref orderedRunCnt);

        SharedTextBuffers.lines = linesArr;
        SharedTextBuffers.orderedRuns = orderedRunsArr;
        SharedTextBuffers.lineCount = lineCnt;
        SharedTextBuffers.orderedRunCount = orderedRunCnt;
    }

    private void LayoutText(TextProcessSettings settings)
    {
        SharedTextBuffers.positionedGlyphCount = 0;
        SharedTextBuffers.EnsurePositionedGlyphCapacity(SharedTextBuffers.shapedGlyphCount);

        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(settings.fontSize, out float ascender, out float descender, out float lineHeight);
            float scale = fontProvider.GetScale(baseFontId, settings.fontSize);
            Layout.SetFontMetrics(ascender, descender, lineHeight, scale);
        }

        var layoutSettings = new LayoutSettings
        {
            maxWidth = settings.maxWidth,
            maxHeight = settings.maxHeight,
            lineSpacing = settings.lineSpacing,
            horizontalAlignment = settings.horizontalAlignment,
            verticalAlignment = settings.verticalAlignment
        };
        Layout.SetLayoutSettings(layoutSettings);

        int glyphCnt = SharedTextBuffers.positionedGlyphCount;
        Layout.Layout(
            SharedTextBuffers.lines.AsSpan(0, SharedTextBuffers.lineCount),
            SharedTextBuffers.orderedRuns.AsSpan(0, SharedTextBuffers.orderedRunCount),
            SharedTextBuffers.shapedGlyphs.AsSpan(0, SharedTextBuffers.shapedGlyphCount),
            SharedTextBuffers.positionedGlyphs, ref glyphCnt,
            out resultWidth, out resultHeight);
        SharedTextBuffers.positionedGlyphCount = glyphCnt;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int FindTagEnd(ReadOnlySpan<char> text, int start)
    {
        int end = Math.Min(text.Length, start + 128);
        for (int i = start + 1; i < end; i++)
        {
            if (text[i] == '>')
                return i;
        }
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int SkipToTagEnd(ReadOnlySpan<char> text, int start)
    {
        for (int i = start; i < text.Length; i++)
        {
            if (text[i] == '>')
                return i;
        }
        return text.Length - 1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool MatchesClosingTag(ReadOnlySpan<char> text, int start, string tagName)
    {
        int tagLen = tagName.Length;
        int required = 3 + tagLen;
        if (start + required > text.Length)
            return false;

        if (text[start] != '<' || text[start + 1] != '/')
            return false;

        int offset = start + 2;
        for (int i = 0; i < tagLen; i++)
        {
            char c = text[offset + i];
            char t = tagName[i];
            if (c != t && (c | 0x20) != (t | 0x20))
                return false;
        }

        return text[offset + tagLen] == '>';
    }
}
