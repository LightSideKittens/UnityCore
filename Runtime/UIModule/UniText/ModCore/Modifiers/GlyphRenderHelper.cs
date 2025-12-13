using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore;

/// <summary>
/// Статический helper для рендеринга отдельных глифов.
/// Используется для маркеров списков и других inline элементов.
/// </summary>
public static class GlyphRenderHelper
{
    public static bool DebugLogging = false;

    /// <summary>
    /// Рендерит один символ в указанной позиции.
    /// Добавляет 4 вершины и 6 индексов треугольников.
    /// </summary>
    /// <param name="codepoint">Unicode codepoint символа</param>
    /// <param name="x">X позиция (левый край)</param>
    /// <param name="baselineY">Y позиция baseline</param>
    /// <param name="color">Цвет глифа</param>
    /// <returns>Advance X (ширина для следующего символа), или 0 если глиф не найден</returns>
    public static float DrawGlyph(uint codepoint, float x, float baselineY, Color32 color)
    {
        var fontAsset = UniTextMeshGenerator.currentFontAsset;
        if (fontAsset == null)
            return 0f;

        // Получить glyph для codepoint
        var glyph = GetGlyph(fontAsset, codepoint);
        if (glyph == null)
        {
            if (DebugLogging)
                Debug.LogWarning($"[GlyphRenderHelper] Glyph not found for codepoint U+{codepoint:X4} ('{(char)codepoint}')");
            return 0f;
        }

        var glyphRect = glyph.glyphRect;
        var metrics = glyph.metrics;

        // Пропустить whitespace (zero-size glyphs)
        if (glyphRect.width == 0 || glyphRect.height == 0)
            return metrics.horizontalAdvance * UniTextMeshGenerator.scale;

        float scale = UniTextMeshGenerator.scale;
        float xScale = UniTextMeshGenerator.xScale;
        float padding = fontAsset.AtlasPadding;
        float atlasWidth = fontAsset.AtlasWidth;
        float atlasHeight = fontAsset.AtlasHeight;
        float padding2 = padding * 2;

        float invAtlasWidth = 1f / atlasWidth;
        float invAtlasHeight = 1f / atlasHeight;

        // Vertex positions
        float bearingXScaled = (metrics.horizontalBearingX - padding) * scale;
        float bearingYScaled = (metrics.horizontalBearingY + padding) * scale;
        float heightScaled = (metrics.height + padding2) * scale;
        float widthScaled = (metrics.width + padding2) * scale;

        float tlX = x + bearingXScaled;
        float tlY = baselineY + bearingYScaled;
        float blY = tlY - heightScaled;
        float trX = tlX + widthScaled;

        // UV coordinates
        float uvBLx = (glyphRect.x - padding) * invAtlasWidth;
        float uvBLy = (glyphRect.y - padding) * invAtlasHeight;
        float uvTLy = (glyphRect.y + glyphRect.height + padding) * invAtlasHeight;
        float uvTRx = (glyphRect.x + glyphRect.width + padding) * invAtlasWidth;

        var verts = UniTextMeshGenerator.Vertices;
        var uvData = UniTextMeshGenerator.Uvs0;
        var cols = UniTextMeshGenerator.Colors;
        var tris = UniTextMeshGenerator.Triangles;

        int vertIdx = UniTextMeshGenerator.vertexCount;
        int triIdx = UniTextMeshGenerator.triangleCount;

        int i0 = vertIdx;
        int i1 = vertIdx + 1;
        int i2 = vertIdx + 2;
        int i3 = vertIdx + 3;

        // Vertices (BL, TL, TR, BR)
        ref var v0 = ref verts[i0];
        v0.x = tlX; v0.y = blY; v0.z = 0;
        ref var v1 = ref verts[i1];
        v1.x = tlX; v1.y = tlY; v1.z = 0;
        ref var v2 = ref verts[i2];
        v2.x = trX; v2.y = tlY; v2.z = 0;
        ref var v3 = ref verts[i3];
        v3.x = trX; v3.y = blY; v3.z = 0;

        // UV0 (xy = texture coords, w = xScale for SDF)
        ref var uv0 = ref uvData[i0];
        uv0.x = uvBLx; uv0.y = uvBLy; uv0.z = 0; uv0.w = xScale;
        ref var uv1 = ref uvData[i1];
        uv1.x = uvBLx; uv1.y = uvTLy; uv1.z = 0; uv1.w = xScale;
        ref var uv2 = ref uvData[i2];
        uv2.x = uvTRx; uv2.y = uvTLy; uv2.z = 0; uv2.w = xScale;
        ref var uv3 = ref uvData[i3];
        uv3.x = uvTRx; uv3.y = uvBLy; uv3.z = 0; uv3.w = xScale;

        // Colors
        cols[i0] = color;
        cols[i1] = color;
        cols[i2] = color;
        cols[i3] = color;

        // Triangles
        tris[triIdx] = i0;
        tris[triIdx + 1] = i1;
        tris[triIdx + 2] = i2;
        tris[triIdx + 3] = i2;
        tris[triIdx + 4] = i3;
        tris[triIdx + 5] = i0;

        UniTextMeshGenerator.vertexCount += 4;
        UniTextMeshGenerator.triangleCount += 6;

        return metrics.horizontalAdvance * scale;
    }

