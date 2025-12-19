using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;

public struct UniTextMeshPair
{
    public Mesh mesh;
    public Material material;

    public UniTextMeshPair(Mesh mesh, Material material)
    {
        this.mesh = mesh;
        this.material = material;
    }
}

public class UniTextMeshGenerator
{
    public static int currentCluster;
    public static float currentX;
    public static float currentY;
    public static float currentWidth;
    public static float currentHeight;
    public static float currentBaselineY;
    public static float scale;
    public static float xScale;
    public static Color32 currentDefaultColor;
    public static UniTextFontAsset currentFontAsset;
    public static float offsetX;
    public static float offsetY;
    public static float rectWidth;
    public static HorizontalAlignment horizontalAlignment;
    public static int vertexCount;
    public static int triangleCount;
    private static Dictionary<int, LSList<int>> glyphsByAtlas;
    private readonly UniTextFontProvider fontProvider;
    private readonly UniTextBuffers buf;
    private Canvas canvas;
    private float lossyScale = 1f;
    public Action OnAfterGlyphsPerFont;
    public Action OnBeforeMesh;
    public Action OnGlyph;
    public Action OnRebuildEnd;
    public Action OnRebuildStart;
    private Rect rectOffset;

    public UniTextMeshGenerator(UniTextFontProvider fontProvider, UniTextBuffers uniTextBuffers)
    {
        this.fontProvider = fontProvider ?? throw new ArgumentNullException(nameof(fontProvider));
        buf = uniTextBuffers ?? throw new ArgumentNullException(nameof(uniTextBuffers));
    }

    public float FontSize { get; set; } = 36f;
    public Color32 DefaultColor { get; set; } = new(255, 255, 255, 255);

    public static Vector3[] Vertices => SharedPipelineComponents.MeshVertices;
    public static Vector4[] Uvs0 => SharedPipelineComponents.MeshUvs0;
    public static Vector2[] Uvs2 => SharedPipelineComponents.MeshUvs2;
    public static Color32[] Colors => SharedPipelineComponents.MeshColors32;
    public static int[] Triangles => SharedPipelineComponents.MeshTriangles;
    public static Vector3[] Normals => SharedPipelineComponents.MeshNormals;
    public static Vector4[] Tangents => SharedPipelineComponents.MeshTangents;

    public void SetCanvasParameters(Transform transform, Canvas canvas)
    {
        this.canvas = canvas;
        lossyScale = transform?.lossyScale.x ?? 1f;
    }

    public void SetRectOffset(Rect rect)
    {
        rectOffset = rect;
        rectWidth = rect.width;
    }

    public void SetHorizontalAlignment(HorizontalAlignment alignment)
    {
        horizontalAlignment = alignment;
    }


