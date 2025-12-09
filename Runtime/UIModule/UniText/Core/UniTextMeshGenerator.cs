using System;
using System.Collections.Generic;
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
/// Zero-allocation design using shared static buffers.
///
/// Provides events and public state for modifiers to hook into mesh generation.
/// Modifiers can modify glyph vertices or add additional geometry.
/// </summary>
public class UniTextMeshGenerator
{
    private readonly UniTextFontProvider fontProvider;

    // Settings
    public float FontSize { get; set; } = 36f;
    public Color32 DefaultColor { get; set; } = new Color32(255, 255, 255, 255);

    // DEBUG: Enable detailed logging
    public static bool DebugLogging = false;

    // Canvas parameters for xScale calculation
    private Canvas canvas;
    private float lossyScale = 1f;

    // Rect offset for positioning
    private Rect rectOffset;

    // ═══════════════════════════════════════════════════════════════════
    // EVENTS (instance) - точки расширения для модификаторов
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Перед началом генерации mesh для шрифта. Устанавливает контекст.</summary>
    public Action OnBeforeMesh;

    /// <summary>После генерации каждого глифа. Модификаторы могут модифицировать последние 4 вершины.</summary>
    public Action OnGlyph;

    /// <summary>После всех глифов, перед применением к mesh. Модификаторы могут добавить геометрию.</summary>
    public Action OnAfterGlyphs;

    // ═══════════════════════════════════════════════════════════════════
    // PUBLIC STATE - доступно модификаторам для чтения/записи
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>Cluster текущего глифа (codepoint index)</summary>
    public static int CurrentCluster;

    /// <summary>X позиция текущего глифа</summary>
    public static float CurrentX;

    /// <summary>Y позиция текущего глифа</summary>
    public static float CurrentY;

    /// <summary>Ширина текущего глифа (scaled)</summary>
    public static float CurrentWidth;

    /// <summary>Высота текущего глифа (scaled)</summary>
    public static float CurrentHeight;

    /// <summary>Baseline Y позиция текущего глифа</summary>
    public static float CurrentBaselineY;

    /// <summary>Scale для текущего шрифта (fontSize / pointSize)</summary>
    public static float Scale;

    /// <summary>XScale для SDF рендеринга (для Bold используется отрицательное значение)</summary>
    public static float XScale;

    /// <summary>Цвет по умолчанию</summary>
    public static Color32 CurrentDefaultColor;

    /// <summary>Текущий FontAsset</summary>
    public static UniTextFontAsset CurrentFontAsset;

    /// <summary>Offset по X (rectOffset.xMin)</summary>
    public static float OffsetX;

    /// <summary>Offset по Y (rectOffset.yMax)</summary>
    public static float OffsetY;

    // Буферы для записи геометрии (модификаторы могут добавлять вершины)
    public static Vector3[] Vertices => SharedPipelineComponents.MeshVertices;
    public static Vector4[] Uvs0 => SharedPipelineComponents.MeshUvs0;
    public static Vector2[] Uvs2 => SharedPipelineComponents.MeshUvs2;
    public static Color32[] Colors => SharedPipelineComponents.MeshColors32;
    public static int[] Triangles => SharedPipelineComponents.MeshTriangles;

    /// <summary>Текущее количество вершин в буфере</summary>
    public static int VertexCount;

    /// <summary>Текущее количество индексов треугольников в буфере</summary>
    public static int TriangleCount;

    // Private buffer references for faster access
    private static Vector3[] vertices => SharedPipelineComponents.MeshVertices;
    private static Vector4[] uvs0 => SharedPipelineComponents.MeshUvs0;
    private static Vector2[] uvs2 => SharedPipelineComponents.MeshUvs2;
    private static Color32[] colors32 => SharedPipelineComponents.MeshColors32;
    private static Vector3[] normals => SharedPipelineComponents.MeshNormals;
    private static Vector4[] tangents => SharedPipelineComponents.MeshTangents;
    private static int[] triangles => SharedPipelineComponents.MeshTriangles;

    public UniTextMeshGenerator(UniTextFontProvider fontProvider)
    {
        this.fontProvider = fontProvider ?? throw new ArgumentNullException(nameof(fontProvider));
    }

    public void SetCanvasParameters(Transform transform, Canvas canvas)
    {
        this.canvas = canvas;
        lossyScale = transform?.lossyScale.x ?? 1f;
    }

    public void SetRectOffset(Rect rect) => rectOffset = rect;

