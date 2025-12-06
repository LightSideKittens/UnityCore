// HarfBuzz Shaping Engine для UniText
// Требует HarfBuzzSharp NuGet package
//
// Установка:
// 1. Добавьте HarfBuzzSharp через NuGetForUnity
// 2. Или скопируйте native библиотеки из SkiaForUnity
// 3. Добавьте UNITEXT_HARFBUZZ в Scripting Define Symbols

#if UNITEXT_HARFBUZZ
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using HarfBuzzSharp;
using Buffer = HarfBuzzSharp.Buffer;

/// <summary>
/// Shaping engine, использующий HarfBuzz для OpenType shaping.
/// Поддерживает сложные скрипты: арабский, деванагари, тайский и т.д.
/// </summary>
public sealed class HarfBuzzShapingEngine : IShapingEngine, IDisposable
{
    private ShapedGlyph[] outputBuffer = new ShapedGlyph[256];
    private readonly Buffer harfBuzzBuffer;

    // Кеш HarfBuzz Font объектов (по fontId)
    private readonly Dictionary<int, HarfBuzzFontEntry> fontCache = new();

    private struct HarfBuzzFontEntry
    {
        public Blob blob;
        public Face face;
        public Font font;
    }

    public HarfBuzzShapingEngine()
    {
        harfBuzzBuffer = new Buffer();
    }

    public void Dispose()
    {
        harfBuzzBuffer?.Dispose();

        foreach (var entry in fontCache.Values)
        {
            entry.font?.Dispose();
            entry.face?.Dispose();
            entry.blob?.Dispose();
        }
        fontCache.Clear();
    }

    /// <summary>
    /// Зарегистрировать шрифт для использования в shaping.
    /// Должен быть вызван перед Shape() для каждого fontId.
    /// </summary>
    public void RegisterFont(int fontId, byte[] fontData)
    {
        if (fontData == null || fontData.Length == 0)
            throw new ArgumentException("Font data is empty", nameof(fontData));

        // Удаляем старую запись если есть
        if (fontCache.TryGetValue(fontId, out var oldEntry))
        {
            oldEntry.font?.Dispose();
            oldEntry.face?.Dispose();
            oldEntry.blob?.Dispose();
        }

        // Создаём HarfBuzz объекты
        var blob = Blob.FromMemory(fontData);
        var face = new Face(blob, 0);
        var font = new Font(face);

        fontCache[fontId] = new HarfBuzzFontEntry
        {
            blob = blob,
            face = face,
            font = font
        };
    }

    /// <summary>
    /// Проверить, зарегистрирован ли шрифт.
    /// </summary>
    public bool HasFont(int fontId) => fontCache.ContainsKey(fontId);

    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        int length = codepoints.Length;
        if (length == 0)
            return new ShapingResult(ReadOnlySpan<ShapedGlyph>.Empty, 0);

        // Проверяем наличие шрифта
        if (!fontCache.TryGetValue(fontId, out var fontEntry))
        {
            UnityEngine.Debug.LogError($"HarfBuzzShapingEngine: Font {fontId} not registered. Call RegisterFont() first.");
            return FallbackShape(codepoints, fontProvider, fontId, direction);
        }

        // Настраиваем буфер
        harfBuzzBuffer.ClearContents();
        harfBuzzBuffer.ContentType = ContentType.Unicode;

        // Добавляем codepoints
        foreach (int cp in codepoints)
        {
            harfBuzzBuffer.Add(cp, 0);
        }

        // Устанавливаем направление
        harfBuzzBuffer.Direction = direction == TextDirection.RightToLeft
            ? Direction.RightToLeft
            : Direction.LeftToRight;

        // Устанавливаем скрипт
        harfBuzzBuffer.Script = MapScript(script);

        // Определяем свойства сегмента
        harfBuzzBuffer.GuessSegmentProperties();

        // Shape!
        fontEntry.font.Shape(harfBuzzBuffer);

        // Получаем результат
        var glyphInfos = harfBuzzBuffer.GlyphInfos;
        var glyphPositions = harfBuzzBuffer.GlyphPositions;
        int glyphCount = glyphInfos.Length;

        // Увеличиваем буфер если нужно
        if (outputBuffer.Length < glyphCount)
            outputBuffer = new ShapedGlyph[Math.Max(glyphCount, outputBuffer.Length * 2)];

        // Конвертируем результат
        float totalAdvance = 0;
        float fontSize = fontProvider?.FontSize ?? 36f;
        float scale = fontSize / fontEntry.face.UnitsPerEm;

        for (int i = 0; i < glyphCount; i++)
        {
            var info = glyphInfos[i];
            var pos = glyphPositions[i];

            float advanceX = pos.XAdvance * scale;
            float advanceY = pos.YAdvance * scale;
            float offsetX = pos.XOffset * scale;
            float offsetY = pos.YOffset * scale;

            outputBuffer[i] = new ShapedGlyph
            {
                glyphId = (int)info.Codepoint, // После shaping это glyph ID
                cluster = (int)info.Cluster,
                advanceX = advanceX,
                advanceY = advanceY,
                offsetX = offsetX,
                offsetY = offsetY
            };

            totalAdvance += advanceX;
        }

