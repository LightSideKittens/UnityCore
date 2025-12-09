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
/// </summary>
public class UniTextMeshGenerator
{
    private readonly UniTextFontProvider fontProvider;

    // Settings
    public float FontSize { get; set; } = 36f;
    public Color32 DefaultColor { get; set; } = new Color32(255, 255, 255, 255);

    // DEBUG: Enable detailed logging
    public static bool DebugLogging = true;

    // Canvas parameters for xScale calculation
    private Canvas canvas;
    private float lossyScale = 1f;

    // Rect offset for positioning
    private Rect rectOffset;

    // Use shared static buffers from SharedPipelineComponents
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

        // Collect ALL glyphs in ORIGINAL ORDER for underline rendering by primary font
        // TMP renders ALL underlines using primary font's underscore character
        // We need original order for correct span detection
        List<PositionedGlyph> allGlyphsInOrder = null;
        bool hasAnyUnderlines = false;
        for (int i = 0; i < glyphLen; i++)
        {
            ref readonly var g = ref glyphs[i];
            if (g.hasUnderline || g.hasStrikethrough)
            {
                hasAnyUnderlines = true;
                break;
            }
        }

        if (hasAnyUnderlines)
        {
            // Copy all glyphs in original order for underline processing
            allGlyphsInOrder = new List<PositionedGlyph>(glyphLen);
            for (int i = 0; i < glyphLen; i++)
            {
                allGlyphsInOrder.Add(glyphs[i]);
            }
        }

        // Generate mesh for each font
        // Primary font (fontId=0) renders ALL underlines, other fonts render none
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

            // Primary font (fontId=0) draws ALL underlines from ALL fonts
            bool isPrimaryFont = fontId == 0;
            var underlineGlyphs = isPrimaryFont ? allGlyphsInOrder : null;

            GenerateMeshForFont(mesh, fontGlyphs, fontAsset, underlineGlyphs);

