using System;
using System.Collections.Generic;
using UnityEngine;


public static class SharedPipelineComponents
{
    #region Pipeline Components

    private static readonly Lazy<BidiEngine> bidiEngine = new(() => new BidiEngine());
    private static readonly Lazy<ScriptAnalyzer> scriptAnalyzer = new(() => new ScriptAnalyzer());
    private static readonly Lazy<LineBreakAlgorithm> lineBreakAlgorithm = new(() => new LineBreakAlgorithm());
    private static readonly Lazy<HybridShapingEngine> shapingEngine = new(() =>
    {
        var harfBuzz = new HarfBuzzShapingEngine();
        return new HybridShapingEngine(harfBuzz);
    });

    [ThreadStatic] private static LineBreaker lineBreaker;
    [ThreadStatic] private static TextLayout textLayout;

    public static BidiEngine BidiEngine => bidiEngine.Value;
    public static ScriptAnalyzer ScriptAnalyzer => scriptAnalyzer.Value;
    public static LineBreakAlgorithm LineBreakAlgorithm => lineBreakAlgorithm.Value;
    public static HybridShapingEngine ShapingEngine => shapingEngine.Value;

    public static LineBreaker LineBreaker => lineBreaker ??= new LineBreaker();
    public static TextLayout Layout => textLayout ??= new TextLayout();

    #endregion
    
    [ThreadStatic] public static PooledBuffer<ShapedGlyph> shapingOutputBuffer;

    #region Glyph Grouping (ThreadStatic, for mesh generator)

    [ThreadStatic] private static FastIntDictionary<PooledList<int>> glyphsByFont;
    [ThreadStatic] private static Stack<PooledList<int>> glyphListPool;
    [ThreadStatic] private static PooledList<UniTextRenderData> meshResultBuffer;

    public static FastIntDictionary<PooledList<int>> GlyphsByFont
        => glyphsByFont ??= new FastIntDictionary<PooledList<int>>();

    public static Stack<PooledList<int>> GlyphListPool
        => glyphListPool ??= new Stack<PooledList<int>>();

    public static PooledList<UniTextRenderData> MeshResultBuffer
        => meshResultBuffer ??= new PooledList<UniTextRenderData>(4);

    public static PooledList<int> AcquireGlyphIndexList(int capacity)
    {
        var pool = GlyphListPool;
        if (pool.Count > 0)
        {
            var result = pool.Pop();
            result.EnsureCapacity(capacity);
            return result;
        }

        return new PooledList<int>(capacity);
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
