using UnityEngine;
using UnityEngine.TextCore;

/// <summary>
/// Статический helper для отрисовки линий (underline, strikethrough).
/// Использует TMP's 3-quad approach: левый край + середина (растягиваемая) + правый край.
/// Требует глиф подчёркивания '_' (0x5F) для правильных SDF краёв.
/// </summary>
public static class LineRenderHelper
{
    // Кешированные данные глифа '_'
    private static Glyph cachedUnderscoreGlyph;
    private static int cachedFontInstanceId;

    /// <summary>
    /// Нарисовать линию используя TMP's 3-quad подход.
    /// 12 вершин, 18 индексов треугольников.
    /// </summary>
    /// <param name="startX">Начало линии X</param>
    /// <param name="endX">Конец линии X</param>
    /// <param name="baselineY">Y позиция baseline</param>
    /// <param name="lineYOffset">Offset от baseline (underlineOffset или strikethroughOffset)</param>
    /// <param name="color">Цвет линии</param>
    public static void DrawLine(float startX, float endX, float baselineY, float lineYOffset, Color32 color)
    {
        var fontAsset = UniTextMeshGenerator.currentFontAsset;
        if (fontAsset == null)
            return;

        // Получаем глиф '_'
        var underscoreGlyph = GetUnderscoreGlyph(fontAsset);
        if (underscoreGlyph == null || underscoreGlyph.glyphRect.width == 0)
            return;

        float scale = UniTextMeshGenerator.scale;
        float xScale = UniTextMeshGenerator.xScale;
        float padding = fontAsset.AtlasPadding;
        float atlasWidth = fontAsset.AtlasWidth;
        float atlasHeight = fontAsset.AtlasHeight;

        var underscoreRect = underscoreGlyph.glyphRect;
        var underscoreMetrics = underscoreGlyph.metrics;

        // Толщина линии из FaceInfo
        float lineThickness = fontAsset.FaceInfo.underlineThickness;

        // Start и End позиции
        float y = baselineY + lineYOffset;
        Vector3 start = new Vector3(startX, y, 0);
        Vector3 end = new Vector3(endX, y, 0);

        // Segment width = половина ширины underscore * scale
        float segmentWidth = underscoreMetrics.width * 0.5f * scale;

        // Если линия короче полной ширины underscore, корректируем
        if (end.x - start.x < underscoreMetrics.width * scale)
        {
            segmentWidth = (end.x - start.x) * 0.5f;
        }

        // Thickness с padding
        float thickness = (lineThickness + padding) * scale;
        float paddingScaled = padding * scale;

        var verts = UniTextMeshGenerator.Vertices;
        var uvs0 = UniTextMeshGenerator.Uvs0;
        var uvs2 = UniTextMeshGenerator.Uvs2;
        var colors = UniTextMeshGenerator.Colors;
        var tris = UniTextMeshGenerator.Triangles;

        int vertIdx = UniTextMeshGenerator.vertexCount;
        int triIdx = UniTextMeshGenerator.triangleCount;

        #region VERTICES (12 vertices = 3 quads)

        // Left quad (vertices 0-3)
        verts[vertIdx + 0] = start + new Vector3(0, -thickness, 0);
        verts[vertIdx + 1] = start + new Vector3(0, paddingScaled, 0);
        verts[vertIdx + 2] = verts[vertIdx + 1] + new Vector3(segmentWidth, 0, 0);
        verts[vertIdx + 3] = verts[vertIdx + 0] + new Vector3(segmentWidth, 0, 0);

        // Middle quad (vertices 4-7) - stretches between left and right
        verts[vertIdx + 4] = verts[vertIdx + 3];
        verts[vertIdx + 5] = verts[vertIdx + 2];
        verts[vertIdx + 6] = end + new Vector3(-segmentWidth, paddingScaled, 0);
        verts[vertIdx + 7] = end + new Vector3(-segmentWidth, -thickness, 0);

        // Right quad (vertices 8-11)
        verts[vertIdx + 8] = verts[vertIdx + 7];
        verts[vertIdx + 9] = verts[vertIdx + 6];
        verts[vertIdx + 10] = end + new Vector3(0, paddingScaled, 0);
        verts[vertIdx + 11] = end + new Vector3(0, -thickness, 0);

        #endregion

        #region UV0 (texture coordinates)

        float startPadding = padding;
        float endPadding = padding;

        Vector4 uv0 = new Vector4(
            (underscoreRect.x - startPadding) / atlasWidth,
            (underscoreRect.y - padding) / atlasHeight,
            0, xScale);

        Vector4 uv1 = new Vector4(
            uv0.x,
            (underscoreRect.y + underscoreRect.height + padding) / atlasHeight,
            0, xScale);

        Vector4 uv2 = new Vector4(
            (underscoreRect.x - startPadding + underscoreRect.width * 0.5f) / atlasWidth,
            uv1.y,
            0, xScale);

        Vector4 uv3 = new Vector4(uv2.x, uv0.y, 0, xScale);

        Vector4 uv4 = new Vector4(
            (underscoreRect.x + endPadding + underscoreRect.width * 0.5f) / atlasWidth,
            uv1.y,
            0, xScale);

        Vector4 uv5 = new Vector4(uv4.x, uv0.y, 0, xScale);

        Vector4 uv6 = new Vector4(
            (underscoreRect.x + endPadding + underscoreRect.width) / atlasWidth,
            uv1.y,
            0, xScale);

        Vector4 uv7 = new Vector4(uv6.x, uv0.y, 0, xScale);

        // Left quad UVs
        uvs0[vertIdx + 0] = uv0;
        uvs0[vertIdx + 1] = uv1;
        uvs0[vertIdx + 2] = uv2;
        uvs0[vertIdx + 3] = uv3;

        // Middle quad UVs - sample exact center of underscore glyph with half-pixel offset
        float centerU = (underscoreRect.x + underscoreRect.width * 0.5f) / atlasWidth;
        float halfPixelOffset = 0.5f / atlasWidth;
        uvs0[vertIdx + 4] = new Vector4(centerU - halfPixelOffset, uv0.y, 0, xScale);
        uvs0[vertIdx + 5] = new Vector4(centerU - halfPixelOffset, uv1.y, 0, xScale);
        uvs0[vertIdx + 6] = new Vector4(centerU + halfPixelOffset, uv1.y, 0, xScale);
        uvs0[vertIdx + 7] = new Vector4(centerU + halfPixelOffset, uv0.y, 0, xScale);

        // Right quad UVs
        uvs0[vertIdx + 8] = uv5;
        uvs0[vertIdx + 9] = uv4;
        uvs0[vertIdx + 10] = uv6;
        uvs0[vertIdx + 11] = uv7;

        #endregion

        #region UV2 (SDF scale - normalized X position along line)

        float totalWidth = end.x - start.x;
        if (totalWidth < 0.001f) totalWidth = 1f;

        float maxUvX_Left = (verts[vertIdx + 2].x - start.x) / totalWidth;
        float minUvX_Mid = (verts[vertIdx + 4].x - start.x) / totalWidth;
        float maxUvX_Mid = (verts[vertIdx + 6].x - start.x) / totalWidth;
        float minUvX_Right = (verts[vertIdx + 8].x - start.x) / totalWidth;

        // Left quad UV2
        uvs2[vertIdx + 0] = new Vector2(0, 0);
        uvs2[vertIdx + 1] = new Vector2(0, 1);
        uvs2[vertIdx + 2] = new Vector2(maxUvX_Left, 1);
        uvs2[vertIdx + 3] = new Vector2(maxUvX_Left, 0);

        // Middle quad UV2
        uvs2[vertIdx + 4] = new Vector2(minUvX_Mid, 0);
        uvs2[vertIdx + 5] = new Vector2(minUvX_Mid, 1);
        uvs2[vertIdx + 6] = new Vector2(maxUvX_Mid, 1);
        uvs2[vertIdx + 7] = new Vector2(maxUvX_Mid, 0);

        // Right quad UV2
        uvs2[vertIdx + 8] = new Vector2(minUvX_Right, 0);
        uvs2[vertIdx + 9] = new Vector2(minUvX_Right, 1);
        uvs2[vertIdx + 10] = new Vector2(1, 1);
        uvs2[vertIdx + 11] = new Vector2(1, 0);

        #endregion

        #region COLORS

        for (int i = 0; i < 12; i++)
        {
            colors[vertIdx + i] = color;
        }

        #endregion

        #region TRIANGLES (3 quads = 18 indices)

        // Left quad
        tris[triIdx + 0] = vertIdx + 0;
        tris[triIdx + 1] = vertIdx + 1;
        tris[triIdx + 2] = vertIdx + 2;
        tris[triIdx + 3] = vertIdx + 2;
        tris[triIdx + 4] = vertIdx + 3;
        tris[triIdx + 5] = vertIdx + 0;

        // Middle quad
        tris[triIdx + 6] = vertIdx + 4;
        tris[triIdx + 7] = vertIdx + 5;
        tris[triIdx + 8] = vertIdx + 6;
        tris[triIdx + 9] = vertIdx + 6;
        tris[triIdx + 10] = vertIdx + 7;
        tris[triIdx + 11] = vertIdx + 4;

        // Right quad
        tris[triIdx + 12] = vertIdx + 8;
        tris[triIdx + 13] = vertIdx + 9;
        tris[triIdx + 14] = vertIdx + 10;
        tris[triIdx + 15] = vertIdx + 10;
        tris[triIdx + 16] = vertIdx + 11;
        tris[triIdx + 17] = vertIdx + 8;

        #endregion

        UniTextMeshGenerator.vertexCount += 12;
        UniTextMeshGenerator.triangleCount += 18;
    }

