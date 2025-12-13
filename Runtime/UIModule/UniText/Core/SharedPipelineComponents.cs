using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;

/// <summary>
/// Shared static pipeline components for UniText.
/// Since the system is single-threaded, we can safely share these between all UniText instances.
/// This eliminates per-instance allocations for pipeline components.
/// </summary>
public static class SharedPipelineComponents
{
    #region Pipeline Components (lazy initialized)

    private static BidiEngine bidiEngine;
    private static ScriptAnalyzer scriptAnalyzer;
    private static LineBreaker lineBreaker;
    private static TextLayout layout;
    private static HybridShapingEngine shapingEngine;
    private static HarfBuzzShapingEngine harfBuzzEngine;

    public static BidiEngine BidiEngine => bidiEngine ??= new BidiEngine();
    public static ScriptAnalyzer ScriptAnalyzer => scriptAnalyzer ??= new ScriptAnalyzer();
    public static LineBreaker LineBreaker => lineBreaker ??= new LineBreaker();
    public static TextLayout Layout => layout ??= new TextLayout();

    public static HybridShapingEngine ShapingEngine
    {
        get
        {
            if (shapingEngine == null)
            {
                harfBuzzEngine = new HarfBuzzShapingEngine();
                shapingEngine = new HybridShapingEngine(harfBuzzEngine);
            }
            return shapingEngine;
        }
    }

    public static HarfBuzzShapingEngine HarfBuzzEngine => harfBuzzEngine;

    #endregion

    #region Shaping Output Buffer

    private static ShapedGlyph[] shapingOutputBuffer = new ShapedGlyph[256];

    public static ShapedGlyph[] ShapingOutputBuffer => shapingOutputBuffer;

    public static void EnsureShapingOutputCapacity(int required)
    {
        if (shapingOutputBuffer.Length < required)
            shapingOutputBuffer = new ShapedGlyph[Math.Max(required, shapingOutputBuffer.Length * 2)];
    }

    #endregion

    #region Mesh Generator Buffers

    // 4 vertices per glyph, match MinGlyphCapacity (256) from CommonData
    private const int InitialMeshCapacity = 256 * 4;

    private static Vector3[] meshVertices = new Vector3[InitialMeshCapacity];
    private static Vector4[] meshUvs0 = new Vector4[InitialMeshCapacity];
    private static Vector2[] meshUvs2 = new Vector2[InitialMeshCapacity];
    private static Color32[] meshColors32 = new Color32[InitialMeshCapacity];
    private static Vector3[] meshNormals = new Vector3[InitialMeshCapacity];
    private static Vector4[] meshTangents = new Vector4[InitialMeshCapacity];
    private static int[] meshTriangles = new int[InitialMeshCapacity * 3 / 2]; // 6 indices per 4 vertices

    private static readonly Vector3 defaultNormal = new(0f, 0f, -1f);
    private static readonly Vector4 defaultTangent = new(-1f, 0f, 0f, 1f);
    private static readonly Vector2[] quadUV2 = { new(0, 0), new(0, 1), new(1, 1), new(1, 0) };

    public static Vector3[] MeshVertices => meshVertices;
    public static Vector4[] MeshUvs0 => meshUvs0;
    public static Vector2[] MeshUvs2 => meshUvs2;
    public static Color32[] MeshColors32 => meshColors32;
    public static Vector3[] MeshNormals => meshNormals;
    public static Vector4[] MeshTangents => meshTangents;
    public static int[] MeshTriangles => meshTriangles;

    public static void EnsureMeshCapacity(int vertexCount, int triangleCount)
    {
        if (meshVertices.Length < vertexCount)
        {
            int newSize = Mathf.NextPowerOfTwo(vertexCount);
            meshVertices = new Vector3[newSize];
            meshUvs0 = new Vector4[newSize];
            meshUvs2 = new Vector2[newSize];
            meshColors32 = new Color32[newSize];
            meshNormals = new Vector3[newSize];
            meshTangents = new Vector4[newSize];

            // Pre-fill static values
            Array.Fill(meshNormals, defaultNormal);
            Array.Fill(meshTangents, defaultTangent);

            // Pre-fill UV2 pattern (repeating quad corners)
            for (int i = 0; i < newSize; i++)
                meshUvs2[i] = quadUV2[i & 3];
        }
        if (meshTriangles.Length < triangleCount)
        {
            meshTriangles = new int[Mathf.NextPowerOfTwo(triangleCount)];
        }
    }

    #endregion

    #region Glyph Grouping (for mesh generator)

    private static Dictionary<int, LSList<PositionedGlyph>> glyphsByFont;
    private static Stack<LSList<PositionedGlyph>> glyphListPool;
    private static LSList<UniTextMeshPair> meshResultBuffer;

    public static Dictionary<int, LSList<PositionedGlyph>> GlyphsByFont
        => glyphsByFont ??= new Dictionary<int, LSList<PositionedGlyph>>();

    public static Stack<LSList<PositionedGlyph>> GlyphListPool
        => glyphListPool ??= new Stack<LSList<PositionedGlyph>>();

    public static LSList<UniTextMeshPair> MeshResultBuffer
        => meshResultBuffer ??= new LSList<UniTextMeshPair>(4);

    public static LSList<PositionedGlyph> AcquireGlyphList()
    {
        var pool = GlyphListPool;
        return pool.Count > 0 ? pool.Pop() : new LSList<PositionedGlyph>(64);
    }

    public static void ReleaseGlyphList(LSList<PositionedGlyph> list)
    {
        // FakeClear is safe for PositionedGlyph (pure value type)
        list.FakeClear();
        GlyphListPool.Push(list);
    }

    public static void ClearGlyphsByFont()
    {
        var dict = GlyphsByFont;
        var pool = GlyphListPool;
        foreach (var kvp in dict)
        {
            // FakeClear is safe for PositionedGlyph (pure value type)
            kvp.Value.FakeClear();
            pool.Push(kvp.Value);
        }
        dict.Clear();
    }

    #endregion

    #region Domain Reload

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        // Dispose HarfBuzz engine on domain reload
        harfBuzzEngine?.Dispose();

        // Reset all references
        bidiEngine = null;
        scriptAnalyzer = null;
        lineBreaker = null;
        layout = null;
        shapingEngine = null;
        harfBuzzEngine = null;

        // Reset buffers (will be recreated on demand)
        shapingOutputBuffer = new ShapedGlyph[256];
        meshVertices = new Vector3[InitialMeshCapacity];
        meshUvs0 = new Vector4[InitialMeshCapacity];
        meshUvs2 = new Vector2[InitialMeshCapacity];
        meshColors32 = new Color32[InitialMeshCapacity];
        meshNormals = new Vector3[InitialMeshCapacity];
        meshTangents = new Vector4[InitialMeshCapacity];
        meshTriangles = new int[InitialMeshCapacity * 3 / 2];

        glyphsByFont = null;
        glyphListPool = null;
        meshResultBuffer = null;
    }

    #endregion
}
