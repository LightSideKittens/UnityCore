using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Simple 1:1 shaping for scripts without complex shaping requirements.
/// </summary>
public sealed class UniTextShapingEngine : IShapingEngine
{
    private ShapedGlyph[] outputBuffer = new ShapedGlyph[256];

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

        if (outputBuffer.Length < length)
            outputBuffer = new ShapedGlyph[Math.Max(length, outputBuffer.Length * 2)];

        float totalAdvance = 0;
        bool isRtl = direction == TextDirection.RightToLeft;
        var unicodeData = UnicodeData.Provider;

        for (int i = 0; i < length; i++)
        {
            int codepoint = codepoints[i];
            int glyphCodepoint = codepoint;

            if (isRtl && unicodeData != null && unicodeData.IsBidiMirrored(codepoint))
            {
                int mirrored = unicodeData.GetBidiMirroringGlyph(codepoint);
                if (mirrored != 0 && mirrored != codepoint)
                    glyphCodepoint = mirrored;
            }

            uint glyphIndex = 0;
            float advance = 0;
            fontProvider?.TryGetGlyphInfo(fontId, glyphCodepoint, out glyphIndex, out advance);

            int outputIndex = isRtl ? (length - 1 - i) : i;
            outputBuffer[outputIndex] = new ShapedGlyph
            {
                glyphId = (int)glyphIndex,
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanHandle(UnicodeScript script)
    {
        return script switch
        {
            // Simple scripts (1:1 mapping)
            UnicodeScript.Latin => true,
            UnicodeScript.Cyrillic => true,
            UnicodeScript.Greek => true,
            UnicodeScript.Armenian => true,
            UnicodeScript.Georgian => true,
            UnicodeScript.Han => true,
            UnicodeScript.Hiragana => true,
            UnicodeScript.Katakana => true,
            UnicodeScript.Hangul => true,
            UnicodeScript.Bopomofo => true,
            UnicodeScript.Common => true,
            UnicodeScript.Inherited => true,
            UnicodeScript.Unknown => true,

            // Complex scripts (need HarfBuzz)
            UnicodeScript.Arabic => false,
            UnicodeScript.Hebrew => false,
            UnicodeScript.Syriac => false,
            UnicodeScript.Thaana => false,
            UnicodeScript.Devanagari => false,
            UnicodeScript.Bengali => false,
            UnicodeScript.Gurmukhi => false,
            UnicodeScript.Gujarati => false,
            UnicodeScript.Oriya => false,
            UnicodeScript.Tamil => false,
            UnicodeScript.Telugu => false,
            UnicodeScript.Kannada => false,
            UnicodeScript.Malayalam => false,
            UnicodeScript.Sinhala => false,
            UnicodeScript.Thai => false,
            UnicodeScript.Lao => false,
            UnicodeScript.Myanmar => false,
            UnicodeScript.Khmer => false,
            UnicodeScript.Tibetan => false,

            _ => true
        };
    }
}

/// <summary>
/// Hybrid engine: simple shaping for basic scripts, HarfBuzz for complex scripts.
/// </summary>
public sealed class HybridShapingEngine : IShapingEngine
{
    private readonly UniTextShapingEngine simpleEngine = new();
    private readonly IShapingEngine complexEngine;

    public HybridShapingEngine(IShapingEngine harfBuzzEngine)
    {
        complexEngine = harfBuzzEngine ?? simpleEngine;
    }

    public HybridShapingEngine()
    {
        complexEngine = simpleEngine;
    }

    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        var engine = UniTextShapingEngine.CanHandle(script) ? simpleEngine : complexEngine;
        return engine.Shape(codepoints, fontProvider, fontId, script, direction);
    }
}
