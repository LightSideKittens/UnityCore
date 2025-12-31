using System;
using System.Runtime.CompilerServices;


public sealed class UniTextShapingEngine : IShapingEngine
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        var length = codepoints.Length;
        if (length == 0)
            return new ShapingResult(ReadOnlySpan<ShapedGlyph>.Empty, 0);

        SharedPipelineComponents.EnsureShapingOutputCapacity(length);

        var buffer = SharedPipelineComponents.ShapingOutputBuffer;
        float totalAdvance = 0;

        var font = fontProvider.GetFontAsset(fontId);
        var fontSize = fontProvider.FontSize;

        if (direction == TextDirection.RightToLeft)
        {
            var unicodeData = UnicodeData.Provider;
            var hasUnicodeData = unicodeData != null;

            for (var i = 0; i < length; i++)
            {
                var codepoint = codepoints[i];
                var glyphCodepoint = codepoint;

                if (hasUnicodeData && unicodeData.IsBidiMirrored(codepoint))
                {
                    var mirrored = unicodeData.GetBidiMirroringGlyph(codepoint);
                    if (mirrored != 0 && mirrored != codepoint)
                        glyphCodepoint = mirrored;
                }

                uint glyphIndex;
                float advance;

                HarfBuzzShapingEngine.TryGetGlyphInfo(font, (uint)glyphCodepoint, fontSize, out glyphIndex, out advance);
                
                buffer[length - 1 - i] = new ShapedGlyph
                {
                    glyphId = (int)glyphIndex,
                    cluster = i,
                    advanceX = advance
                };

                totalAdvance += advance;
            }
        }
        else
        {
            for (var i = 0; i < length; i++)
            {
                var codepoint = codepoints[i];

                uint glyphIndex;
                float advance;

                HarfBuzzShapingEngine.TryGetGlyphInfo(font, (uint)codepoint, fontSize, out glyphIndex, out advance);
                
                buffer[i] = new ShapedGlyph
                {
                    glyphId = (int)glyphIndex,
                    cluster = i,
                    advanceX = advance
                };

                totalAdvance += advance;
            }
        }

        return new ShapingResult(SharedPipelineComponents.ShapingOutputBuffer.AsSpan(0, length), totalAdvance);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanHandle(UnicodeScript script)
    {
        return script switch
        {
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


public sealed class HybridShapingEngine : IShapingEngine
{
    private static readonly UniTextShapingEngine simpleEngine = new();
    private readonly IShapingEngine complexEngine;

    public HybridShapingEngine(IShapingEngine harfBuzzEngine)
    {
        complexEngine = harfBuzzEngine ?? simpleEngine;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        if (UniTextShapingEngine.CanHandle(script))
            return simpleEngine.Shape(codepoints, fontProvider, fontId, script, direction);

        return complexEngine.Shape(codepoints, fontProvider, fontId, script, direction);
    }
}