using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Настройки обработки текста
/// </summary>
public struct TextProcessSettings
{
    public float maxWidth;
    public float maxHeight;
    public float fontSize;
    public TextDirection baseDirection;
    public bool enableRichText;
    public bool enableWordWrap;
    public HorizontalAlignment horizontalAlignment;
    public VerticalAlignment verticalAlignment;
    public float lineSpacing;

    public static TextProcessSettings Default => new()
    {
        maxWidth = float.MaxValue,
        maxHeight = float.MaxValue,
        fontSize = 36f,
        baseDirection = TextDirection.LeftToRight,
        enableRichText = true,
        enableWordWrap = true,
        horizontalAlignment = HorizontalAlignment.Left,
        verticalAlignment = VerticalAlignment.Top,
        lineSpacing = 0
    };
}

/// <summary>
/// Главный координатор text processing pipeline.
/// Использует SharedTextBuffers для избежания аллокаций.
/// </summary>
public sealed class TextProcessor
{
    // Pipeline components
    private readonly RichTextParser parser;
    private readonly BidiEngine bidiEngine;
    private readonly ScriptAnalyzer scriptAnalyzer;
    private readonly Itemizer itemizer;  // Reserved for future use
    private readonly IShapingEngine shapingEngine;
    private readonly LineBreaker lineBreaker;
    private readonly TextLayout layout;

    // State
    private UniTextFontProvider fontProvider;
    private int baseFontId;
    private float resultWidth;
    private float resultHeight;

    /// <summary>
    /// Создать TextProcessor с использованием статического UnicodeData.
    /// </summary>
    public TextProcessor(TagRegistry tagRegistry = null)
    {
        var unicodeData = UnicodeData.Provider;
        if (unicodeData == null)
            throw new InvalidOperationException("UnicodeData not initialized. Call UnicodeData.EnsureInitialized() first.");

        parser = new RichTextParser(tagRegistry ?? TagRegistry.CreateDefault());
        bidiEngine = new BidiEngine();
        scriptAnalyzer = new ScriptAnalyzer();
        itemizer = new Itemizer();
        shapingEngine = new UniTextShapingEngine();
        lineBreaker = new LineBreaker();
        layout = new TextLayout();
    }

    /// <summary>
    /// Создать с custom shaping engine (для HarfBuzz).
    /// </summary>
    public TextProcessor(IShapingEngine shapingEngine, TagRegistry tagRegistry = null)
    {
        if (shapingEngine == null)
            throw new ArgumentNullException(nameof(shapingEngine));

        var unicodeData = UnicodeData.Provider;
        if (unicodeData == null)
            throw new InvalidOperationException("UnicodeData not initialized. Call UnicodeData.EnsureInitialized() first.");

        parser = new RichTextParser(tagRegistry ?? TagRegistry.CreateDefault());
        bidiEngine = new BidiEngine();
        scriptAnalyzer = new ScriptAnalyzer();
        itemizer = new Itemizer();
        this.shapingEngine = shapingEngine;
        lineBreaker = new LineBreaker();
        layout = new TextLayout();
    }

    /// <summary>
    /// Установить font provider.
    /// </summary>
    public void SetFontProvider(UniTextFontProvider provider, int defaultFontId = 0)
    {
        fontProvider = provider;
        baseFontId = defaultFontId;
    }

    /// <summary>
    /// Обработать текст.
    /// </summary>
    public ReadOnlySpan<PositionedGlyph> Process(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        // Сброс shared буферов перед обработкой
        SharedTextBuffers.Reset();
        resultWidth = 0;
        resultHeight = 0;

        if (text.IsEmpty)
        {
            return ReadOnlySpan<PositionedGlyph>.Empty;
        }

        if (fontProvider != null)
        {
            fontProvider.SetFontSize(settings.fontSize);
        }

        // 1. Parse
        if (settings.enableRichText)
            Parse(text);
        else
            ParsePlain(text);

        if (SharedTextBuffers.codepointCount == 0)
        {
            return ReadOnlySpan<PositionedGlyph>.Empty;
        }

        // 2. BiDi Analysis
        AnalyzeBidi(settings.baseDirection);

        // 3. Script Analysis
        AnalyzeScripts();

        // 4. Itemization
        Itemize();

        // 5. Shaping
        Shape();

        // 6. Line Breaking
        float maxWidth = settings.enableWordWrap ? settings.maxWidth : float.MaxValue;
        BreakLines(maxWidth);

        // 7. Layout
        LayoutText(settings);

        return SharedTextBuffers.positionedGlyphs.AsSpan(0, SharedTextBuffers.positionedGlyphCount);
    }