    /// <summary>
    /// Получить глиф подчёркивания '_' (кешируется per font).
    /// </summary>
    private static Glyph GetUnderscoreGlyph(UniTextFontAsset fontAsset)
    {
        int fontId = fontAsset.GetInstanceID();

        if (cachedUnderscoreGlyph != null && cachedFontInstanceId == fontId)
            return cachedUnderscoreGlyph;

        cachedUnderscoreGlyph = null;
        cachedFontInstanceId = fontId;

        const uint underscoreCodepoint = '_';

        // Сначала ищем в characterTable
        var charTable = fontAsset.CharacterLookupTable;
        if (charTable != null && charTable.TryGetValue(underscoreCodepoint, out var character) && character?.glyph != null)
        {
            cachedUnderscoreGlyph = character.glyph;
            return cachedUnderscoreGlyph;
        }

        // Пробуем добавить динамически
        if (fontAsset.TryAddCharacter(underscoreCodepoint, out var addedChar) && addedChar?.glyph != null)
        {
            cachedUnderscoreGlyph = addedChar.glyph;
        }

        return cachedUnderscoreGlyph;
    }

    /// <summary>
    /// Сброс кеша. Вызывается при domain reload.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        cachedUnderscoreGlyph = null;
        cachedFontInstanceId = 0;
    }
}
