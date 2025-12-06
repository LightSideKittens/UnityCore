using System;
using System.Runtime.CompilerServices;

/// <summary>
/// Simple shaping engine for scripts that don't require complex shaping.
/// Works with Latin, Cyrillic, Greek, CJK and other simple scripts.
/// For complex scripts (Arabic, Hebrew, Indic, Thai), use HarfBuzzShapingEngine.
/// </summary>
public sealed class UniTextShapingEngine : IShapingEngine
{
    // Reusable output buffer
    private ShapedGlyph[] outputBuffer = new ShapedGlyph[256];

    /// <summary>
    /// Shape codepoints into glyphs using simple 1:1 mapping.
    /// Applies BiDi mirroring for RTL runs (UAX #9).
    /// </summary>
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

        // Ensure buffer capacity
        if (outputBuffer.Length < length)
            outputBuffer = new ShapedGlyph[Math.Max(length, outputBuffer.Length * 2)];

        float totalAdvance = 0;
        bool isRtl = direction == TextDirection.RightToLeft;
        var unicodeData = UnicodeData.Provider;

        // Simple 1:1 codepoint to glyph mapping
        for (int i = 0; i < length; i++)
        {
            int codepoint = codepoints[i];

            // UAX #9: Apply mirroring for RTL runs
            int glyphCodepoint = codepoint;
            if (isRtl && unicodeData != null && unicodeData.IsBidiMirrored(codepoint))
            {
                int mirrored = unicodeData.GetBidiMirroringGlyph(codepoint);
                if (mirrored != 0 && mirrored != codepoint)
                    glyphCodepoint = mirrored;
            }

            uint glyphIndex = 0;
            float advance = 0;

            // Get glyph index and advance together to ensure consistency
            if (fontProvider != null)
            {
                fontProvider.TryGetGlyphInfo(fontId, glyphCodepoint, out glyphIndex, out advance);
            }

            outputBuffer[i] = new ShapedGlyph
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

    /// <summary>
    /// Check if this engine can handle the specified script.
    /// Returns true for simple scripts, false for complex ones.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool CanHandle(UnicodeScript script)
    {
        // Scripts that DON'T need complex shaping (HarfBuzz)
        return script switch
        {
            // Simple scripts - 1:1 mapping
            UnicodeScript.Latin => true,
            UnicodeScript.Cyrillic => true,
            UnicodeScript.Greek => true,
            UnicodeScript.Armenian => true,
            UnicodeScript.Georgian => true,

            // CJK - simple spacing (no ligatures/reordering)
            UnicodeScript.Han => true,
            UnicodeScript.Hiragana => true,
            UnicodeScript.Katakana => true,
            UnicodeScript.Hangul => true,
            UnicodeScript.Bopomofo => true,

            // Common/Inherited - usually punctuation, numbers
            UnicodeScript.Common => true,
            UnicodeScript.Inherited => true,
            UnicodeScript.Unknown => true,

            // Complex scripts - need HarfBuzz
            UnicodeScript.Arabic => false,
            UnicodeScript.Hebrew => false,
            UnicodeScript.Syriac => false,
            UnicodeScript.Thaana => false,

            // Indic scripts - need HarfBuzz
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

            // Southeast Asian - need HarfBuzz
            UnicodeScript.Thai => false,
            UnicodeScript.Lao => false,
            UnicodeScript.Myanmar => false,
            UnicodeScript.Khmer => false,

            // Tibetan - needs HarfBuzz
            UnicodeScript.Tibetan => false,

            // Default - assume simple
            _ => true
        };
    }
}

/// <summary>
/// Hybrid shaping engine that automatically selects between simple and HarfBuzz shaping.
/// </summary>
public sealed class HybridShapingEngine : IShapingEngine
{
    private readonly UniTextShapingEngine simpleEngine;
    private readonly IShapingEngine complexEngine;

    /// <summary>
    /// Create hybrid engine with HarfBuzz for complex scripts.
    /// </summary>
    public HybridShapingEngine(IShapingEngine harfBuzzEngine)
    {
        simpleEngine = new UniTextShapingEngine();
        complexEngine = harfBuzzEngine ?? simpleEngine;
    }

    /// <summary>
    /// Create hybrid engine (uses simple shaping for all if HarfBuzz not available).
    /// </summary>
    public HybridShapingEngine()
    {
        simpleEngine = new UniTextShapingEngine();
        complexEngine = simpleEngine;
    }

    public ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction)
    {
        // Use simple engine for simple scripts
        if (UniTextShapingEngine.CanHandle(script))
        {
            return simpleEngine.Shape(codepoints, fontProvider, fontId, script, direction);
        }

        // Use complex engine (HarfBuzz) for complex scripts
        return complexEngine.Shape(codepoints, fontProvider, fontId, script, direction);
    }
}
