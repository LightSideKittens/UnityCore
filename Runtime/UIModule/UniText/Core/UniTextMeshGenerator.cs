using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Pair of mesh and material for rendering.
/// </summary>
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

/// <summary>
/// Generates meshes for text rendering.
/// Based on TMP mesh generation logic.
/// Zero-allocation design (after warmup).
/// </summary>
public class UniTextMeshGenerator
{
    private readonly UniTextFontProvider fontProvider;

    // Settings
    public float FontSize { get; set; } = 36f;
    public Color32 DefaultColor { get; set; } = new Color32(255, 255, 255, 255);

    // Canvas parameters for xScale calculation
    private Canvas canvas;
    private float lossyScale = 1f;

    // Rect offset for positioning
    private Rect rectOffset;

    // Working buffers (reused, grow as needed)
    private Vector3[] vertices = new Vector3[256];
    private Vector4[] uvs0 = new Vector4[256];
    private Vector2[] uvs2 = new Vector2[256];
    private Color32[] colors32 = new Color32[256];
    private Vector3[] normals = new Vector3[256];
    private Vector4[] tangents = new Vector4[256];
    private int[] triangles = new int[384];

    // Glyph grouping (reused, zero-allocation after warmup)
    private readonly Dictionary<int, List<PositionedGlyph>> glyphsByFont = new();
    private readonly Stack<List<PositionedGlyph>> glyphListPool = new();
    private readonly List<UniTextMeshPair> resultBuffer = new(4);

    private static readonly Vector3 s_DefaultNormal = new(0f, 0f, -1f);
    private static readonly Vector4 s_DefaultTangent = new(-1f, 0f, 0f, 1f);

    public UniTextMeshGenerator(UniTextFontProvider fontProvider)
    {
        this.fontProvider = fontProvider ?? throw new ArgumentNullException(nameof(fontProvider));
    }

    public void SetCanvasParameters(Transform transform, Canvas canvas)
    {
        this.canvas = canvas;
        this.lossyScale = transform != null ? transform.lossyScale.x : 1f;
    }

    public void SetRectOffset(Rect rect)
    {
        this.rectOffset = rect;
    }

    /// <summary>
    /// Generates meshes from positioned glyphs.
    /// Zero-allocation after warmup.
    /// </summary>
    public List<UniTextMeshPair> GenerateMeshes(ReadOnlySpan<PositionedGlyph> glyphs, Func<Mesh> meshProvider)
    {
        // Return pooled lists to pool before clearing dictionary
        foreach (var kvp in glyphsByFont)
        {
            var list = kvp.Value;
            list.Clear();
            glyphListPool.Push(list);
        }
        glyphsByFont.Clear();
        resultBuffer.Clear();

        if (glyphs.Length == 0)
            return resultBuffer;

        // Group glyphs by fontId (each font needs its own mesh/material)
        for (int i = 0; i < glyphs.Length; i++)
        {
            var glyph = glyphs[i];
            if (!glyphsByFont.TryGetValue(glyph.fontId, out var list))
            {
                // Get from pool or create new
                list = glyphListPool.Count > 0 ? glyphListPool.Pop() : new List<PositionedGlyph>(64);
                glyphsByFont[glyph.fontId] = list;
            }
            list.Add(glyph);
        }

        // Generate mesh for each font
        foreach (var kvp in glyphsByFont)
        {
            int fontId = kvp.Key;
            var fontGlyphs = kvp.Value;

            var fontAsset = fontProvider.GetFontAsset(fontId);
            if (fontAsset == null)
                continue;

            var mesh = meshProvider?.Invoke() ?? new Mesh();
            GenerateMeshForFont(mesh, fontGlyphs, fontAsset);

            resultBuffer.Add(new UniTextMeshPair(mesh, fontAsset.material));
        }

        return resultBuffer;
    }

