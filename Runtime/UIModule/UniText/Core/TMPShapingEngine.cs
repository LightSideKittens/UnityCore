using System;

/// <summary>
/// Shaping engine, использующий TMP для получения метрик глифов.
/// Не выполняет настоящий OpenType shaping (лигатуры, кернинг и т.д.),
/// но корректно получает advance из TMP шрифта.
///
/// Для полноценного shaping (арабский, деванагари, etc.) нужен HarfBuzz.
/// </summary>
public sealed class TMPShapingEngine : IShapingEngine
{
    private readonly IUnicodeDataProvider unicodeData;
    private ShapedGlyph[] buffer = new ShapedGlyph[256];

    public TMPShapingEngine(IUnicodeDataProvider unicodeData)
    {
        this.unicodeData = unicodeData ?? throw new ArgumentNullException(nameof(unicodeData));
    }

    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        TMPFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        if (buffer.Length < codepoints.Length)
            buffer = new ShapedGlyph[codepoints.Length];

        float totalAdvance = 0;
        bool isRtl = direction == TextDirection.RightToLeft;

        for (int i = 0; i < codepoints.Length; i++)
        {
            int cp = codepoints[i];

            // UAX #9: Apply mirroring for RTL runs
            // Скобки и другие paired characters должны зеркалиться в RTL контексте
            int glyphCp = cp;
            if (isRtl && unicodeData.IsBidiMirrored(cp))
            {
                int mirrored = unicodeData.GetBidiMirroringGlyph(cp);
                if (mirrored != 0)
                    glyphCp = mirrored;
            }

            float advance = GetAdvanceForCodepoint(glyphCp, fontProvider, fontId);

            buffer[i] = new ShapedGlyph
            {
                glyphId = glyphCp, // Используем (возможно зеркальный) codepoint как glyph ID
                cluster = i,
                advanceX = advance,
                advanceY = 0,
                offsetX = 0,
                offsetY = 0
            };

            totalAdvance += advance;
        }

        return new ShapingResult(buffer.AsSpan(0, codepoints.Length), totalAdvance);
    }

    private float GetAdvanceForCodepoint(int codepoint, TMPFontProvider fontProvider, int fontId)
    {
        // Default ignorable characters are zero-width
        if (unicodeData.IsDefaultIgnorable(codepoint))
        {
            return 0;
        }

        // Try to get advance from font
        if (fontProvider != null && fontProvider.TryGetGlyphMetrics(fontId, codepoint, out var metrics))
        {
            return metrics.advance;
        }

        // Fallback: approximate based on Unicode properties
        var gc = unicodeData.GetGeneralCategory(codepoint);

        // Spaces
        if (gc == GeneralCategory.Zs)
        {
            return 5f; // Примерная ширина пробела
        }

        // Wide characters (CJK)
        var eaw = unicodeData.GetEastAsianWidth(codepoint);
        if (eaw == EastAsianWidth.W || eaw == EastAsianWidth.F)
        {
            return 20f; // Примерная ширина широкого символа
        }

        // Default
        return 10f;
    }
}
