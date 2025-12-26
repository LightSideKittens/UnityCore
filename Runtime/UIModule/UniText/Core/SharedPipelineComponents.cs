using System;
using System.Collections.Generic;
using UnityEngine;


public static class SharedPipelineComponents
{
    #region Pipeline Components (lazy initialized, thread-safe or stateless)

    private static BidiEngine bidiEngine;
    private static ScriptAnalyzer scriptAnalyzer;
    private static LineBreakAlgorithm lineBreakAlgorithm;
    [ThreadStatic] private static LineBreaker lineBreaker;
    [ThreadStatic] private static TextLayout textLayout;
    private static HybridShapingEngine shapingEngine;
    private static HarfBuzzShapingEngine harfBuzzEngine;
    private static readonly object shapingEngineLock = new();

    public static BidiEngine BidiEngine => bidiEngine ??= new BidiEngine();
    public static ScriptAnalyzer ScriptAnalyzer => scriptAnalyzer ??= new ScriptAnalyzer();
    public static LineBreakAlgorithm LineBreakAlgorithm => lineBreakAlgorithm ??= new LineBreakAlgorithm();

    public static LineBreaker LineBreaker => lineBreaker ??= new LineBreaker();
    public static TextLayout Layout => textLayout ??= new TextLayout();

    public static HybridShapingEngine ShapingEngine
    {
        get
        {
            if (shapingEngine == null)
            {
                lock (shapingEngineLock)
                {
                    if (shapingEngine == null)
                    {
                        harfBuzzEngine = new HarfBuzzShapingEngine();
                        shapingEngine = new HybridShapingEngine(harfBuzzEngine);
                    }
                }
            }

            return shapingEngine;
        }
    }

    public static HarfBuzzShapingEngine HarfBuzzEngine => harfBuzzEngine;

    #endregion

    #region Shaping Output Buffer (ThreadStatic)

    [ThreadStatic] private static ShapedGlyph[] threadShapingOutputBuffer;

    public static ShapedGlyph[] ShapingOutputBuffer
    {
        get
        {
            threadShapingOutputBuffer ??= new ShapedGlyph[256];
            return threadShapingOutputBuffer;
        }
    }

    public static void EnsureShapingOutputCapacity(int required)
    {
        if (threadShapingOutputBuffer == null)
        {
            threadShapingOutputBuffer = new ShapedGlyph[Math.Max(required, 256)];
            return;
        }
        if (threadShapingOutputBuffer.Length < required)
            threadShapingOutputBuffer = new ShapedGlyph[Math.Max(required, threadShapingOutputBuffer.Length * 2)];
    }

    #endregion

    #region Mesh Generator Buffers (ThreadStatic)

    private const int InitialMeshCapacity = 256 * 4;

    [ThreadStatic] private static Vector3[] threadMeshVertices;
    [ThreadStatic] private static Vector4[] threadMeshUvs0;
    [ThreadStatic] private static Vector2[] threadMeshUvs2;
    [ThreadStatic] private static Color32[] threadMeshColors32;
    [ThreadStatic] private static Vector3[] threadMeshNormals;
    [ThreadStatic] private static Vector4[] threadMeshTangents;
    [ThreadStatic] private static int[] threadMeshTriangles;

    private static readonly Vector3 defaultNormal = new(0f, 0f, -1f);
    private static readonly Vector4 defaultTangent = new(-1f, 0f, 0f, 1f);
    private static readonly Vector2[] quadUV2 = { new(0, 0), new(0, 1), new(1, 1), new(1, 0) };

    public static Vector3[] MeshVertices
    {
        get
        {
            threadMeshVertices ??= new Vector3[InitialMeshCapacity];
            return threadMeshVertices;
        }
    }

    public static Vector4[] MeshUvs0
    {
        get
        {
            threadMeshUvs0 ??= new Vector4[InitialMeshCapacity];
            return threadMeshUvs0;
        }
    }

    public static Vector2[] MeshUvs2
    {
        get
        {
            if (threadMeshUvs2 == null)
            {
                threadMeshUvs2 = new Vector2[InitialMeshCapacity];
                for (var i = 0; i < InitialMeshCapacity; i++)
                    threadMeshUvs2[i] = quadUV2[i & 3];
            }
            return threadMeshUvs2;
        }
    }

    public static Color32[] MeshColors32
    {
        get
        {
            threadMeshColors32 ??= new Color32[InitialMeshCapacity];
            return threadMeshColors32;
        }
    }

    public static Vector3[] MeshNormals
    {
        get
        {
            if (threadMeshNormals == null)
            {
                threadMeshNormals = new Vector3[InitialMeshCapacity];
                Array.Fill(threadMeshNormals, defaultNormal);
            }
            return threadMeshNormals;
        }
    }

    public static Vector4[] MeshTangents
    {
        get
        {
            if (threadMeshTangents == null)
            {
                threadMeshTangents = new Vector4[InitialMeshCapacity];
                Array.Fill(threadMeshTangents, defaultTangent);
            }
            return threadMeshTangents;
        }
    }

    public static int[] MeshTriangles
    {
        get
        {
            threadMeshTriangles ??= new int[InitialMeshCapacity * 3 / 2];
            return threadMeshTriangles;
        }
    }

    public static void EnsureMeshCapacity(int vertexCount, int triangleCount)
    {
        var verts = MeshVertices;
        var uvs0 = MeshUvs0;
        var uvs2 = MeshUvs2;
        var colors = MeshColors32;
        var normals = MeshNormals;
        var tangents = MeshTangents;
        var tris = MeshTriangles;

        if (verts.Length < vertexCount)
        {
            var newSize = Mathf.NextPowerOfTwo(vertexCount);
            threadMeshVertices = new Vector3[newSize];
            threadMeshUvs0 = new Vector4[newSize];
            threadMeshUvs2 = new Vector2[newSize];
            threadMeshColors32 = new Color32[newSize];
            threadMeshNormals = new Vector3[newSize];
            threadMeshTangents = new Vector4[newSize];

            Array.Fill(threadMeshNormals, defaultNormal);
            Array.Fill(threadMeshTangents, defaultTangent);

            for (var i = 0; i < newSize; i++)
                threadMeshUvs2[i] = quadUV2[i & 3];
        }

        if (tris.Length < triangleCount)
            threadMeshTriangles = new int[Mathf.NextPowerOfTwo(triangleCount)];
    }

    #endregion

    #region Glyph Grouping (ThreadStatic, for mesh generator)

    [ThreadStatic] private static FastIntDictionary<PooledList<int>> threadGlyphsByFont;
    [ThreadStatic] private static Stack<PooledList<int>> threadGlyphListPool;
    [ThreadStatic] private static PooledList<UniTextRenderData> threadMeshResultBuffer;

    public static FastIntDictionary<PooledList<int>> GlyphsByFont
        => threadGlyphsByFont ??= new FastIntDictionary<PooledList<int>>();

    public static Stack<PooledList<int>> GlyphListPool
        => threadGlyphListPool ??= new Stack<PooledList<int>>();

    public static PooledList<UniTextRenderData> MeshResultBuffer
        => threadMeshResultBuffer ??= new PooledList<UniTextRenderData>(4);

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
}