    public LSList<UniTextMeshPair> GenerateMeshes(ReadOnlySpan<PositionedGlyph> glyphs, Func<Mesh> meshProvider)
    {
        OnRebuildStart?.Invoke();

        var glyphsByFont = SharedPipelineComponents.GlyphsByFont;
        var resultBuffer = SharedPipelineComponents.MeshResultBuffer;

        SharedPipelineComponents.ClearGlyphsByFont();
        resultBuffer.Clear();

        if (glyphs.Length == 0)
            return resultBuffer;

        var glyphLen = glyphs.Length;
        for (var i = 0; i < glyphLen; i++)
        {
            var fontId = glyphs[i].fontId;
            if (!glyphsByFont.TryGetValue(fontId, out var list))
            {
                list = SharedPipelineComponents.AcquireGlyphIndexList();
                glyphsByFont[fontId] = list;
            }

            list.Add(i);
        }

        var positionedGlyphs = buf.positionedGlyphs;
        foreach (var kvp in glyphsByFont)
        {
            var fontId = kvp.Key;
            var glyphIndices = kvp.Value;

            var fontAsset = fontProvider.GetFontAsset(fontId);
            if (fontAsset == null) continue;

            var glyphLookup = fontAsset.GlyphLookupTable;
            var hasMultipleAtlases = fontAsset.AtlasTextures != null && fontAsset.AtlasTextures.Length > 1;

            if (!hasMultipleAtlases)
            {
                var mesh = meshProvider?.Invoke() ?? new Mesh();
                GenerateMeshForFont(mesh, glyphIndices, positionedGlyphs, fontAsset);
                resultBuffer.Add(new UniTextMeshPair(mesh, fontAsset.Material));
            }
            else
            {
                glyphsByAtlas ??= new Dictionary<int, LSList<int>>();
                glyphsByAtlas.Clear();

                var count = glyphIndices.Count;
                for (var i = 0; i < count; i++)
                {
                    var glyphIndex = glyphIndices[i];
                    ref readonly var glyph = ref positionedGlyphs[glyphIndex];
                    var atlasIndex = 0;
                    if (glyphLookup.TryGetValue((uint)glyph.glyphId, out var glyphData) && glyphData != null)
                        atlasIndex = glyphData.atlasIndex;

                    if (!glyphsByAtlas.TryGetValue(atlasIndex, out var atlasList))
                    {
                        atlasList = SharedPipelineComponents.AcquireGlyphIndexList();
                        glyphsByAtlas[atlasIndex] = atlasList;
                    }

                    atlasList.Add(glyphIndex);
                }

                foreach (var atlasKvp in glyphsByAtlas)
                {
                    var atlasIndex = atlasKvp.Key;
                    var atlasIndices = atlasKvp.Value;

                    var mesh = meshProvider?.Invoke() ?? new Mesh();
                    GenerateMeshForFont(mesh, atlasIndices, positionedGlyphs, fontAsset);
                    var atlasMat = fontAsset.material;
                    resultBuffer.Add(new UniTextMeshPair(mesh, atlasMat));

                    SharedPipelineComponents.ReleaseGlyphIndexList(atlasIndices);
                }

                glyphsByAtlas.Clear();
            }
        }

        if (buf != null)
            buf.hasValidGlyphCache = true;

        OnRebuildEnd?.Invoke();

        return resultBuffer;
    }

