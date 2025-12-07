using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Simple 1:1 shaping for scripts without complex shaping requirements.
/// </summary>
public sealed class UniTextShapingEngine : IShapingEngine
{
    private ShapedGlyph[] outputBuffer = new ShapedGlyph[256];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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

        // Cache locals for faster access in loop
        var buffer = outputBuffer;
        float totalAdvance = 0;

        // Split into two paths: RTL (rare) and LTR (common)
        if (direction == TextDirection.RightToLeft)
        {
            var unicodeData = UnicodeData.Provider;
            bool hasUnicodeData = unicodeData != null;

            for (int i = 0; i < length; i++)
            {
                int codepoint = codepoints[i];
                int glyphCodepoint = codepoint;

                // BiDi mirroring for RTL
                if (hasUnicodeData && unicodeData.IsBidiMirrored(codepoint))
                {
                    int mirrored = unicodeData.GetBidiMirroringGlyph(codepoint);
                    if (mirrored != 0 && mirrored != codepoint)
                        glyphCodepoint = mirrored;
                }

                fontProvider.TryGetGlyphInfo(fontId, glyphCodepoint, out uint glyphIndex, out float advance);

                // RTL: reverse order
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
            // LTR fast path - no mirroring, no reverse
            for (int i = 0; i < length; i++)
            {
                int codepoint = codepoints[i];
                fontProvider.TryGetGlyphInfo(fontId, codepoint, out uint glyphIndex, out float advance);

                buffer[i] = new ShapedGlyph
                {
                    glyphId = (int)glyphIndex,
                    cluster = i,
                    advanceX = advance
                };

                totalAdvance += advance;
            }
        }

        return new ShapingResult(buffer.AsSpan(0, length), totalAdvance);
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

    // DEBUG: Enable detailed logging
    public static bool DebugLogging = false;

    public HybridShapingEngine(IShapingEngine harfBuzzEngine)
    {
        complexEngine = harfBuzzEngine ?? simpleEngine;
        if (DebugLogging)
            UnityEngine.Debug.Log($"[HybridShapingEngine] Created with complexEngine={complexEngine.GetType().Name}");
    }

    public HybridShapingEngine()
    {
        complexEngine = simpleEngine;
        if (DebugLogging)
            UnityEngine.Debug.Log("[HybridShapingEngine] Created with NO complex engine (using simple for all)");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        // Fast path: avoid virtual call overhead for simple scripts
        if (UniTextShapingEngine.CanHandle(script))
            return simpleEngine.Shape(codepoints, fontProvider, fontId, script, direction);

        return complexEngine.Shape(codepoints, fontProvider, fontId, script, direction);
    }
}
