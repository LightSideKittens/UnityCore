using System;


public interface IShapingEngine
{
    ShapingResult Shape(
        ReadOnlySpan<int> codepoints,
        UniTextFontProvider fontProvider,
        int fontId,
        UnicodeScript script,
        TextDirection direction);
}


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