    /// <summary>
    /// Generates meshes from positioned glyphs.
    /// Zero-allocation using shared static buffers.
    /// </summary>
    public List<UniTextMeshPair> GenerateMeshes(ReadOnlySpan<PositionedGlyph> glyphs, Func<Mesh> meshProvider)
    {
        // Use shared static collections
        var glyphsByFont = SharedPipelineComponents.GlyphsByFont;
        var resultBuffer = SharedPipelineComponents.MeshResultBuffer;

        // Return pooled lists to pool before clearing dictionary
        SharedPipelineComponents.ClearGlyphsByFont();
        resultBuffer.Clear();

        if (DebugLogging)
            Debug.Log($"[UniTextMeshGenerator.GenerateMeshes] Input: {glyphs.Length} positioned glyphs");

        if (glyphs.Length == 0)
            return resultBuffer;

        // Group glyphs by fontId (each font needs its own mesh/material)
        int glyphLen = glyphs.Length;
        for (int i = 0; i < glyphLen; i++)
        {
            ref readonly var glyph = ref glyphs[i];
            int fontId = glyph.fontId;
            if (!glyphsByFont.TryGetValue(fontId, out var list))
            {
                // Get from shared pool
                list = SharedPipelineComponents.AcquireGlyphList();
                glyphsByFont[fontId] = list;
            }
            list.Add(glyph);
        }

        if (DebugLogging)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[UniTextMeshGenerator.GenerateMeshes] Grouped into {glyphsByFont.Count} fonts: ");
            foreach (var kvp in glyphsByFont)
                sb.Append($"fontId={kvp.Key}({kvp.Value.Count} glyphs), ");
            Debug.Log(sb.ToString());
        }

        // Generate mesh for each font
        foreach (var kvp in glyphsByFont)
        {
            int fontId = kvp.Key;
            var fontGlyphs = kvp.Value;

            var fontAsset = fontProvider.GetFontAsset(fontId);
            if (fontAsset == null)
            {
                if (DebugLogging)
                    Debug.LogWarning($"[UniTextMeshGenerator.GenerateMeshes] No fontAsset for fontId={fontId}!");
                continue;
            }

            var mesh = meshProvider?.Invoke() ?? new Mesh();

            GenerateMeshForFont(mesh, fontGlyphs, fontAsset);

            resultBuffer.Add(new UniTextMeshPair(mesh, fontAsset.Material));
        }

        if (DebugLogging)
            Debug.Log($"[UniTextMeshGenerator.GenerateMeshes] Result: {resultBuffer.Count} mesh pairs");

