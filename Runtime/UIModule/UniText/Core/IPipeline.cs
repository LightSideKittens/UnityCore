using System;

/// <summary>
/// Абстракция shaping engine.
/// Единственный интерфейс, сохранённый для поддержки HarfBuzz.
/// </summary>
public interface IShapingEngine
{
    /// <summary>
    /// Shape codepoints в глифы.
    /// </summary>
    ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        TMPFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction);
}

/// <summary>
/// Результат shaping.
/// </summary>
public readonly ref struct ShapingResult
{
    public readonly ReadOnlySpan<ShapedGlyph> Glyphs;
    public readonly float TotalAdvance;

    public ShapingResult(ReadOnlySpan<ShapedGlyph> glyphs, float totalAdvance)
    {
        Glyphs = glyphs;
        TotalAdvance = totalAdvance;
    }
}

/// <summary>
/// Метрики глифа.
/// </summary>
public struct GlyphMetrics
{
    public float width;
    public float height;
    public float bearingX;
    public float bearingY;
    public float advance;
}
