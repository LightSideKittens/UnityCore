using System;
using System.Collections.Generic;
using UnityEngine;


public static class SharedPipelineComponents
{
    #region Pipeline Components (lazy initialized)

    private static BidiEngine bidiEngine;
    private static ScriptAnalyzer scriptAnalyzer;
    private static LineBreakAlgorithm lineBreakAlgorithm;
    private static LineBreaker lineBreaker;
    private static TextLayout layout;
    private static HybridShapingEngine shapingEngine;
    private static HarfBuzzShapingEngine harfBuzzEngine;

    public static BidiEngine BidiEngine => bidiEngine ??= new BidiEngine();
    public static ScriptAnalyzer ScriptAnalyzer => scriptAnalyzer ??= new ScriptAnalyzer();
    public static LineBreakAlgorithm LineBreakAlgorithm => lineBreakAlgorithm ??= new LineBreakAlgorithm();
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

    private const int InitialMeshCapacity = 256 * 4;

    private static Vector3[] meshVertices = new Vector3[InitialMeshCapacity];
    private static Vector4[] meshUvs0 = new Vector4[InitialMeshCapacity];
    private static Vector2[] meshUvs2 = new Vector2[InitialMeshCapacity];
    private static Color32[] meshColors32 = new Color32[InitialMeshCapacity];
    private static Vector3[] meshNormals = new Vector3[InitialMeshCapacity];
    private static Vector4[] meshTangents = new Vector4[InitialMeshCapacity];
    private static int[] meshTriangles = new int[InitialMeshCapacity * 3 / 2];

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
            var newSize = Mathf.NextPowerOfTwo(vertexCount);
            meshVertices = new Vector3[newSize];
            meshUvs0 = new Vector4[newSize];
            meshUvs2 = new Vector2[newSize];
            meshColors32 = new Color32[newSize];
            meshNormals = new Vector3[newSize];
            meshTangents = new Vector4[newSize];

            Array.Fill(meshNormals, defaultNormal);
            Array.Fill(meshTangents, defaultTangent);

            for (var i = 0; i < newSize; i++)
                meshUvs2[i] = quadUV2[i & 3];
        }

        if (meshTriangles.Length < triangleCount) meshTriangles = new int[Mathf.NextPowerOfTwo(triangleCount)];
    }

    #endregion

    #region Glyph Grouping (for mesh generator)

    private static FastIntDictionary<PooledList<int>> glyphsByFont;
    private static Stack<PooledList<int>> glyphListPool;
    private static PooledList<UniTextRenderData> meshResultBuffer;

    public static FastIntDictionary<PooledList<int>> GlyphsByFont
        => glyphsByFont ??= new FastIntDictionary<PooledList<int>>();

    public static Stack<PooledList<int>> GlyphListPool
        => glyphListPool ??= new Stack<PooledList<int>>();

    public static PooledList<UniTextRenderData> MeshResultBuffer
        => meshResultBuffer ??= new PooledList<UniTextRenderData>(4);

    public static PooledList<int> AcquireGlyphIndexList()
    {
        var pool = GlyphListPool;
        return pool.Count > 0 ? pool.Pop() : new PooledList<int>(64);
    }

    public static void ReleaseGlyphIndexList(PooledList<int> list)
    {
        list.FakeClear();
        GlyphListPool.Push(list);
    }

    public static void ClearGlyphsByFont()
    {
        var dict = GlyphsByFont;
        var pool = GlyphListPool;
        foreach (var kvp in dict)
        {
            kvp.Value.FakeClear();
            pool.Push(kvp.Value);
        }

        dict.ClearFast();
    }

    #endregion

    #region Domain Reload

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        harfBuzzEngine?.Dispose();

        bidiEngine = null;
        scriptAnalyzer = null;
        lineBreakAlgorithm = null;
        lineBreaker = null;
        layout = null;
        shapingEngine = null;
        harfBuzzEngine = null;

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