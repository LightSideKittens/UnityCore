using System;
using System.Runtime.CompilerServices;
using UnityEngine;

public struct UniTextRenderData
{
    public Mesh mesh;
    public Material material;
    public Texture texture;
    public int fontId;

    public UniTextRenderData(Mesh mesh, Material material, Texture texture, int fontId)
    {
        this.mesh = mesh;
        this.material = material;
        this.texture = texture;
        this.fontId = fontId;
    }
}


public struct GeneratedMeshSegment
{
    public int fontId;
    public int atlasIndex;
    public int vertexStart;
    public int vertexCount;
    public int triangleStart;
    public int triangleCount;
    public Material material;
    public Texture texture;
}

public class UniTextMeshGenerator
{
    [ThreadStatic] private static UniTextMeshGenerator current;
    public static UniTextMeshGenerator Current => current;

    public int currentCluster;
    public float x;
    public float y;
    public float width;
    public float height;
    public float baselineY;
    public float scale;
    public float xScale;
    public Color32 defaultColor;
    public UniTextFont font;
    public float offsetX;
    public float offsetY;
    public float rectWidth;
    public HorizontalAlignment hAlignment;
    public int vertexCount;
    public int triangleCount;

    [ThreadStatic] private static FastIntDictionary<PooledList<int>> glyphsByAtlas;

    private PooledBuffer<Vector3> vertices;
    private PooledBuffer<Vector4> uvs0;
    private PooledBuffer<Vector2> uvs1;
    private PooledBuffer<Color32> colors;
    private PooledBuffer<int> triangles;
    private PooledList<GeneratedMeshSegment> generatedSegments;
    private bool hasGeneratedData;

    private readonly UniTextFontProvider fontProvider;
    private readonly UniTextBuffers buf;
    private Canvas canvas;
    private float lossyScale = 1f;
    private bool hasWorldCamera;
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
    public bool HasGeneratedData => hasGeneratedData;

    public Vector3[] Vertices => vertices.data;
    public Vector4[] Uvs0 => uvs0.data;
    public Color32[] Colors => colors.data;
    public int[] Triangles => triangles.data;

    public Vector2[] Uvs1
    {
        get
        {
            EnsureUvs1();
            return uvs1.data;
        }
    }

    private void EnsureUvs1()
    {
        if (uvs1.data != null) return;
        uvs1.Rent(vertices.Capacity);
        uvs1.count = vertexCount;
    }

    #region Instance Buffer Management

    private void RentInstanceBuffers(int estimatedVertices, int estimatedTriangles)
    {
        vertices.Rent(estimatedVertices);
        uvs0.Rent(estimatedVertices);
        colors.Rent(estimatedVertices);
        triangles.Rent(estimatedTriangles);
        generatedSegments ??= new PooledList<GeneratedMeshSegment>(4);
        generatedSegments.FakeClear();

        current = this;
    }