    /// <summary>
    /// Результаты последней обработки.
    /// </summary>
    public float ResultWidth => resultWidth;
    public float ResultHeight => resultHeight;
    public Vector2 ResultSize => new(resultWidth, resultHeight);
    public ReadOnlySpan<PositionedGlyph> PositionedGlyphs => SharedTextBuffers.positionedGlyphs.AsSpan(0, SharedTextBuffers.positionedGlyphCount);
    public ReadOnlySpan<int> Codepoints => SharedTextBuffers.codepoints.AsSpan(0, SharedTextBuffers.codepointCount);
    public IReadOnlyList<TextAttributeBase> Attributes => SharedTextBuffers.attributes;

    #region Pipeline Steps

    private void Parse(ReadOnlySpan<char> text)
    {
        SharedTextBuffers.codepointCount = 0;
        SharedTextBuffers.attributes.Clear();

        SharedTextBuffers.EnsureCodepointCapacity(text.Length);

        int i = 0;
        bool inNoparse = false;

        while (i < text.Length)
        {
            char c = text[i];

            if (c == '<' && !inNoparse)
            {
                int tagEnd = FindTagEnd(text, i);
                if (tagEnd > i)
                {
                    var tagContent = text.Slice(i + 1, tagEnd - i - 1);
                    if (parser.ProcessTagDirect(tagContent, SharedTextBuffers.codepointCount, SharedTextBuffers.attributes, AddCodepoint, out bool isNoparseOpen))
                    {
                        if (isNoparseOpen) inNoparse = true;
                        i = tagEnd + 1;
                        continue;
                    }
                }
            }
            else if (c == '<' && inNoparse)
            {
                if (MatchesClosingTag(text, i, "noparse"))
                {
                    inNoparse = false;
                    i = SkipToTagEnd(text, i) + 1;
                    continue;
                }
            }

            AddCharacter(text, ref i);
        }
    }

    private void ParsePlain(ReadOnlySpan<char> text)
    {
        SharedTextBuffers.codepointCount = 0;
        SharedTextBuffers.attributes.Clear();

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

        // Конвертируем TextDirection в BidiParagraphDirection
        var direction = requestedDirection switch
        {
            TextDirection.RightToLeft => BidiParagraphDirection.RightToLeft,
            TextDirection.LeftToRight => BidiParagraphDirection.LeftToRight,
            TextDirection.Auto => BidiParagraphDirection.Auto,
            _ => BidiParagraphDirection.Auto
        };

        var result = bidiEngine.Process(SharedTextBuffers.codepoints.AsSpan(0, cpCount), direction);

        if (result.levels != null && result.levels.Length > 0)
        {
            int copyLen = Math.Min(result.levels.Length, cpCount);
            result.levels.AsSpan(0, copyLen).CopyTo(SharedTextBuffers.bidiLevels);
        }
        else
        {
            SharedTextBuffers.bidiLevels.AsSpan(0, cpCount).Fill(0);
        }

        // Сохраняем информацию о параграфах (каждый имеет свой baseLevel)
        SharedTextBuffers.bidiParagraphs = result.paragraphs ?? Array.Empty<BidiParagraph>();

        // baseDirection первого параграфа (для совместимости)
        SharedTextBuffers.baseDirection = result.Direction == BidiDirection.RightToLeft
            ? TextDirection.RightToLeft
            : TextDirection.LeftToRight;
    }