            resultBuffer.Add(new UniTextMeshPair(mesh, fontAsset.Material));
        }

        if (DebugLogging)
            Debug.Log($"[UniTextMeshGenerator.GenerateMeshes] Result: {resultBuffer.Count} mesh pairs");

        return resultBuffer;
    }

    private void GenerateMeshForFont(Mesh mesh, List<PositionedGlyph> glyphs, UniTextFontAsset fontAsset, List<PositionedGlyph> underlineGlyphs = null)
    {
        int glyphCount = glyphs.Count;
        int underlineGlyphCount = underlineGlyphs?.Count ?? 0;
        // Reserve extra space for underline/strikethrough lines (worst case: each glyph has both)
        // TMP uses 12 vertices (3 quads) per underline/strikethrough span
        // Use underlineGlyphCount for buffer size since underlines come from separate list
        int maxVertexCount = glyphCount * 4 + underlineGlyphCount * 24; // 4 per glyph + 12 for underline + 12 for strikethrough
        int maxTriangleCount = glyphCount * 6 + underlineGlyphCount * 36;

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

        // glyphId после shaping — это glyph index из HarfBuzz или Unity
        // Используем GlyphLookupTable для получения данных глифа
        var glyphLookup = fontAsset.GlyphLookupTable;
        Color32 defaultColor = DefaultColor;
        int vertIdx = 0;
        int triIdx = 0;

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

            // Bold flag: if stylePadding > 0, glyph is bold
            // Shader uses UV0.w sign: negative = bold, positive = normal
            bool isBold = glyph.stylePadding > 0;
            float glyphXScale = isBold ? -xScale : xScale;

            // Vertex positions
            float bearingXScaled = (metrics.horizontalBearingX - padding) * scale;
            float bearingYScaled = (metrics.horizontalBearingY + padding) * scale;
            float heightScaled = (metrics.height + padding2) * scale;
            float widthScaled = (metrics.width + padding2) * scale;

            float tlX = offsetX + glyph.x + bearingXScaled;
            float tlY = offsetY - glyph.y + bearingYScaled;
            float blY = tlY - heightScaled;
            float trX = tlX + widthScaled;

            // Italic shear: shift top vertices right, bottom vertices left
            float topShearX = 0f;
            float bottomShearX = 0f;
            if (glyph.italicAngle != 0)
            {
                float italicAngle = fontAsset.ItalicStyle;
                float shearValue = italicAngle * 0.01f;

                // midPoint - центр глифа по высоте (относительно baseline)
                float midY = (tlY + blY) * 0.5f;
                topShearX = shearValue * (tlY - midY);
                bottomShearX = shearValue * (blY - midY);
            }

            // UV coordinates
            float uvBLx = (glyphRect.x - padding) * invAtlasWidth;
            float uvBLy = (glyphRect.y - padding) * invAtlasHeight;
            float uvTLy = (glyphRect.y + glyphRect.height + padding) * invAtlasHeight;
            float uvTRx = (glyphRect.x + glyphRect.width + padding) * invAtlasWidth;

            int i0 = vertIdx;
            int i1 = vertIdx + 1;
            int i2 = vertIdx + 2;
            int i3 = vertIdx + 3;

            // Vertices (BL, TL, TR, BR) - direct field assignment avoids struct construction
            // Apply italic shear to X positions
            ref var v0 = ref verts[i0];
            v0.x = tlX + bottomShearX; v0.y = blY; v0.z = 0;
            ref var v1 = ref verts[i1];
            v1.x = tlX + topShearX; v1.y = tlY; v1.z = 0;
            ref var v2 = ref verts[i2];
            v2.x = trX + topShearX; v2.y = tlY; v2.z = 0;
            ref var v3 = ref verts[i3];
            v3.x = trX + bottomShearX; v3.y = blY; v3.z = 0;

            // UV0 (xy = texture coords, w = xScale for SDF; negative w = bold)
            ref var uv0 = ref uvData[i0];
            uv0.x = uvBLx; uv0.y = uvBLy; uv0.z = 0; uv0.w = glyphXScale;
            ref var uv1 = ref uvData[i1];
            uv1.x = uvBLx; uv1.y = uvTLy; uv1.z = 0; uv1.w = glyphXScale;
            ref var uv2 = ref uvData[i2];
            uv2.x = uvTRx; uv2.y = uvTLy; uv2.z = 0; uv2.w = glyphXScale;
            ref var uv3 = ref uvData[i3];
            uv3.x = uvTRx; uv3.y = uvBLy; uv3.z = 0; uv3.w = glyphXScale;

            // Colors
            Color32 color = glyph.color.a > 0 ? glyph.color : defaultColor;
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
            triIdx += 6;

            vertIdx += 4;
        }

        // Draw underline and strikethrough lines
        // Only draw if underlineGlyphs is provided (primary font draws ALL underlines from ALL fonts)
        // Non-primary fonts pass null and skip underline rendering
        if (underlineGlyphs != null && underlineGlyphs.Count > 0)
        {
            DrawTextLines(underlineGlyphs, fontAsset, scale, xScale, padding, offsetX, offsetY, defaultColor,
                ref vertIdx, ref triIdx, verts, uvData, uvs2, cols, tris);
        }

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

                // Also log what's in the glyphLookup for debugging
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

            if (DebugLogging)
                Debug.Log($"[UniTextMeshGenerator.GenerateMeshForFont] Created mesh with {vertIdx} vertices, {triIdx} triangle indices");
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

    /// <summary>
    /// Draws underline and strikethrough lines for glyphs.
    /// Uses TMP's 3-quad approach: left edge + middle stretch + right edge.
    /// Requires underscore character '_' (0x5F) in font for proper SDF edges.
    /// </summary>
    private void DrawTextLines(
        List<PositionedGlyph> glyphs,
        UniTextFontAsset fontAsset,
        float scale,
        float xScale,
        float padding,
        float offsetX,
        float offsetY,
        Color32 defaultColor,
        ref int vertIdx,
        ref int triIdx,
        Vector3[] verts,
        Vector4[] uvData,
        Vector2[] uv2Data,
        Color32[] cols,
        int[] tris)
    {
        // Count glyphs with underline/strikethrough
        int underlineCount = 0, strikethroughCount = 0;
        for (int i = 0; i < glyphs.Count; i++)
        {
            if (glyphs[i].hasUnderline) underlineCount++;
            if (glyphs[i].hasStrikethrough) strikethroughCount++;
        }

        if (underlineCount == 0 && strikethroughCount == 0)
            return;

        if (DebugLogging)
            Debug.Log($"[DrawTextLines] Found {underlineCount} underline, {strikethroughCount} strikethrough glyphs");

        // Get underscore character for underline/strikethrough (TMP uses 0x5F '_')
        UnityEngine.TextCore.Glyph underscoreGlyph = null;
        var charTable = fontAsset.characterTable;
        if (charTable != null)
        {
            foreach (var charInfo in charTable)
            {
                if (charInfo.unicode == '_' && charInfo.glyph != null)
                {
                    underscoreGlyph = charInfo.glyph;
                    break;
                }
            }
        }

        // Try to add underscore dynamically if not found
        if (underscoreGlyph == null)
        {
            if (fontAsset.TryAddCharacter('_', out var addedChar) && addedChar?.glyph != null)
            {
                underscoreGlyph = addedChar.glyph;
                if (DebugLogging)
                    Debug.Log("[DrawTextLines] Added underscore character dynamically");
            }
        }

        if (underscoreGlyph == null || underscoreGlyph.glyphRect.width == 0)
        {
            if (DebugLogging)
                Debug.LogWarning("[DrawTextLines] Underscore character '_' not found - cannot draw underline/strikethrough!");
            return;
        }

        var underscoreRect = underscoreGlyph.glyphRect;
        var underscoreMetrics = underscoreGlyph.metrics;

        float atlasWidth = fontAsset.AtlasWidth;
        float atlasHeight = fontAsset.AtlasHeight;

        // FaceInfo metrics
        var faceInfo = fontAsset.FaceInfo;
        float underlineOffset = faceInfo.underlineOffset * scale;
        float underlineThickness = faceInfo.underlineThickness;

        // Strikethrough should be at middle of x-height (around meanLine * 0.5 or xHeight * 0.5)
        // If strikethroughOffset is 0, use better fallback based on available metrics
        float strikethroughOffset;
        if (faceInfo.strikethroughOffset != 0)
        {
            strikethroughOffset = faceInfo.strikethroughOffset * scale;
        }
        else
        {
            // Fallback: use middle of x-height (meanLine is top of lowercase letters)
            // If meanLine is 0, use ascender * 0.35 as approximation
            float xHeightMid = faceInfo.meanLine > 0
                ? faceInfo.meanLine * 0.5f
                : faceInfo.ascentLine * 0.35f;
            strikethroughOffset = xHeightMid * scale;
        }

        if (DebugLogging)
        {
            Debug.Log($"[DrawTextLines] Underscore glyph: rect=({underscoreRect.x},{underscoreRect.y},{underscoreRect.width},{underscoreRect.height}), metrics.width={underscoreMetrics.width}");
            Debug.Log($"[DrawTextLines] FaceInfo: underlineOffset={faceInfo.underlineOffset}, underlineThickness={underlineThickness}, strikethroughOffset(raw)={faceInfo.strikethroughOffset}, meanLine={faceInfo.meanLine}, ascentLine={faceInfo.ascentLine}");
            Debug.Log($"[DrawTextLines] Calculated: underlineOffset={underlineOffset}, strikethroughOffset={strikethroughOffset}, scale={scale}");
        }

        int glyphCount = glyphs.Count;

        // Line break threshold - if Y changes by more than this, it's a new line
        float lineBreakThreshold = faceInfo.lineHeight * scale * 0.5f;
        if (lineBreakThreshold < 1f) lineBreakThreshold = 10f; // fallback

        // Process underline spans - break on line changes
        int spanStart = -1;
        float spanY = 0;
        for (int i = 0; i <= glyphCount; i++)
        {
            bool hasUnderline = i < glyphCount && glyphs[i].hasUnderline;
            float currentY = i < glyphCount ? glyphs[i].y : 0;

            // Check if we need to break span due to line change
            bool lineChanged = spanStart >= 0 && i < glyphCount && Mathf.Abs(currentY - spanY) > lineBreakThreshold;

            if (lineChanged && spanStart >= 0)
            {
                // End current span before line break
                DrawUnderlineMesh(glyphs, spanStart, i - 1,
                    underscoreGlyph, underscoreRect, underscoreMetrics,
                    atlasWidth, atlasHeight, padding,
                    scale, xScale, underlineOffset, underlineThickness,
                    offsetX, offsetY, defaultColor,
                    ref vertIdx, ref triIdx, verts, uvData, uv2Data, cols, tris);
                // Start new span on current glyph if it has underline
                if (hasUnderline)
                {
                    spanStart = i;
                    spanY = currentY;
                }
                else
                {
                    spanStart = -1;
                }
            }
            else if (hasUnderline && spanStart < 0)
            {
                spanStart = i;
                spanY = currentY;
            }
            else if (!hasUnderline && spanStart >= 0)
            {
                DrawUnderlineMesh(glyphs, spanStart, i - 1,
                    underscoreGlyph, underscoreRect, underscoreMetrics,
                    atlasWidth, atlasHeight, padding,
                    scale, xScale, underlineOffset, underlineThickness,
                    offsetX, offsetY, defaultColor,
                    ref vertIdx, ref triIdx, verts, uvData, uv2Data, cols, tris);
                spanStart = -1;
            }
        }

        // Process strikethrough spans - break on line changes
        spanStart = -1;
        spanY = 0;
        for (int i = 0; i <= glyphCount; i++)
        {
            bool hasStrikethrough = i < glyphCount && glyphs[i].hasStrikethrough;
            float currentY = i < glyphCount ? glyphs[i].y : 0;

            // Check if we need to break span due to line change
            bool lineChanged = spanStart >= 0 && i < glyphCount && Mathf.Abs(currentY - spanY) > lineBreakThreshold;

            if (lineChanged && spanStart >= 0)
            {
                // End current span before line break
                DrawUnderlineMesh(glyphs, spanStart, i - 1,
                    underscoreGlyph, underscoreRect, underscoreMetrics,
                    atlasWidth, atlasHeight, padding,
                    scale, xScale, strikethroughOffset, underlineThickness,
                    offsetX, offsetY, defaultColor,
                    ref vertIdx, ref triIdx, verts, uvData, uv2Data, cols, tris);
                // Start new span on current glyph if it has strikethrough
                if (hasStrikethrough)
                {
                    spanStart = i;
                    spanY = currentY;
                }
                else
                {
                    spanStart = -1;
                }
            }
            else if (hasStrikethrough && spanStart < 0)
            {
                spanStart = i;
                spanY = currentY;
            }
            else if (!hasStrikethrough && spanStart >= 0)
            {
                DrawUnderlineMesh(glyphs, spanStart, i - 1,
                    underscoreGlyph, underscoreRect, underscoreMetrics,
                    atlasWidth, atlasHeight, padding,
                    scale, xScale, strikethroughOffset, underlineThickness,
                    offsetX, offsetY, defaultColor,
                    ref vertIdx, ref triIdx, verts, uvData, uv2Data, cols, tris);
                spanStart = -1;
            }
        }
    }

    /// <summary>
    /// Draws underline/strikethrough mesh using TMP's 3-quad approach.
    /// 3 quads (12 vertices): left edge + middle stretch + right edge.
    /// </summary>
    private static void DrawUnderlineMesh(
        List<PositionedGlyph> glyphs,
        int startIdx,
        int endIdx,
        UnityEngine.TextCore.Glyph underscoreGlyph,
        UnityEngine.TextCore.GlyphRect underscoreRect,
        UnityEngine.TextCore.GlyphMetrics underscoreMetrics,
        float atlasWidth,
        float atlasHeight,
        float padding,
        float scale,
        float xScale,
        float lineYOffset,
        float lineThickness,
        float offsetX,
        float offsetY,
        Color32 defaultColor,
        ref int vertIdx,
        ref int triIdx,
        Vector3[] verts,
        Vector4[] uvData,
        Vector2[] uv2Data,
        Color32[] cols,
        int[] tris)
    {
        if (startIdx > endIdx) return;

        var startGlyph = glyphs[startIdx];
        var endGlyph = glyphs[endIdx];

        // Calculate Y position first - use maximum glyph.y (lowest baseline) for consistent line
        float maxGlyphY = startGlyph.y;
        for (int i = startIdx + 1; i <= endIdx; i++)
        {
            if (glyphs[i].y > maxGlyphY) maxGlyphY = glyphs[i].y;
        }

        // Calculate start and end X positions
        float startX = offsetX + startGlyph.x;
        float endX;

        // Check if next glyph exists and is on the SAME line
        bool useNextGlyph = false;
        if (endIdx + 1 < glyphs.Count)
        {
            float nextGlyphY = glyphs[endIdx + 1].y;
            // Only use next glyph's X if it's on the same line (similar Y)
            float yDiff = Mathf.Abs(nextGlyphY - endGlyph.y);
            useNextGlyph = yDiff < 5f; // Small threshold for same-line detection
        }

        if (useNextGlyph)
        {
            endX = offsetX + glyphs[endIdx + 1].x;
        }
        else
        {
            // Next glyph is on different line or doesn't exist - estimate end position
            float avgGlyphWidth = endIdx > startIdx
                ? (endGlyph.x - startGlyph.x) / (endIdx - startIdx)
                : 15f;
            endX = offsetX + endGlyph.x + avgGlyphWidth;
        }

        // Baseline Y in world space
        float baselineY = offsetY - maxGlyphY;

        // Create start and end vectors at the baseline + offset
        Vector3 start = new Vector3(startX, baselineY + lineYOffset, 0);
        Vector3 end = new Vector3(endX, baselineY + lineYOffset, 0);

        // Ensure both have same Y (TMP does this)
        start.y = Mathf.Min(start.y, end.y);
        end.y = start.y;

        // Segment width = half of underscore glyph width * scale
        float segmentWidth = underscoreMetrics.width * 0.5f * scale;

        // If line is shorter than full underscore width, adjust segment
        if (end.x - start.x < underscoreMetrics.width * scale)
        {
            segmentWidth = (end.x - start.x) * 0.5f;
        }

        // Thickness from font metrics
        float thickness = (lineThickness + padding) * scale;
        float paddingScaled = padding * scale;

        Color32 color = startGlyph.color.a > 0 ? startGlyph.color : defaultColor;

        if (UniTextMeshGenerator.DebugLogging)
        {
            Debug.Log($"[DrawUnderlineMesh] glyphs[{startIdx}..{endIdx}]: start=({start.x},{start.y}), end=({end.x},{end.y})");
            Debug.Log($"[DrawUnderlineMesh] segmentWidth={segmentWidth}, thickness={thickness}, padding={paddingScaled}");
        }

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

        // TMP formula: startPadding = m_padding * startScale / maxScale
        // Since we use uniform scale, just use padding directly
        float startPadding = padding;
        float endPadding = padding;

        // UV coordinates EXACTLY as TMP does:
        // uv0 = left edge with padding (BL of left quad)
        // uv1 = same X, top Y with padding (TL of left quad)
        // uv2 = center of glyph from left (TR of left quad)
        // uv3 = same X as uv2, bottom Y (BR of left quad)
        // uv4 = center + padding (TL of right quad)
        // uv5 = same X as uv4, bottom Y (BL of right quad)
        // uv6 = right edge with padding (TR of right quad)
        // uv7 = same X as uv6, bottom Y (BR of right quad)

        Vector4 uv0 = new Vector4(
            (underscoreRect.x - startPadding) / atlasWidth,
            (underscoreRect.y - padding) / atlasHeight,
            0, xScale);

        Vector4 uv1 = new Vector4(
            uv0.x,
            (underscoreRect.y + underscoreRect.height + padding) / atlasHeight,
            0, xScale);

        // TMP: (underlineGlyphRect.x - startPadding + underlineGlyphRect.width / 2)
        Vector4 uv2 = new Vector4(
            (underscoreRect.x - startPadding + underscoreRect.width * 0.5f) / atlasWidth,
            uv1.y,
            0, xScale);

        Vector4 uv3 = new Vector4(uv2.x, uv0.y, 0, xScale);

        // TMP: (underlineGlyphRect.x + endPadding + underlineGlyphRect.width / 2)
        Vector4 uv4 = new Vector4(
            (underscoreRect.x + endPadding + underscoreRect.width * 0.5f) / atlasWidth,
            uv1.y,
            0, xScale);

        Vector4 uv5 = new Vector4(uv4.x, uv0.y, 0, xScale);

        // TMP: (underlineGlyphRect.x + endPadding + underlineGlyphRect.width)
        Vector4 uv6 = new Vector4(
            (underscoreRect.x + endPadding + underscoreRect.width) / atlasWidth,
            uv1.y,
            0, xScale);

        Vector4 uv7 = new Vector4(uv6.x, uv0.y, 0, xScale);

        // Left quad UVs - left half of underscore
        uvData[vertIdx + 0] = uv0;
        uvData[vertIdx + 1] = uv1;
        uvData[vertIdx + 2] = uv2;
        uvData[vertIdx + 3] = uv3;

        // Middle quad UVs - sample center point (tiny UV range for stretch)
        uvData[vertIdx + 4] = new Vector4(uv2.x - uv2.x * 0.001f, uv0.y, 0, xScale);
        uvData[vertIdx + 5] = new Vector4(uv2.x - uv2.x * 0.001f, uv1.y, 0, xScale);
        uvData[vertIdx + 6] = new Vector4(uv2.x + uv2.x * 0.001f, uv1.y, 0, xScale);
        uvData[vertIdx + 7] = new Vector4(uv2.x + uv2.x * 0.001f, uv0.y, 0, xScale);

        // Right quad UVs - right half of underscore
        uvData[vertIdx + 8] = uv5;
        uvData[vertIdx + 9] = uv4;
        uvData[vertIdx + 10] = uv6;
        uvData[vertIdx + 11] = uv7;

        #endregion

        #region UV2 (SDF scale - normalized X position along line)

        float totalWidth = end.x - start.x;
        if (totalWidth < 0.001f) totalWidth = 1f;

        float maxUvX_Left = (verts[vertIdx + 2].x - start.x) / totalWidth;
        float minUvX_Mid = (verts[vertIdx + 4].x - start.x) / totalWidth;
        float maxUvX_Mid = (verts[vertIdx + 6].x - start.x) / totalWidth;
        float minUvX_Right = (verts[vertIdx + 8].x - start.x) / totalWidth;

        // Left quad UV2
        uv2Data[vertIdx + 0] = new Vector2(0, 0);
        uv2Data[vertIdx + 1] = new Vector2(0, 1);
        uv2Data[vertIdx + 2] = new Vector2(maxUvX_Left, 1);
        uv2Data[vertIdx + 3] = new Vector2(maxUvX_Left, 0);

        // Middle quad UV2
        uv2Data[vertIdx + 4] = new Vector2(minUvX_Mid, 0);
        uv2Data[vertIdx + 5] = new Vector2(minUvX_Mid, 1);
        uv2Data[vertIdx + 6] = new Vector2(maxUvX_Mid, 1);
        uv2Data[vertIdx + 7] = new Vector2(maxUvX_Mid, 0);

        // Right quad UV2
        uv2Data[vertIdx + 8] = new Vector2(minUvX_Right, 0);
        uv2Data[vertIdx + 9] = new Vector2(minUvX_Right, 1);
        uv2Data[vertIdx + 10] = new Vector2(1, 1);
        uv2Data[vertIdx + 11] = new Vector2(1, 0);

        #endregion

        #region COLORS

        for (int i = 0; i < 12; i++)
        {
            cols[vertIdx + i] = color;
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

        vertIdx += 12;
        triIdx += 18;
    }
}
