/// <summary>
/// Делегат для модификации глифа по ссылке.
/// </summary>
public delegate void GlyphAction(ref PositionedGlyph glyph);

/// <summary>
/// Хелпер для модификаторов с общей логикой итерации по глифам.
/// </summary>
public static class ModifierHelper
{
    /// <summary>
    /// Применяет action к каждому глифу в диапазоне codepoints [start, end).
    /// Автоматически обрабатывает:
    /// - Доступ к SharedTextBuffers
    /// - Clamp диапазона
    /// - Маппинг codepoint → glyph
    /// - Соседние глифы с тем же cluster (combining marks/diacritics)
    /// </summary>
    public static void ForEachGlyphInRange(int start, int end, GlyphAction action)
    {
        var glyphs = SharedTextBuffers.positionedGlyphs;
        var map = SharedTextBuffers.logicalToGlyph;
        int cpCount = SharedTextBuffers.codepointCount;
        int glyphCount = SharedTextBuffers.positionedGlyphCount;

        // Clamp to valid range
        if (start < 0) start = 0;
        if (end > cpCount) end = cpCount;

        for (int cp = start; cp < end; cp++)
        {
            int glyphIndex = map[cp];
            if (glyphIndex < 0 || glyphIndex >= glyphCount)
                continue;

            // Применить к основному глифу
            action(ref glyphs[glyphIndex]);

            // Применить к соседним глифам с тем же cluster (для combining marks/diacritics)
            int cluster = glyphs[glyphIndex].cluster;
            for (int g = glyphIndex - 1; g >= 0 && glyphs[g].cluster == cluster; g--)
                action(ref glyphs[g]);
            for (int g = glyphIndex + 1; g < glyphCount && glyphs[g].cluster == cluster; g++)
                action(ref glyphs[g]);
        }
    }
}