    private void GenerateMeshForFont(Mesh mesh, LSList<int> glyphIndices, PositionedGlyph[] positionedGlyphs,
        UniTextFontAsset fontAsset)
    {
        var glyphCount = glyphIndices.Count;
        var maxVertexCount = glyphCount * 4 + glyphCount * 8;
        var maxTriangleCount = glyphCount * 6 + glyphCount * 12;

        EnsureBufferCapacity(maxVertexCount, maxTriangleCount);

        var pointSize = fontAsset.FaceInfo.pointSize;
        var scale = pointSize > 0 ? FontSize / pointSize : 1f;
        float atlasWidth = fontAsset.AtlasWidth;
        float atlasHeight = fontAsset.AtlasHeight;
        float padding = fontAsset.AtlasPadding;
        var invAtlasWidth = 1f / atlasWidth;
        var invAtlasHeight = 1f / atlasHeight;
        var padding2 = padding * 2;

        var offsetX = rectOffset.xMin;
        var offsetY = rectOffset.yMax;

        var xScale = CalculateXScale(scale);

        UniTextMeshGenerator.scale = scale;
        UniTextMeshGenerator.xScale = xScale;
        UniTextMeshGenerator.offsetX = offsetX;
        UniTextMeshGenerator.offsetY = offsetY;
        currentDefaultColor = DefaultColor;
        currentFontAsset = fontAsset;
        vertexCount = 0;
        triangleCount = 0;

        OnBeforeMesh?.Invoke();

        var glyphLookup = fontAsset.GlyphLookupTable;
        var defaultColor = DefaultColor;

        buf.EnsureGlyphCacheCapacity(buf.shapedGlyphCount);
        var glyphCache = buf.glyphDataCache;
        var useCache = buf.hasValidGlyphCache;

        var foundCount = 0;
        var notFoundCount = 0;
        var whitespaceCount = 0;

        var verts = Vertices;
        var uvData = Uvs0;
        var cols = Colors;
        var tris = Triangles;

        for (var i = 0; i < glyphCount; i++)
        {
            var glyphIndex = glyphIndices[i];
            ref var glyph = ref positionedGlyphs[glyphIndex];
            var cacheIndex = glyph.shapedGlyphIndex;

            ref var cachedData = ref glyphCache[cacheIndex];
            if (!useCache || !cachedData.isValid)
            {
                var glyphId = (uint)glyph.glyphId;
                if (!glyphLookup.TryGetValue(glyphId, out var glyphData) || glyphData == null)
                {
                    notFoundCount++;
                    cachedData.isValid = false;
                    continue;
                }

                var rect = glyphData.glyphRect;
                var metrics = glyphData.metrics;
                cachedData.rectX = rect.x;
                cachedData.rectY = rect.y;
                cachedData.rectWidth = rect.width;
                cachedData.rectHeight = rect.height;
                cachedData.bearingX = metrics.horizontalBearingX;
                cachedData.bearingY = metrics.horizontalBearingY;
                cachedData.width = metrics.width;
                cachedData.height = metrics.height;
                cachedData.isValid = true;
            }

            if (cachedData.rectWidth == 0 || cachedData.rectHeight == 0)
            {
                whitespaceCount++;
                continue;
            }

            foundCount++;

            var cluster = glyph.cluster;

            var bearingXScaled = (cachedData.bearingX - padding) * scale;
            var bearingYScaled = (cachedData.bearingY + padding) * scale;
            var heightScaled = (cachedData.height + padding2) * scale;
            var widthScaled = (cachedData.width + padding2) * scale;

            var tlX = offsetX + glyph.x + bearingXScaled;
            var tlY = offsetY - glyph.y + bearingYScaled;
            var blY = tlY - heightScaled;
            var trX = tlX + widthScaled;

            glyph.left = tlX;
            glyph.top = tlY;
            glyph.right = trX;
            glyph.bottom = blY;

            var uvBLx = (cachedData.rectX - padding) * invAtlasWidth;
            var uvBLy = (cachedData.rectY - padding) * invAtlasHeight;
            var uvTLy = (cachedData.rectY + cachedData.rectHeight + padding) * invAtlasHeight;
            var uvTRx = (cachedData.rectX + cachedData.rectWidth + padding) * invAtlasWidth;

            var i0 = vertexCount;
            var i1 = vertexCount + 1;
            var i2 = vertexCount + 2;
            var i3 = vertexCount + 3;

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

            cols[i0] = defaultColor;
            cols[i1] = defaultColor;
            cols[i2] = defaultColor;
            cols[i3] = defaultColor;

            tris[triangleCount] = i0;
            tris[triangleCount + 1] = i1;
            tris[triangleCount + 2] = i2;
            tris[triangleCount + 3] = i2;
            tris[triangleCount + 4] = i3;
            tris[triangleCount + 5] = i0;

            currentCluster = cluster;
            currentX = glyph.x;
            currentY = glyph.y;
            currentWidth = widthScaled;
            currentHeight = heightScaled;
            currentBaselineY = offsetY - glyph.y;

            vertexCount += 4;
            triangleCount += 6;

            OnGlyph?.Invoke();
        }

        OnAfterGlyphsPerFont?.Invoke();

        mesh.Clear();

        if (vertexCount > 0)
        {
            mesh.SetVertices(Vertices, 0, vertexCount);
            mesh.SetNormals(Normals, 0, vertexCount);
            mesh.SetTangents(Tangents, 0, vertexCount);
            mesh.SetUVs(0, Uvs0, 0, vertexCount);
            mesh.SetUVs(1, Uvs2, 0, vertexCount);
            mesh.SetColors(Colors, 0, vertexCount);
            mesh.SetTriangles(Triangles, 0, triangleCount, 0);
        }
    }

    private static void EnsureBufferCapacity(int vertexCount, int triangleCount)
    {
        SharedPipelineComponents.EnsureMeshCapacity(vertexCount, triangleCount);
    }

    private float CalculateXScale(float scale)
    {
        if (canvas == null) return scale;

        var absLossyScale = Mathf.Abs(lossyScale);
        return scale * (canvas.worldCamera != null ? absLossyScale : 1f);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        glyphsByAtlas = null;
    }
}