    public void ReturnInstanceBuffers()
    {
        current = null;

        vertices.Return();
        uvs0.Return();
        uvs1.Return();
        colors.Return();
        triangles.Return();
        hasGeneratedData = false;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int additionalVertices, int additionalTriangles)
    {
        var requiredVertices = vertexCount + additionalVertices;
        var requiredTriangles = triangleCount + additionalTriangles;

        if (requiredVertices > vertices.Capacity)
            GrowVertexBuffers(requiredVertices);

        if (requiredTriangles > triangles.Capacity)
            GrowTriangleBuffer(requiredTriangles);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowVertexBuffers(int required)
    {
        var newCapacity = Math.Max(required, vertices.Capacity * 2);
        var currentCount = vertexCount;

        GrowBuffer(ref vertices, newCapacity, currentCount);
        GrowBuffer(ref uvs0, newCapacity, currentCount);
        GrowBuffer(ref colors, newCapacity, currentCount);
        if (uvs1.data != null) GrowBuffer(ref uvs1, newCapacity, currentCount);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowTriangleBuffer(int required)
    {
        var newCapacity = Math.Max(required, triangles.Capacity * 2);
        GrowBuffer(ref triangles, newCapacity, triangleCount);
    }

    private static void GrowBuffer<T>(ref PooledBuffer<T> buffer, int newCapacity, int currentCount)
    {
        var oldData = buffer.data;

        var newData = UniTextArrayPool<T>.Rent(newCapacity);
        if (oldData != null && currentCount > 0)
            oldData.AsSpan(0, currentCount).CopyTo(newData);

        UniTextArrayPool<T>.Return(oldData);
        buffer.data = newData;
    }

    #endregion
    
    public void SetCanvasParametersCached(float lossyScale, bool hasWorldCamera)
    {
        this.lossyScale = lossyScale;
        this.hasWorldCamera = hasWorldCamera;
    }

    public void SetRectOffset(Rect rect)
    {
        rectOffset = rect;
        rectWidth = rect.width;
    }

    public void SetHorizontalAlignment(HorizontalAlignment alignment)
    {
        hAlignment = alignment;
    }

    private float CalculateXScale(float scale)
    {
        var absLossyScale = Mathf.Abs(lossyScale);
        return scale * (hasWorldCamera ? absLossyScale : 1f);
    }

    #region Parallel Mesh Generation
    
    public void GenerateMeshDataOnly(ReadOnlySpan<PositionedGlyph> glyphs)
    {
        OnRebuildStart?.Invoke();

        var glyphLen = glyphs.Length;
        var estimatedVertices = glyphLen * 4;
        var estimatedTriangles = glyphLen * 6;

        RentInstanceBuffers(estimatedVertices, estimatedTriangles);

        var glyphsByFont = SharedPipelineComponents.GlyphsByFont;
        SharedPipelineComponents.ClearGlyphsByFont();

        var lastFontId = -1;
        PooledList<int> lastList = null;

        for (var i = 0; i < glyphLen; i++)
        {
            var fontId = glyphs[i].fontId;
            if (lastFontId != fontId)
            {
                lastFontId = fontId;
                if (!glyphsByFont.TryGetValue(fontId, out var list))
                {
                    list = SharedPipelineComponents.AcquireGlyphIndexList(glyphLen);
                    glyphsByFont[fontId] = list;
                }
                lastList = list;
            }
            lastList.buffer[lastList.buffer.count++] = i;
        }

        var positionedGlyphs = buf.positionedGlyphs.data;
        foreach (var kvp in glyphsByFont)
        {
            var fontId = kvp.Key;
            var glyphIndices = kvp.Value;
            var fontAsset = fontProvider.GetFontAsset(fontId);
            var glyphLookup = fontAsset.GlyphLookupTable;
            var hasMultipleAtlases = fontAsset.AtlasTextures is { Length: > 1 };
            var material = fontProvider.GetMaterial(fontId);

            if (!hasMultipleAtlases)
            {
                var vertexStart = vertices.count;
                var triangleStart = triangles.count;

                GenerateMeshDataForFont(glyphIndices, positionedGlyphs, fontAsset);

                generatedSegments.Add(new GeneratedMeshSegment
                {
                    fontId = fontId,
                    atlasIndex = 0,
                    vertexStart = vertexStart,
                    vertexCount = vertices.count - vertexStart,
                    triangleStart = triangleStart,
                    triangleCount = triangles.count - triangleStart,
                    material = material,
                    texture = fontAsset.AtlasTexture
                });
            }
            else
            {
                glyphsByAtlas ??= new FastIntDictionary<PooledList<int>>();
                glyphsByAtlas.ClearFast();

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
                        atlasList = SharedPipelineComponents.AcquireGlyphIndexList(count);
                        glyphsByAtlas[atlasIndex] = atlasList;
                    }
                    atlasList.buffer[atlasList.buffer.count++] = glyphIndex;
                }

                foreach (var atlasKvp in glyphsByAtlas)
                {
                    var atlasIndex = atlasKvp.Key;
                    var atlasIndices = atlasKvp.Value;

                    var vertexStart = vertices.count;
                    var triangleStart = triangles.count;

                    GenerateMeshDataForFont(atlasIndices, positionedGlyphs, fontAsset);

                    var atlasTexture = fontAsset.AtlasTextures != null && atlasIndex < fontAsset.AtlasTextures.Length
                        ? fontAsset.AtlasTextures[atlasIndex]
                        : fontAsset.AtlasTexture;

                    generatedSegments.Add(new GeneratedMeshSegment
                    {
                        fontId = fontId,
                        atlasIndex = atlasIndex,
                        vertexStart = vertexStart,
                        vertexCount = vertices.count - vertexStart,
                        triangleStart = triangleStart,
                        triangleCount = triangles.count - triangleStart,
                        material = material,
                        texture = atlasTexture
                    });

                    SharedPipelineComponents.ReleaseGlyphIndexList(atlasIndices);
                }
                glyphsByAtlas.ClearFast();
            }
        }

        buf.hasValidGlyphCache = true;
        hasGeneratedData = true;
        OnRebuildEnd?.Invoke();
    }

