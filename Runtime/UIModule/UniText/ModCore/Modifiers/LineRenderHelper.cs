using System;
using UnityEngine;
using UnityEngine.TextCore;


public static class LineRenderHelper
{
    [ThreadStatic] private static Glyph cachedUnderscoreGlyph;
    [ThreadStatic] private static UniTextFont cachedUnderscoreFont;
    [ThreadStatic] private static int cachedFontProviderId;


    public static void DrawLine(UniTextFontProvider fontProvider, float startX, float endX, float baselineY, float lineYOffset, Color32 color)
    {
        var gen = UniTextMeshGenerator.Current;
        if (gen == null || fontProvider == null)
            return;

        var currentFont = gen.font;
        if (currentFont == null)
            return;

        var underscoreGlyph = GetUnderscoreGlyph(fontProvider, out var glyphFont);
        if (underscoreGlyph == null || underscoreGlyph.glyphRect.width == 0)
            return;

        if (glyphFont != currentFont)
            return;

        gen.EnsureCapacity(12, 18);

        var scale = gen.scale;
        var xScaleVal = gen.xScale;
        float padding = currentFont.AtlasPadding;
        float atlasWidth = currentFont.AtlasWidth;
        float atlasHeight = currentFont.AtlasHeight;

        var underscoreRect = underscoreGlyph.glyphRect;
        var underscoreMetrics = underscoreGlyph.metrics;

        var lineThickness = currentFont.FaceInfo.underlineThickness;

        var y = baselineY + lineYOffset;
        var start = new Vector3(startX, y, 0);
        var end = new Vector3(endX, y, 0);

        var segmentWidth = underscoreMetrics.width * 0.5f * scale;

        if (end.x - start.x < underscoreMetrics.width * scale) segmentWidth = (end.x - start.x) * 0.5f;

        var thickness = (lineThickness + padding) * scale;
        var paddingScaled = padding * scale;

        var verts = gen.Vertices;
        var uvs0 = gen.Uvs0;
        var uvs1 = gen.Uvs1;
        var colors = gen.Colors;
        var tris = gen.Triangles;

        var vertIdx = gen.vertexCount;
        var triIdx = gen.triangleCount;

        #region VERTICES (12 vertices = 3 quads)

        verts[vertIdx + 0] = start + new Vector3(0, -thickness, 0);
        verts[vertIdx + 1] = start + new Vector3(0, paddingScaled, 0);
        verts[vertIdx + 2] = verts[vertIdx + 1] + new Vector3(segmentWidth, 0, 0);
        verts[vertIdx + 3] = verts[vertIdx + 0] + new Vector3(segmentWidth, 0, 0);

        verts[vertIdx + 4] = verts[vertIdx + 3];
        verts[vertIdx + 5] = verts[vertIdx + 2];
        verts[vertIdx + 6] = end + new Vector3(-segmentWidth, paddingScaled, 0);
        verts[vertIdx + 7] = end + new Vector3(-segmentWidth, -thickness, 0);

        verts[vertIdx + 8] = verts[vertIdx + 7];
        verts[vertIdx + 9] = verts[vertIdx + 6];
        verts[vertIdx + 10] = end + new Vector3(0, paddingScaled, 0);
        verts[vertIdx + 11] = end + new Vector3(0, -thickness, 0);

        #endregion

        #region UV0 (texture coordinates)

        var startPadding = padding;
        var endPadding = padding;

        var uv0 = new Vector4(
            (underscoreRect.x - startPadding) / atlasWidth,
            (underscoreRect.y - padding) / atlasHeight,
            0, xScaleVal);

        var uv1 = new Vector4(
            uv0.x,
            (underscoreRect.y + underscoreRect.height + padding) / atlasHeight,
            0, xScaleVal);

        var uv2 = new Vector4(
            (underscoreRect.x - startPadding + underscoreRect.width * 0.5f) / atlasWidth,
            uv1.y,
            0, xScaleVal);

        var uv3 = new Vector4(uv2.x, uv0.y, 0, xScaleVal);

        var uv4 = new Vector4(
            (underscoreRect.x + endPadding + underscoreRect.width * 0.5f) / atlasWidth,
            uv1.y,
            0, xScaleVal);

        var uv5 = new Vector4(uv4.x, uv0.y, 0, xScaleVal);

        var uv6 = new Vector4(
            (underscoreRect.x + endPadding + underscoreRect.width) / atlasWidth,
            uv1.y,
            0, xScaleVal);

        var uv7 = new Vector4(uv6.x, uv0.y, 0, xScaleVal);

        uvs0[vertIdx + 0] = uv0;
        uvs0[vertIdx + 1] = uv1;
        uvs0[vertIdx + 2] = uv2;
        uvs0[vertIdx + 3] = uv3;

        var centerU = (underscoreRect.x + underscoreRect.width * 0.5f) / atlasWidth;
        var halfPixelOffset = 0.5f / atlasWidth;
        uvs0[vertIdx + 4] = new Vector4(centerU - halfPixelOffset, uv0.y, 0, xScaleVal);
        uvs0[vertIdx + 5] = new Vector4(centerU - halfPixelOffset, uv1.y, 0, xScaleVal);
        uvs0[vertIdx + 6] = new Vector4(centerU + halfPixelOffset, uv1.y, 0, xScaleVal);
        uvs0[vertIdx + 7] = new Vector4(centerU + halfPixelOffset, uv0.y, 0, xScaleVal);

        uvs0[vertIdx + 8] = uv5;
        uvs0[vertIdx + 9] = uv4;
        uvs0[vertIdx + 10] = uv6;
        uvs0[vertIdx + 11] = uv7;

        #endregion

        #region UV1 (SDF scale - normalized X position along line)

        var totalWidth = end.x - start.x;
        if (totalWidth < 0.001f) totalWidth = 1f;

        var maxUvX_Left = (verts[vertIdx + 2].x - start.x) / totalWidth;
        var minUvX_Mid = (verts[vertIdx + 4].x - start.x) / totalWidth;
        var maxUvX_Mid = (verts[vertIdx + 6].x - start.x) / totalWidth;
        var minUvX_Right = (verts[vertIdx + 8].x - start.x) / totalWidth;

        uvs1[vertIdx + 0] = new Vector2(0, 0);
        uvs1[vertIdx + 1] = new Vector2(0, 1);
        uvs1[vertIdx + 2] = new Vector2(maxUvX_Left, 1);
        uvs1[vertIdx + 3] = new Vector2(maxUvX_Left, 0);

        uvs1[vertIdx + 4] = new Vector2(minUvX_Mid, 0);
        uvs1[vertIdx + 5] = new Vector2(minUvX_Mid, 1);
        uvs1[vertIdx + 6] = new Vector2(maxUvX_Mid, 1);
        uvs1[vertIdx + 7] = new Vector2(maxUvX_Mid, 0);

        uvs1[vertIdx + 8] = new Vector2(minUvX_Right, 0);
        uvs1[vertIdx + 9] = new Vector2(minUvX_Right, 1);
        uvs1[vertIdx + 10] = new Vector2(1, 1);
        uvs1[vertIdx + 11] = new Vector2(1, 0);

        #endregion

        #region COLORS

        for (var i = 0; i < 12; i++) colors[vertIdx + i] = color;

        #endregion

        #region TRIANGLES (3 quads = 18 indices)

        tris[triIdx + 0] = vertIdx + 0;
        tris[triIdx + 1] = vertIdx + 1;
        tris[triIdx + 2] = vertIdx + 2;
        tris[triIdx + 3] = vertIdx + 2;
        tris[triIdx + 4] = vertIdx + 3;
        tris[triIdx + 5] = vertIdx + 0;

        tris[triIdx + 6] = vertIdx + 4;
        tris[triIdx + 7] = vertIdx + 5;
        tris[triIdx + 8] = vertIdx + 6;
        tris[triIdx + 9] = vertIdx + 6;
        tris[triIdx + 10] = vertIdx + 7;
        tris[triIdx + 11] = vertIdx + 4;

        tris[triIdx + 12] = vertIdx + 8;
        tris[triIdx + 13] = vertIdx + 9;
        tris[triIdx + 14] = vertIdx + 10;
        tris[triIdx + 15] = vertIdx + 10;
        tris[triIdx + 16] = vertIdx + 11;
        tris[triIdx + 17] = vertIdx + 8;

        #endregion

        gen.vertexCount += 12;
        gen.triangleCount += 18;
    }


    private static Glyph GetUnderscoreGlyph(UniTextFontProvider fontProvider, out UniTextFont font)
    {
        var providerId = fontProvider.GetHashCode();

        if (cachedUnderscoreGlyph != null && cachedFontProviderId == providerId)
        {
            font = cachedUnderscoreFont;
            return cachedUnderscoreGlyph;
        }

        cachedUnderscoreGlyph = null;
        cachedUnderscoreFont = null;
        cachedFontProviderId = providerId;

        const uint underscoreCodepoint = '_';

        var fontId = fontProvider.FindFontForCodepoint((int)underscoreCodepoint);
        font = fontProvider.GetFontAsset(fontId);

        var charTable = font.CharacterLookupTable;
        if (charTable != null && charTable.TryGetValue(underscoreCodepoint, out var character) &&
            character?.glyph != null)
        {
            cachedUnderscoreGlyph = character.glyph;
            cachedUnderscoreFont = font;
        }

        return cachedUnderscoreGlyph;
    }
}
