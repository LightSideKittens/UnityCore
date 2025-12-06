using System;
using System.Collections.Generic;
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
/// Использует unified buffers для избежания лишних копирований.
/// </summary>
public sealed class TextProcessor
{
    // Pipeline components
    private readonly RichTextParser parser;
    private readonly BidiEngine bidiEngine;
    private readonly ScriptAnalyzer scriptAnalyzer;
    private readonly Itemizer itemizer;
    private readonly IShapingEngine shapingEngine;
    private readonly LineBreaker lineBreaker;
    private readonly TextLayout layout;

    // Unified buffers
    private int[] codepoints = new int[256];
    private int codepointCount;

    private readonly List<TextAttributeBase> attributes = new(32);

    private byte[] bidiLevels = new byte[256];
    private BidiParagraph[] bidiParagraphs = Array.Empty<BidiParagraph>();
    private TextDirection baseDirection;

    private UnicodeScript[] scripts = new UnicodeScript[256];

    private TextRun[] runs = new TextRun[32];
    private int runCount;

    private ShapedRun[] shapedRuns = new ShapedRun[32];
    private int shapedRunCount;
    private ShapedGlyph[] shapedGlyphs = new ShapedGlyph[256];
    private int shapedGlyphCount;

    private TextLine[] lines = new TextLine[16];
    private int lineCount;
    private ShapedRun[] orderedRuns = new ShapedRun[32];
    private int orderedRunCount;

    private PositionedGlyph[] positionedGlyphs = new PositionedGlyph[256];
    private int positionedGlyphCount;

    // State
    private TMPFontProvider fontProvider;
    private int baseFontId;
    private float resultWidth;
    private float resultHeight;

    public TextProcessor(IUnicodeDataProvider unicodeData, TagRegistry tagRegistry = null)
    {
        if (unicodeData == null)
            throw new ArgumentNullException(nameof(unicodeData));

        parser = new RichTextParser(tagRegistry ?? TagRegistry.CreateDefault());
        bidiEngine = new BidiEngine(unicodeData);
        scriptAnalyzer = new ScriptAnalyzer(unicodeData);
        itemizer = new Itemizer(unicodeData);
        shapingEngine = new TMPShapingEngine(unicodeData);
        lineBreaker = new LineBreaker(unicodeData);
        layout = new TextLayout();
    }

    /// <summary>
    /// Создать с custom shaping engine (для HarfBuzz).
    /// </summary>
    public TextProcessor(IUnicodeDataProvider unicodeData, IShapingEngine shapingEngine, TagRegistry tagRegistry = null)
    {
        if (unicodeData == null)
            throw new ArgumentNullException(nameof(unicodeData));
        if (shapingEngine == null)
            throw new ArgumentNullException(nameof(shapingEngine));

        parser = new RichTextParser(tagRegistry ?? TagRegistry.CreateDefault());
        bidiEngine = new BidiEngine(unicodeData);
        scriptAnalyzer = new ScriptAnalyzer(unicodeData);
        itemizer = new Itemizer(unicodeData);
        this.shapingEngine = shapingEngine;
        lineBreaker = new LineBreaker(unicodeData);
        layout = new TextLayout();
    }

    /// <summary>
    /// Установить font provider.
    /// </summary>
    public void SetFontProvider(TMPFontProvider provider, int defaultFontId = 0)
    {
        fontProvider = provider;
        baseFontId = defaultFontId;
    }