    private void GenerateMeshDataForFont(PooledList<int> glyphIndices, PositionedGlyph[] positionedGlyphs, UniTextFont font)
    {
        var glyphCount = glyphIndices.Count;

        var pointSize = font.FaceInfo.pointSize;
        var scaleVal = pointSize > 0 ? FontSize / pointSize : 1f;
        float atlasWidth = font.AtlasWidth;
        float atlasHeight = font.AtlasHeight;
        float padding = font.AtlasPadding;
        var invAtlasWidth = 1f / atlasWidth;
        var invAtlasHeight = 1f / atlasHeight;
        var padding2 = padding * 2;

        var offX = rectOffset.xMin;
        var offY = rectOffset.yMax;
        var xScaleVal = CalculateXScale(scaleVal);

        scale = scaleVal;
        xScale = xScaleVal;
        offsetX = offX;
        offsetY = offY;
        this.font = font;
        vertexCount = vertices.count;
        triangleCount = triangles.count;

        OnBeforeMesh?.Invoke();

        var glyphLookup = font.GlyphLookupTable;

        buf.glyphDataCache.EnsureCapacity(buf.shapedGlyphs.count);
        var glyphCache = buf.glyphDataCache.data;
        var useCache = buf.hasValidGlyphCache;

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
                continue;

            var cluster = glyph.cluster;

            var bearingXScaled = (cachedData.bearingX - padding) * scaleVal;
            var bearingYScaled = (cachedData.bearingY + padding) * scaleVal;
            var heightScaled = (cachedData.height + padding2) * scaleVal;
            var widthScaled = (cachedData.width + padding2) * scaleVal;

            var tlX = offX + glyph.x + bearingXScaled;
            var tlY = offY - glyph.y + bearingYScaled;
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

            var verts = Vertices;
            var uvData = Uvs0;
            var cols = Colors;
            var tris = Triangles;

            ref var v0 = ref verts[i0];
            v0.x = tlX; v0.y = blY; v0.z = 0;
            ref var v1 = ref verts[i1];
            v1.x = tlX; v1.y = tlY; v1.z = 0;
            ref var v2 = ref verts[i2];
            v2.x = trX; v2.y = tlY; v2.z = 0;
            ref var v3 = ref verts[i3];
            v3.x = trX; v3.y = blY; v3.z = 0;

            ref var uv0 = ref uvData[i0];
            uv0.x = uvBLx; uv0.y = uvBLy; uv0.z = 0; uv0.w = xScaleVal;
            ref var uv1 = ref uvData[i1];
            uv1.x = uvBLx; uv1.y = uvTLy; uv1.z = 0; uv1.w = xScaleVal;
            ref var uv2 = ref uvData[i2];
            uv2.x = uvTRx; uv2.y = uvTLy; uv2.z = 0; uv2.w = xScaleVal;
            ref var uv3 = ref uvData[i3];
            uv3.x = uvTRx; uv3.y = uvBLy; uv3.z = 0; uv3.w = xScaleVal;

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
            x = glyph.x;
            y = glyph.y;
            width = widthScaled;
            height = heightScaled;
            baselineY = offY - glyph.y;

            vertexCount += 4;
            triangleCount += 6;

            OnGlyph?.Invoke();
        }

        OnAfterGlyphsPerFont?.Invoke();

        vertices.count = vertexCount;
        uvs0.count = vertexCount;
        colors.count = vertexCount;
        triangles.count = triangleCount;
        if (uvs1.data != null) uvs1.count = vertexCount;
    }
    
    public PooledList<UniTextRenderData> ApplyMeshesToUnity(Func<Mesh> meshProvider)
    {
        var resultBuffer = SharedPipelineComponents.MeshResultBuffer;
        resultBuffer.Clear();

        if (!hasGeneratedData || generatedSegments == null || generatedSegments.Count == 0)
            return resultBuffer;

        for (var i = 0; i < generatedSegments.Count; i++)
        {
            ref var segment = ref generatedSegments.buffer[i];

            var mesh = meshProvider();
            mesh.Clear();

            if (segment.vertexCount > 0)
            {
                mesh.SetVertices(vertices.data, segment.vertexStart, segment.vertexCount);
                mesh.SetUVs(0, uvs0.data, segment.vertexStart, segment.vertexCount);
                if (uvs1.data != null)
                    mesh.SetUVs(1, uvs1.data, segment.vertexStart, segment.vertexCount);
                mesh.SetColors(colors.data, segment.vertexStart, segment.vertexCount);

                var triStart = segment.triangleStart;
                var triCount = segment.triangleCount;
                var vertOffset = segment.vertexStart;

                var adjustedTris = UniTextArrayPool<int>.Rent(triCount);
                for (var t = 0; t < triCount; t++)
                    adjustedTris[t] = triangles[triStart + t] - vertOffset;

                mesh.SetTriangles(adjustedTris, 0, triCount, 0);
                UniTextArrayPool<int>.Return(adjustedTris);
            }

            resultBuffer.Add(new UniTextRenderData(mesh, segment.material, segment.texture, segment.fontId));
        }

        return resultBuffer;
    }

    #endregion
}