    /// <summary>
    /// Рендерит строку в указанной позиции.
    /// </summary>
    /// <param name="text">Текст для рендеринга</param>
    /// <param name="x">X позиция (левый край)</param>
    /// <param name="baselineY">Y позиция baseline</param>
    /// <param name="color">Цвет текста</param>
    /// <returns>Общая ширина отрендеренного текста</returns>
    public static float DrawString(string text, float x, float baselineY, Color32 color)
    {
        if (string.IsNullOrEmpty(text))
            return 0f;

        float totalWidth = 0f;
        float currentX = x;

        for (int i = 0; i < text.Length; i++)
        {
            uint codepoint = text[i];

            // Handle surrogate pairs
            if (char.IsHighSurrogate((char)codepoint) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, text[i + 1]);
                i++;
            }

            float advance = DrawGlyph(codepoint, currentX, baselineY, color);
            currentX += advance;
            totalWidth += advance;
        }

        return totalWidth;
    }

    /// <summary>
    /// Измеряет ширину строки без рендеринга.
    /// </summary>
    public static float MeasureString(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0f;

        var fontAsset = UniTextMeshGenerator.currentFontAsset;
        if (fontAsset == null)
            return 0f;

        float scale = UniTextMeshGenerator.scale;
        float totalWidth = 0f;

        for (int i = 0; i < text.Length; i++)
        {
            uint codepoint = text[i];

            // Handle surrogate pairs
            if (char.IsHighSurrogate((char)codepoint) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, text[i + 1]);
                i++;
            }

            var glyph = GetGlyph(fontAsset, codepoint);
            if (glyph != null)
            {
                totalWidth += glyph.metrics.horizontalAdvance * scale;
            }
        }

        return totalWidth;
    }

    /// <summary>
    /// Рендерит StringBuilder в указанной позиции (zero-allocation).
    /// </summary>
    public static float DrawString(StringBuilder sb, float x, float baselineY, Color32 color)
    {
        if (sb == null || sb.Length == 0)
            return 0f;

        float totalWidth = 0f;
        float currentX = x;
        int len = sb.Length;

        for (int i = 0; i < len; i++)
        {
            uint codepoint = sb[i];

            // Handle surrogate pairs
            if (char.IsHighSurrogate((char)codepoint) && i + 1 < len && char.IsLowSurrogate(sb[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, sb[i + 1]);
                i++;
            }

            float advance = DrawGlyph(codepoint, currentX, baselineY, color);
            currentX += advance;
            totalWidth += advance;
        }

        return totalWidth;
    }

    /// <summary>
    /// Измеряет ширину StringBuilder без рендеринга (zero-allocation).
    /// </summary>
    public static float MeasureString(StringBuilder sb)
    {
        if (sb == null || sb.Length == 0)
            return 0f;

        var fontAsset = UniTextMeshGenerator.currentFontAsset;
        if (fontAsset == null)
            return 0f;

        float scale = UniTextMeshGenerator.scale;
        float totalWidth = 0f;
        int len = sb.Length;

        for (int i = 0; i < len; i++)
        {
            uint codepoint = sb[i];

            // Handle surrogate pairs
            if (char.IsHighSurrogate((char)codepoint) && i + 1 < len && char.IsLowSurrogate(sb[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, sb[i + 1]);
                i++;
            }

            var glyph = GetGlyph(fontAsset, codepoint);
            if (glyph != null)
            {
                totalWidth += glyph.metrics.horizontalAdvance * scale;
            }
        }

        return totalWidth;
    }

    /// <summary>
    /// Получить Glyph для codepoint, добавляя динамически если нужно.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Glyph GetGlyph(UniTextFontAsset fontAsset, uint codepoint)
    {
        var charTable = fontAsset.CharacterLookupTable;
        if (charTable != null && charTable.TryGetValue(codepoint, out var character) && character?.glyph != null)
        {
            return character.glyph;
        }

        // Пробуем добавить динамически
        if (fontAsset.TryAddCharacter(codepoint, out var addedChar) && addedChar?.glyph != null)
        {
            return addedChar.glyph;
        }

        return null;
    }
}