        return resultBuffer;
    }

    private void GenerateMeshForFont(Mesh mesh, List<PositionedGlyph> glyphs, UniTextFontAsset fontAsset)
    {
        int glyphCount = glyphs.Count;
        // Base: 4 vertices per glyph + extra for modifiers (underline, strikethrough, etc.)
        int maxVertexCount = glyphCount * 4 + glyphCount * 8;
        int maxTriangleCount = glyphCount * 6 + glyphCount * 12;

        // Ensure buffer capacity
        EnsureBufferCapacity(maxVertexCount, maxTriangleCount);

        float pointSize = fontAsset.FaceInfo.pointSize;
        float scale = pointSize > 0 ? FontSize / pointSize : 1f;
        float atlasWidth = fontAsset.AtlasWidth;
        float atlasHeight = fontAsset.AtlasHeight;
        float padding = fontAsset.AtlasPadding;
        float invAtlasWidth = 1f / atlasWidth;
        float invAtlasHeight = 1f / atlasHeight;
        float padding2 = padding * 2;

        float offsetX = rectOffset.xMin;
        float offsetY = rectOffset.yMax;

        // Calculate xScale for SDF rendering
        float xScale = CalculateXScale(scale);

        // ═══════════════════════════════════════════════════════════════════
        // Set static state for modifiers
        // ═══════════════════════════════════════════════════════════════════
        Scale = scale;
        XScale = xScale;
        OffsetX = offsetX;
        OffsetY = offsetY;
        CurrentDefaultColor = DefaultColor;
        CurrentFontAsset = fontAsset;
        VertexCount = 0;
        TriangleCount = 0;

        // Invoke OnBeforeMesh - modifiers can prepare
        OnBeforeMesh?.Invoke();

        var glyphLookup = fontAsset.GlyphLookupTable;
        Color32 defaultColor = DefaultColor;

        // DEBUG: Track glyph lookup statistics
        int foundCount = 0;
        int notFoundCount = 0;
        int whitespaceCount = 0;
        var notFoundGlyphs = DebugLogging ? new System.Collections.Generic.List<int>() : null;

        if (DebugLogging)
        {
            Debug.Log($"[UniTextMeshGenerator.GenerateMeshForFont] Processing {glyphCount} glyphs, glyphLookup has {glyphLookup?.Count ?? 0} entries");
        }

        // Cache local references for faster access
        var verts = vertices;
        var uvData = uvs0;
        var cols = colors32;
        var tris = triangles;

        for (int i = 0; i < glyphCount; i++)
        {
            var glyph = glyphs[i];
            uint glyphIndex = (uint)glyph.glyphId;

            // Lookup по glyph index
            if (!glyphLookup.TryGetValue(glyphIndex, out var glyphData) || glyphData == null)
            {
                notFoundCount++;
                if (DebugLogging && notFoundGlyphs != null && notFoundGlyphs.Count < 50)
                    notFoundGlyphs.Add((int)glyphIndex);
                continue;
            }

            var glyphRect = glyphData.glyphRect;
            var metrics = glyphData.metrics;

            // Skip whitespace (zero-size glyphs)
            if (glyphRect.width == 0 || glyphRect.height == 0)
            {
                whitespaceCount++;
                continue;
            }

            foundCount++;

            int cluster = glyph.cluster;

            // Vertex positions (base, without modifier effects)
            float bearingXScaled = (metrics.horizontalBearingX - padding) * scale;
            float bearingYScaled = (metrics.horizontalBearingY + padding) * scale;
            float heightScaled = (metrics.height + padding2) * scale;
            float widthScaled = (metrics.width + padding2) * scale;

            float tlX = offsetX + glyph.x + bearingXScaled;
            float tlY = offsetY - glyph.y + bearingYScaled;
            float blY = tlY - heightScaled;
            float trX = tlX + widthScaled;

            // UV coordinates
            float uvBLx = (glyphRect.x - padding) * invAtlasWidth;
            float uvBLy = (glyphRect.y - padding) * invAtlasHeight;
            float uvTLy = (glyphRect.y + glyphRect.height + padding) * invAtlasHeight;
            float uvTRx = (glyphRect.x + glyphRect.width + padding) * invAtlasWidth;

            int i0 = VertexCount;
            int i1 = VertexCount + 1;
            int i2 = VertexCount + 2;
            int i3 = VertexCount + 3;

            // Vertices (BL, TL, TR, BR) - base positions
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

            // Colors - default
            cols[i0] = defaultColor;
            cols[i1] = defaultColor;
            cols[i2] = defaultColor;
            cols[i3] = defaultColor;

            // Triangles
            tris[TriangleCount] = i0;
            tris[TriangleCount + 1] = i1;
            tris[TriangleCount + 2] = i2;
            tris[TriangleCount + 3] = i2;
            tris[TriangleCount + 4] = i3;
            tris[TriangleCount + 5] = i0;

            // ═══════════════════════════════════════════════════════════════════
            // Set current glyph state for modifiers
            // ═══════════════════════════════════════════════════════════════════
            CurrentCluster = cluster;
            CurrentX = glyph.x;
            CurrentY = glyph.y;
            CurrentWidth = widthScaled;
            CurrentHeight = heightScaled;
            CurrentBaselineY = offsetY - glyph.y;

            VertexCount += 4;
            TriangleCount += 6;

            // Invoke OnGlyph - modifiers can modify last 4 vertices
            OnGlyph?.Invoke();
        }

        // Invoke OnAfterGlyphs - modifiers can add geometry (underline, strikethrough)
        OnAfterGlyphs?.Invoke();

        // DEBUG: Log glyph lookup statistics
        if (DebugLogging)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append($"[UniTextMeshGenerator.GenerateMeshForFont] Stats: found={foundCount}, notFound={notFoundCount}, whitespace={whitespaceCount}\n");
            if (notFoundCount > 0 && notFoundGlyphs != null && notFoundGlyphs.Count > 0)
            {
                sb.Append("  Missing glyph IDs: ");
                for (int j = 0; j < notFoundGlyphs.Count; j++)
                {
                    sb.Append(notFoundGlyphs[j]);
                    sb.Append(", ");
                }
                if (notFoundGlyphs.Count >= 50)
                    sb.Append("... (truncated)");
                sb.Append("\n");

                sb.Append("  Available glyph IDs in lookup (first 30): ");
                int cnt = 0;
                foreach (var key in glyphLookup.Keys)
                {
                    if (cnt++ >= 30) break;
                    sb.Append(key);
                    sb.Append(", ");
                }
                if (glyphLookup.Count > 30)
                    sb.Append("...");
            }
            Debug.Log(sb.ToString());
        }

        // Apply to mesh using SetXxx with count parameter (no allocation)
        mesh.Clear();

        if (VertexCount > 0)
        {
            mesh.SetVertices(vertices, 0, VertexCount);
            mesh.SetNormals(normals, 0, VertexCount);
            mesh.SetTangents(tangents, 0, VertexCount);
            mesh.SetUVs(0, uvs0, 0, VertexCount);
            mesh.SetUVs(1, uvs2, 0, VertexCount);
            mesh.SetColors(colors32, 0, VertexCount);
            mesh.SetTriangles(triangles, 0, TriangleCount, 0);
            mesh.RecalculateBounds();

            if (DebugLogging)
                Debug.Log($"[UniTextMeshGenerator.GenerateMeshForFont] Created mesh with {VertexCount} vertices, {TriangleCount} triangle indices");
        }
        else
        {
            if (DebugLogging)
                Debug.LogWarning($"[UniTextMeshGenerator.GenerateMeshForFont] No vertices generated! All glyphs were either not found or whitespace.");
        }
    }

    private static void EnsureBufferCapacity(int vertexCount, int triangleCount)
    {
        SharedPipelineComponents.EnsureMeshCapacity(vertexCount, triangleCount);
    }

    private float CalculateXScale(float scale)
    {
        if (canvas == null) return scale;

        float absLossyScale = Mathf.Abs(lossyScale);
        return scale * (canvas.worldCamera != null ? absLossyScale : 1f);
    }
}
