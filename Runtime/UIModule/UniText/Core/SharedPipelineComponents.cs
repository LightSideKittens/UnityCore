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