    /// <summary>
    /// Обработать текст.
    /// </summary>
    public ReadOnlySpan<PositionedGlyph> Process(ReadOnlySpan<char> text, TextProcessSettings settings)
    {
        if (text.IsEmpty)
        {
            positionedGlyphCount = 0;
            resultWidth = 0;
            resultHeight = 0;
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

        if (codepointCount == 0)
        {
            positionedGlyphCount = 0;
            resultWidth = 0;
            resultHeight = 0;
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

        return positionedGlyphs.AsSpan(0, positionedGlyphCount);
    }

    /// <summary>
    /// Результаты последней обработки.
    /// </summary>
    public float ResultWidth => resultWidth;
    public float ResultHeight => resultHeight;
    public Vector2 ResultSize => new(resultWidth, resultHeight);
    public ReadOnlySpan<PositionedGlyph> PositionedGlyphs => positionedGlyphs.AsSpan(0, positionedGlyphCount);
    public ReadOnlySpan<int> Codepoints => codepoints.AsSpan(0, codepointCount);
    public IReadOnlyList<TextAttributeBase> Attributes => attributes;

    #region Pipeline Steps

    private void Parse(ReadOnlySpan<char> text)
    {
        codepointCount = 0;
        attributes.Clear();

        EnsureCodepointCapacity(text.Length);

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
                    if (parser.ProcessTagDirect(tagContent, codepointCount, attributes, AddCodepoint, out bool isNoparseOpen))
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
        codepointCount = 0;
        attributes.Clear();

        EnsureCodepointCapacity(text.Length);

        int i = 0;
        while (i < text.Length)
        {
            AddCharacter(text, ref i);
        }
    }

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

    private void AddCodepoint(int cp)
    {
        EnsureCodepointCapacity(codepointCount + 1);
        codepoints[codepointCount++] = cp;
    }

    private void AnalyzeBidi(TextDirection requestedDirection)
    {
        EnsureBidiCapacity(codepointCount);

        // Конвертируем TextDirection в BidiParagraphDirection
        var direction = requestedDirection switch
        {
            TextDirection.RightToLeft => BidiParagraphDirection.RightToLeft,
            TextDirection.LeftToRight => BidiParagraphDirection.LeftToRight,
            TextDirection.Auto => BidiParagraphDirection.Auto,
            _ => BidiParagraphDirection.Auto
        };

        var result = bidiEngine.Process(codepoints.AsSpan(0, codepointCount), direction);

        if (result.levels != null && result.levels.Length > 0)
        {
            int copyLen = Math.Min(result.levels.Length, codepointCount);
            result.levels.AsSpan(0, copyLen).CopyTo(bidiLevels);
        }
        else
        {
            bidiLevels.AsSpan(0, codepointCount).Fill(0);
        }

        // Сохраняем информацию о параграфах (каждый имеет свой baseLevel)
        bidiParagraphs = result.paragraphs ?? Array.Empty<BidiParagraph>();

        // baseDirection первого параграфа (для совместимости)
        baseDirection = result.Direction == BidiDirection.RightToLeft
            ? TextDirection.RightToLeft
            : TextDirection.LeftToRight;
    }

    private void AnalyzeScripts()
    {
        EnsureScriptCapacity(codepointCount);
        scriptAnalyzer.Analyze(codepoints.AsSpan(0, codepointCount), scripts);
    }

    private void Itemize()
    {
        runCount = 0;

        if (codepointCount == 0) return;

        var cp = codepoints.AsSpan(0, codepointCount);

        int runStart = 0;
        byte currentLevel = bidiLevels[0];
        var currentScript = scripts[0];
        int currentFontId = fontProvider?.FindFontForCodepoint(cp[0], baseFontId) ?? baseFontId;

        for (int i = 1; i < codepointCount; i++)
        {
            bool needBreak = false;

            if (bidiLevels[i] != currentLevel)
                needBreak = true;

            if (scripts[i] != currentScript)
                needBreak = true;

            int fontId = fontProvider?.FindFontForCodepoint(cp[i], baseFontId) ?? baseFontId;
            if (fontId != currentFontId)
                needBreak = true;

            if (needBreak)
            {
                AddRun(runStart, i - runStart, currentLevel, currentScript, currentFontId);
                runStart = i;
                currentLevel = bidiLevels[i];
                currentScript = scripts[i];
                currentFontId = fontId;
            }
        }

        AddRun(runStart, codepointCount - runStart, currentLevel, currentScript, currentFontId);
    }

    private void AddRun(int start, int length, byte bidiLevel, UnicodeScript script, int fontId)
    {
        EnsureRunCapacity(runCount + 1);

        runs[runCount++] = new TextRun
        {
            range = new TextRange(start, length),
            bidiLevel = bidiLevel,
            script = script,
            fontId = fontId,
            attributeSnapshot = 0
        };
    }

    private void Shape()
    {
        shapedRunCount = 0;
        shapedGlyphCount = 0;

        var cp = codepoints.AsSpan(0, codepointCount);

        for (int i = 0; i < runCount; i++)
        {
            var run = runs[i];
            var runCodepoints = cp.Slice(run.range.start, run.range.length);

            var result = shapingEngine.Shape(
                runCodepoints,
                fontProvider,
                run.fontId,
                run.script,
                run.Direction);

            int glyphStart = shapedGlyphCount;
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

    private void AddShapedGlyphs(ReadOnlySpan<ShapedGlyph> glyphs)
    {
        EnsureShapedGlyphCapacity(shapedGlyphCount + glyphs.Length);
        glyphs.CopyTo(shapedGlyphs.AsSpan(shapedGlyphCount));
        shapedGlyphCount += glyphs.Length;
    }

    private void AddShapedRun(ShapedRun run)
    {
        EnsureShapedRunCapacity(shapedRunCount + 1);
        shapedRuns[shapedRunCount++] = run;
    }

    private void BreakLines(float maxWidth)
    {
        lineCount = 0;
        orderedRunCount = 0;

        lineBreaker.BreakLines(
            codepoints.AsSpan(0, codepointCount),
            shapedRuns.AsSpan(0, shapedRunCount),
            shapedGlyphs.AsSpan(0, shapedGlyphCount),
            maxWidth,
            bidiParagraphs,
            ref lines, ref lineCount,
            ref orderedRuns, ref orderedRunCount);
    }

    private void LayoutText(TextProcessSettings settings)
    {
        positionedGlyphCount = 0;

        // Убедимся, что буфер достаточно большой для всех глифов
        EnsurePositionedGlyphCapacity(shapedGlyphCount);

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

        layout.Layout(
            lines.AsSpan(0, lineCount),
            orderedRuns.AsSpan(0, orderedRunCount),
            shapedGlyphs.AsSpan(0, shapedGlyphCount),
            positionedGlyphs, ref positionedGlyphCount,
            out resultWidth, out resultHeight);
    }

    #endregion

    #region Buffer Management

    private void EnsureCodepointCapacity(int required)
    {
        if (codepoints.Length >= required) return;
        Array.Resize(ref codepoints, Math.Max(required, codepoints.Length * 2));
    }

    private void EnsureBidiCapacity(int required)
    {
        if (bidiLevels.Length >= required) return;
        Array.Resize(ref bidiLevels, Math.Max(required, bidiLevels.Length * 2));
    }

    private void EnsureScriptCapacity(int required)
    {
        if (scripts.Length >= required) return;
        Array.Resize(ref scripts, Math.Max(required, scripts.Length * 2));
    }

    private void EnsureRunCapacity(int required)
    {
        if (runs.Length >= required) return;
        Array.Resize(ref runs, Math.Max(required, runs.Length * 2));
    }

    private void EnsureShapedRunCapacity(int required)
    {
        if (shapedRuns.Length >= required) return;
        Array.Resize(ref shapedRuns, Math.Max(required, shapedRuns.Length * 2));
    }

    private void EnsureShapedGlyphCapacity(int required)
    {
        if (shapedGlyphs.Length >= required) return;
        Array.Resize(ref shapedGlyphs, Math.Max(required, shapedGlyphs.Length * 2));
    }

    private void EnsurePositionedGlyphCapacity(int required)
    {
        if (positionedGlyphs.Length >= required) return;
        Array.Resize(ref positionedGlyphs, Math.Max(required, positionedGlyphs.Length * 2));
    }

    #endregion

    #region Utilities

    private static int FindTagEnd(ReadOnlySpan<char> text, int start)
    {
        for (int i = start + 1; i < text.Length && i < start + 128; i++)
        {
            if (text[i] == '>')
                return i;
        }
        return -1;
    }

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

            if (char.ToLowerInvariant(c) != char.ToLowerInvariant(t))
                return false;
        }

        return text[start + 2 + tagName.Length] == '>';
    }

    #endregion
}
