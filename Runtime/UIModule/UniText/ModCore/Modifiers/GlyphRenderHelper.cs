using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.TextCore;


public static class GlyphRenderHelper
{
    public static float DrawGlyph(UniTextFontProvider fontProvider, uint codepoint, float x, float baselineY, Color32 color)
    {
        var currentFont = UniTextMeshGenerator.currentFont;
        if (currentFont == null || fontProvider == null)
            return 0f;

        var glyph = GetGlyph(fontProvider, codepoint, out var glyphFont);
        if (glyph == null) return 0f;

        // Only render if this glyph belongs to the current font being rendered
        if (glyphFont != currentFont)
            return glyph.metrics.horizontalAdvance * UniTextMeshGenerator.scale;

        var glyphRect = glyph.glyphRect;
        var metrics = glyph.metrics;

        if (glyphRect.width == 0 || glyphRect.height == 0)
            return metrics.horizontalAdvance * UniTextMeshGenerator.scale;

        var scale = UniTextMeshGenerator.scale;
        var xScale = UniTextMeshGenerator.xScale;
        float padding = currentFont.AtlasPadding;
        float atlasWidth = currentFont.AtlasWidth;
        float atlasHeight = currentFont.AtlasHeight;
        var padding2 = padding * 2;

        var invAtlasWidth = 1f / atlasWidth;
        var invAtlasHeight = 1f / atlasHeight;

        var bearingXScaled = (metrics.horizontalBearingX - padding) * scale;
        var bearingYScaled = (metrics.horizontalBearingY + padding) * scale;
        var heightScaled = (metrics.height + padding2) * scale;
        var widthScaled = (metrics.width + padding2) * scale;

        var tlX = x + bearingXScaled;
        var tlY = baselineY + bearingYScaled;
        var blY = tlY - heightScaled;
        var trX = tlX + widthScaled;

        var uvBLx = (glyphRect.x - padding) * invAtlasWidth;
        var uvBLy = (glyphRect.y - padding) * invAtlasHeight;
        var uvTLy = (glyphRect.y + glyphRect.height + padding) * invAtlasHeight;
        var uvTRx = (glyphRect.x + glyphRect.width + padding) * invAtlasWidth;

        var verts = UniTextMeshGenerator.Vertices;
        var uvData = UniTextMeshGenerator.Uvs0;
        var cols = UniTextMeshGenerator.Colors;
        var tris = UniTextMeshGenerator.Triangles;

        var vertIdx = UniTextMeshGenerator.vertexCount;
        var triIdx = UniTextMeshGenerator.triangleCount;

        var i0 = vertIdx;
        var i1 = vertIdx + 1;
        var i2 = vertIdx + 2;
        var i3 = vertIdx + 3;

        ref var v0 = ref verts[i0];
        v0.x = tlX;
        v0.y = blY;
        v0.z = 0;
        ref var v1 = ref verts[i1];
        v1.x = tlX;
        v1.y = tlY;
        v1.z = 0;
        ref var v2 = ref verts[i2];
        v2.x = trX;
        v2.y = tlY;
        v2.z = 0;
        ref var v3 = ref verts[i3];
        v3.x = trX;
        v3.y = blY;
        v3.z = 0;

        ref var uv0 = ref uvData[i0];
        uv0.x = uvBLx;
        uv0.y = uvBLy;
        uv0.z = 0;
        uv0.w = xScale;
        ref var uv1 = ref uvData[i1];
        uv1.x = uvBLx;
        uv1.y = uvTLy;
        uv1.z = 0;
        uv1.w = xScale;
        ref var uv2 = ref uvData[i2];
        uv2.x = uvTRx;
        uv2.y = uvTLy;
        uv2.z = 0;
        uv2.w = xScale;
        ref var uv3 = ref uvData[i3];
        uv3.x = uvTRx;
        uv3.y = uvBLy;
        uv3.z = 0;
        uv3.w = xScale;

        cols[i0] = color;
        cols[i1] = color;
        cols[i2] = color;
        cols[i3] = color;

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

    public static float DrawString(UniTextFontProvider fontProvider, string text, float x, float baselineY, Color32 color)
    {
        if (string.IsNullOrEmpty(text) || fontProvider == null)
            return 0f;

        var totalWidth = 0f;
        var currentX = x;

        for (var i = 0; i < text.Length; i++)
        {
            uint codepoint = text[i];

            if (char.IsHighSurrogate((char)codepoint) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, text[i + 1]);
                i++;
            }

            var advance = DrawGlyph(fontProvider, codepoint, currentX, baselineY, color);
            currentX += advance;
            totalWidth += advance;
        }

        return totalWidth;
    }


    public static float MeasureString(UniTextFontProvider fontProvider, string text)
    {
        if (string.IsNullOrEmpty(text) || fontProvider == null)
            return 0f;

        var totalWidth = 0f;

        for (var i = 0; i < text.Length; i++)
        {
            uint codepoint = text[i];

            if (char.IsHighSurrogate((char)codepoint) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, text[i + 1]);
                i++;
            }

            var glyph = GetGlyph(fontProvider, codepoint, out var font);
            if (glyph != null)
            {
                var pointSize = font.FaceInfo.pointSize;
                var scale = pointSize > 0 ? fontProvider.FontSize / pointSize : 1f;
                totalWidth += glyph.metrics.horizontalAdvance * scale;
            }
        }

        return totalWidth;
    }


    public static float DrawString(UniTextFontProvider fontProvider, StringBuilder sb, float x, float baselineY, Color32 color)
    {
        if (sb == null || sb.Length == 0 || fontProvider == null)
            return 0f;

        var totalWidth = 0f;
        var currentX = x;
        var len = sb.Length;

        for (var i = 0; i < len; i++)
        {
            uint codepoint = sb[i];

            if (char.IsHighSurrogate((char)codepoint) && i + 1 < len && char.IsLowSurrogate(sb[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, sb[i + 1]);
                i++;
            }

            var advance = DrawGlyph(fontProvider, codepoint, currentX, baselineY, color);
            currentX += advance;
            totalWidth += advance;
        }

        return totalWidth;
    }


    public static float MeasureString(UniTextFontProvider fontProvider, StringBuilder sb)
    {
        if (sb == null || sb.Length == 0 || fontProvider == null)
            return 0f;

        var totalWidth = 0f;
        var len = sb.Length;

        for (var i = 0; i < len; i++)
        {
            uint codepoint = sb[i];

            if (char.IsHighSurrogate((char)codepoint) && i + 1 < len && char.IsLowSurrogate(sb[i + 1]))
            {
                codepoint = (uint)char.ConvertToUtf32((char)codepoint, sb[i + 1]);
                i++;
            }

            var glyph = GetGlyph(fontProvider, codepoint, out var font);
            if (glyph != null)
            {
                var pointSize = font.FaceInfo.pointSize;
                var scale = pointSize > 0 ? fontProvider.FontSize / pointSize : 1f;
                totalWidth += glyph.metrics.horizontalAdvance * scale;
            }
        }

        return totalWidth;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Glyph GetGlyph(UniTextFontProvider fontProvider, uint codepoint, out UniTextFont font)
    {
        var fontId = fontProvider.FindFontForCodepoint((int)codepoint, 0);
        font = fontProvider.GetFontAsset(fontId);

        var charTable = font.CharacterLookupTable;
        if (charTable != null && charTable.TryGetValue(codepoint, out var character) && character?.glyph != null)
            return character.glyph;

        return null;
    }
}