    private void AnalyzeScripts()
    {
        int cpCount = SharedTextBuffers.codepointCount;
        SharedTextBuffers.EnsureScriptCapacity(cpCount);
        scriptAnalyzer.Analyze(SharedTextBuffers.codepoints.AsSpan(0, cpCount), SharedTextBuffers.scripts);
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

        int runStart = 0;
        byte currentLevel = lvlSpan[0];
        var currentScript = scrSpan[0];
        int currentFontId = fp?.FindFontForCodepoint(cpSpan[0], baseFont) ?? baseFont;

        for (int i = 1; i < cpCount; i++)
        {
            byte level = lvlSpan[i];
            var script = scrSpan[i];
            int fontId = fp?.FindFontForCodepoint(cpSpan[i], baseFont) ?? baseFont;

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
            fontId = fontId,
            attributeSnapshot = 0
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

        for (int i = 0; i < runCnt; i++)
        {
            var run = SharedTextBuffers.runs[i];
            var runCodepoints = cp.Slice(run.range.start, run.range.length);

            var result = shapingEngine.Shape(
                runCodepoints,
                fontProvider,
                run.fontId,
                run.script,
                run.Direction);

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
                fontId = run.fontId,
                attributeSnapshot = run.attributeSnapshot
            });
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

    private void BreakLines(float maxWidth)
    {
        SharedTextBuffers.lineCount = 0;
        SharedTextBuffers.orderedRunCount = 0;

        var linesArr = SharedTextBuffers.lines;
        var orderedRunsArr = SharedTextBuffers.orderedRuns;
        int lineCnt = SharedTextBuffers.lineCount;
        int orderedRunCnt = SharedTextBuffers.orderedRunCount;

        lineBreaker.BreakLines(
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

        // Убедимся, что буфер достаточно большой для всех глифов
        SharedTextBuffers.EnsurePositionedGlyphCapacity(SharedTextBuffers.shapedGlyphCount);

        if (fontProvider != null)
        {
            fontProvider.GetLineMetrics(settings.fontSize, out float ascender, out float descender, out float lineHeight);
            float scale = fontProvider.GetScale(baseFontId, settings.fontSize);
            layout.SetFontMetrics(ascender, descender, lineHeight, scale);
        }

        var layoutSettings = new LayoutSettings
        {
            maxWidth = settings.maxWidth,
            maxHeight = settings.maxHeight,
            lineSpacing = settings.lineSpacing,
            horizontalAlignment = settings.horizontalAlignment,
            verticalAlignment = settings.verticalAlignment
        };
        layout.SetLayoutSettings(layoutSettings);

        int glyphCnt = SharedTextBuffers.positionedGlyphCount;
        layout.Layout(
            SharedTextBuffers.lines.AsSpan(0, SharedTextBuffers.lineCount),
            SharedTextBuffers.orderedRuns.AsSpan(0, SharedTextBuffers.orderedRunCount),
            SharedTextBuffers.shapedGlyphs.AsSpan(0, SharedTextBuffers.shapedGlyphCount),
            SharedTextBuffers.positionedGlyphs, ref glyphCnt,
            out resultWidth, out resultHeight);
        SharedTextBuffers.positionedGlyphCount = glyphCnt;
    }

    #endregion

    #region Utilities

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

    private static bool MatchesClosingTag(ReadOnlySpan<char> text, int start, string tagName)
    {
        int required = 3 + tagName.Length;
        if (start + required > text.Length)
            return false;

        if (text[start] != '<' || text[start + 1] != '/')
            return false;

        for (int i = 0; i < tagName.Length; i++)
        {
            char c = text[start + 2 + i];
            char t = tagName[i];

            // Inline lowercase comparison (avoid char.ToLowerInvariant overhead)
            if (c != t && (c | 0x20) != (t | 0x20))
                return false;
        }

        return text[start + 2 + tagName.Length] == '>';
    }

    #endregion
}