        return new ShapingResult(outputBuffer.AsSpan(0, glyphCount), totalAdvance);
    }

    /// <summary>
    /// Fallback shaping когда HarfBuzz шрифт не доступен.
    /// </summary>
    private ShapingResult FallbackShape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        TextDirection direction)
    {
        int length = codepoints.Length;
        if (outputBuffer.Length < length)
            outputBuffer = new ShapedGlyph[Math.Max(length, outputBuffer.Length * 2)];

        float totalAdvance = 0;

        for (int i = 0; i < length; i++)
        {
            int cp = codepoints[i];
            float advance = 0;

            if (fontProvider != null && fontProvider.TryGetGlyphMetrics(fontId, cp, out var metrics))
            {
                advance = metrics.advance;
            }

            outputBuffer[i] = new ShapedGlyph
            {
                glyphId = cp,
                cluster = i,
                advanceX = advance,
                advanceY = 0,
                offsetX = 0,
                offsetY = 0
            };

            totalAdvance += advance;
        }

        return new ShapingResult(outputBuffer.AsSpan(0, length), totalAdvance);
    }

    /// <summary>
    /// Маппинг UnicodeScript → HarfBuzz Script.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Script MapScript(UnicodeScript script)
    {
        // HarfBuzz использует ISO 15924 script tags
        return script switch
        {
            UnicodeScript.Arabic => Script.Arabic,
            UnicodeScript.Armenian => Script.Armenian,
            UnicodeScript.Bengali => Script.Bengali,
            UnicodeScript.Cyrillic => Script.Cyrillic,
            UnicodeScript.Devanagari => Script.Devanagari,
            UnicodeScript.Georgian => Script.Georgian,
            UnicodeScript.Greek => Script.Greek,
            UnicodeScript.Gujarati => Script.Gujarati,
            UnicodeScript.Gurmukhi => Script.Gurmukhi,
            UnicodeScript.Han => Script.Han,
            UnicodeScript.Hangul => Script.Hangul,
            UnicodeScript.Hebrew => Script.Hebrew,
            UnicodeScript.Hiragana => Script.Hiragana,
            UnicodeScript.Kannada => Script.Kannada,
            UnicodeScript.Katakana => Script.Katakana,
            UnicodeScript.Khmer => Script.Khmer,
            UnicodeScript.Lao => Script.Lao,
            UnicodeScript.Latin => Script.Latin,
            UnicodeScript.Malayalam => Script.Malayalam,
            UnicodeScript.Myanmar => Script.Myanmar,
            UnicodeScript.Oriya => Script.Oriya,
            UnicodeScript.Sinhala => Script.Sinhala,
            UnicodeScript.Tamil => Script.Tamil,
            UnicodeScript.Telugu => Script.Telugu,
            UnicodeScript.Thai => Script.Thai,
            UnicodeScript.Tibetan => Script.Tibetan,
            _ => Script.Common
        };
    }
}

#else

// Stub когда HarfBuzz не установлен
using System;

/// <summary>
/// Stub для HarfBuzzShapingEngine когда HarfBuzzSharp не установлен.
/// Добавьте UNITEXT_HARFBUZZ в Scripting Define Symbols после установки HarfBuzzSharp.
/// </summary>
public sealed class HarfBuzzShapingEngine : IShapingEngine
{
    public HarfBuzzShapingEngine()
    {
        UnityEngine.Debug.LogWarning(
            "HarfBuzzShapingEngine: HarfBuzzSharp not installed. " +
            "Complex scripts (Arabic, Devanagari, Thai) will not render correctly.\n" +
            "To enable: 1) Install HarfBuzzSharp 2) Add UNITEXT_HARFBUZZ to Scripting Define Symbols");
    }

    public void RegisterFont(int fontId, byte[] fontData)
    {
        // No-op
    }

    public bool HasFont(int fontId) => false;

    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        // Fallback to simple shaping (no OpenType features)
        var buffer = new ShapedGlyph[codepoints.Length];
        float totalAdvance = 0;

        for (int i = 0; i < codepoints.Length; i++)
        {
            int cp = codepoints[i];
            float advance = 0;

            if (fontProvider != null && fontProvider.TryGetGlyphMetrics(fontId, cp, out var metrics))
            {
                advance = metrics.advance;
            }

            buffer[i] = new ShapedGlyph
            {
                glyphId = cp,
                cluster = i,
                advanceX = advance,
                advanceY = 0,
                offsetX = 0,
                offsetY = 0
            };

            totalAdvance += advance;
        }

        return new ShapingResult(buffer, totalAdvance);
    }
}

#endif