    private void GenerateMeshForFont(Mesh mesh, List<PositionedGlyph> glyphs, TMP_FontAsset fontAsset)
    {
        int glyphCount = glyphs.Count;
        int maxVertexCount = glyphCount * 4;
        int maxTriangleCount = glyphCount * 6;

        // Ensure buffer capacity
        EnsureBufferCapacity(maxVertexCount, maxTriangleCount);

        float scale = FontSize / fontAsset.faceInfo.pointSize;
        float atlasWidth = fontAsset.atlasWidth;
        float atlasHeight = fontAsset.atlasHeight;
        float padding = fontAsset.atlasPadding;

        float offsetX = rectOffset.xMin;
        float offsetY = rectOffset.yMax;

        // Calculate xScale for SDF rendering
        float xScale = CalculateXScale(scale);

        var glyphLookup = fontAsset.glyphLookupTable;
        int vertIdx = 0;
        int triIdx = 0;

        for (int i = 0; i < glyphCount; i++)
        {
            var glyph = glyphs[i];

            if (!glyphLookup.TryGetValue((uint)glyph.glyphId, out var glyphData))
                continue;

            var glyphRect = glyphData.glyphRect;
            var metrics = glyphData.metrics;

            // Skip whitespace (zero-size glyphs)
            if (glyphRect.width == 0 || glyphRect.height == 0)
                continue;

            // Vertex positions
            float tlX = offsetX + glyph.x + (metrics.horizontalBearingX - padding) * scale;
            float tlY = offsetY - glyph.y + (metrics.horizontalBearingY + padding) * scale;
            float blY = tlY - (metrics.height + padding * 2) * scale;
            float trX = tlX + (metrics.width + padding * 2) * scale;

            // UV coordinates
            float uvBLx = (glyphRect.x - padding) / atlasWidth;
            float uvBLy = (glyphRect.y - padding) / atlasHeight;
            float uvTLy = (glyphRect.y + glyphRect.height + padding) / atlasHeight;
            float uvTRx = (glyphRect.x + glyphRect.width + padding) / atlasWidth;

            int i0 = vertIdx;
            int i1 = vertIdx + 1;
            int i2 = vertIdx + 2;
            int i3 = vertIdx + 3;

            // Vertices (BL, TL, TR, BR)
            vertices[i0] = new Vector3(tlX, blY, 0);
            vertices[i1] = new Vector3(tlX, tlY, 0);
            vertices[i2] = new Vector3(trX, tlY, 0);
            vertices[i3] = new Vector3(trX, blY, 0);

            // UV0 (xy = texture coords, w = xScale for SDF)
            uvs0[i0] = new Vector4(uvBLx, uvBLy, 0, xScale);
            uvs0[i1] = new Vector4(uvBLx, uvTLy, 0, xScale);
            uvs0[i2] = new Vector4(uvTRx, uvTLy, 0, xScale);
            uvs0[i3] = new Vector4(uvTRx, uvBLy, 0, xScale);

            // UV2
            uvs2[i0] = new Vector2(0, 0);
            uvs2[i1] = new Vector2(0, 1);
            uvs2[i2] = new Vector2(1, 1);
            uvs2[i3] = new Vector2(1, 0);

            // Colors
            Color32 color = glyph.color.a > 0 ? glyph.color : DefaultColor;
            colors32[i0] = color;
            colors32[i1] = color;
            colors32[i2] = color;
            colors32[i3] = color;

            // Normals & Tangents
            normals[i0] = s_DefaultNormal;
            normals[i1] = s_DefaultNormal;
            normals[i2] = s_DefaultNormal;
            normals[i3] = s_DefaultNormal;

            tangents[i0] = s_DefaultTangent;
            tangents[i1] = s_DefaultTangent;
            tangents[i2] = s_DefaultTangent;
            tangents[i3] = s_DefaultTangent;

            // Triangles
            triangles[triIdx++] = i0;
            triangles[triIdx++] = i1;
            triangles[triIdx++] = i2;
            triangles[triIdx++] = i2;
            triangles[triIdx++] = i3;
            triangles[triIdx++] = i0;

            vertIdx += 4;
        }

        // Apply to mesh using SetXxx with count parameter (no allocation)
        mesh.Clear();

        if (vertIdx > 0)
        {
            mesh.SetVertices(vertices, 0, vertIdx);
            mesh.SetNormals(normals, 0, vertIdx);
            mesh.SetTangents(tangents, 0, vertIdx);
            mesh.SetUVs(0, uvs0, 0, vertIdx);
            mesh.SetUVs(1, uvs2, 0, vertIdx);
            mesh.SetColors(colors32, 0, vertIdx);
            mesh.SetTriangles(triangles, 0, triIdx, 0);
            mesh.RecalculateBounds();
        }
    }

    private void EnsureBufferCapacity(int vertexCount, int triangleCount)
    {
        if (vertices.Length < vertexCount)
        {
            int newSize = Mathf.NextPowerOfTwo(vertexCount);
            vertices = new Vector3[newSize];
            uvs0 = new Vector4[newSize];
            uvs2 = new Vector2[newSize];
            colors32 = new Color32[newSize];
            normals = new Vector3[newSize];
            tangents = new Vector4[newSize];
        }
        if (triangles.Length < triangleCount)
        {
            triangles = new int[Mathf.NextPowerOfTwo(triangleCount)];
        }
    }

    private float CalculateXScale(float scale)
    {
        float xScale = scale;
        if (canvas != null)
        {
            switch (canvas.renderMode)
            {
                case RenderMode.ScreenSpaceOverlay:
                    xScale *= Mathf.Abs(lossyScale) / canvas.scaleFactor;
                    break;
                case RenderMode.ScreenSpaceCamera:
                    xScale *= canvas.worldCamera != null ? Mathf.Abs(lossyScale) : 1f;
                    break;
                case RenderMode.WorldSpace:
                    xScale *= Mathf.Abs(lossyScale);
                    break;
            }
        }
        return xScale;
    }
}
