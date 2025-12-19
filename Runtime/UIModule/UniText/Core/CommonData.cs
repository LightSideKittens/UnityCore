using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class CommonData
{
    public static int instanceCount;
    public static int rentBuffersCallCount;

    private const int MinCodepointCapacity = 32;
    private const int MinRunCapacity = 64;
    private const int MinGlyphCapacity = 32;
    private const int MinLineCapacity = 32;
    private const int MinParagraphCapacity = 8;


    public static CommonData Current { get; set; }

    public int[] codepoints;
    public int codepointCount;

    public byte[] bidiLevels;
    public BidiParagraph[] bidiParagraphs;
    public int bidiParagraphCount;
    public TextDirection baseDirection;

    public UnicodeScript[] scripts;

    public TextRun[] runs;
    public int runCount;

    public ShapedRun[] shapedRuns;
    public int shapedRunCount;
    public ShapedGlyph[] shapedGlyphs;
    public int shapedGlyphCount;
    public float shapingFontSize;

    public CachedGlyphData[] glyphDataCache;
    public bool hasValidGlyphCache;

    public TextLine[] lines;
    public int lineCount;
    public ShapedRun[] orderedRuns;
    public int orderedRunCount;

    public PositionedGlyph[] positionedGlyphs;
    public int positionedGlyphCount;


    public Color32[] glyphColors;


    public float[] startMargins;

    public int peakCodepointCount;
    public int peakRunCount;
    public int peakGlyphCount;

    private bool isRented;

    private class SharedAttribute
    {
        public object buffer;
        public int refCount;
    }

    private Dictionary<string, SharedAttribute> sharedAttributes;


    public ArrayPoolBuffer<T> AcquireAttribute<T>(string key, int initialCapacity) where T : struct
    {
        sharedAttributes ??= new Dictionary<string, SharedAttribute>(8);

        if (!sharedAttributes.TryGetValue(key, out var attr))
        {
            attr = new SharedAttribute
            {
                buffer = new ArrayPoolBuffer<T>(initialCapacity),
                refCount = 0
            };
            sharedAttributes[key] = attr;
        }

        attr.refCount++;
        return (ArrayPoolBuffer<T>)attr.buffer;
    }


    public ArrayPoolBuffer<T> GetAttribute<T>(string key) where T : struct
    {
        if (sharedAttributes != null && sharedAttributes.TryGetValue(key, out var attr))
            return (ArrayPoolBuffer<T>)attr.buffer;
        return null;
    }


    public void ReleaseAttribute(string key)
    {
        if (sharedAttributes == null || !sharedAttributes.TryGetValue(key, out var attr))
            return;

        attr.refCount--;
        if (attr.refCount <= 0)
        {
            if (attr.buffer is IPoolReturnable poolable)
                poolable.ReturnToPool();
            sharedAttributes.Remove(key);
        }
    }


    public void ClearAllAttributes()
    {
        if (sharedAttributes == null) return;
        foreach (var attr in sharedAttributes.Values)
            if (attr.buffer is IClearable clearable)
                clearable.Clear();
    }


    public void ReturnAllAttributes()
    {
        if (sharedAttributes == null) return;
        foreach (var attr in sharedAttributes.Values)
            if (attr.buffer is IPoolReturnable poolable)
                poolable.ReturnToPool();
        sharedAttributes.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetGlyphScale(float targetFontSize)
    {
        return shapingFontSize > 0 ? targetFontSize / shapingFontSize : 1f;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int EstimateCapacity(int textLength, int minCapacity)
    {
        if (textLength <= minCapacity) return minCapacity;
        return Mathf.NextPowerOfTwo(textLength);
    }


    public void RentBuffers(int textLength)
    {
        if (isRented) return;
        rentBuffersCallCount++;

        var codepointCapacity = EstimateCapacity(textLength, MinCodepointCapacity);
        var glyphCapacity = EstimateCapacity(textLength, MinGlyphCapacity);

        codepoints = UniTextArrayPool<int>.Rent(codepointCapacity);
        bidiLevels = UniTextArrayPool<byte>.Rent(codepointCapacity);
        bidiParagraphs = UniTextArrayPool<BidiParagraph>.Rent(MinParagraphCapacity);
        scripts = UniTextArrayPool<UnicodeScript>.Rent(codepointCapacity);
        startMargins = UniTextArrayPool<float>.Rent(codepointCapacity);
        runs = UniTextArrayPool<TextRun>.Rent(MinRunCapacity);
        shapedRuns = UniTextArrayPool<ShapedRun>.Rent(MinRunCapacity);
        shapedGlyphs = UniTextArrayPool<ShapedGlyph>.Rent(glyphCapacity);
        glyphDataCache = UniTextArrayPool<CachedGlyphData>.Rent(glyphCapacity);
        lines = UniTextArrayPool<TextLine>.Rent(MinLineCapacity);
        orderedRuns = UniTextArrayPool<ShapedRun>.Rent(MinRunCapacity);
        positionedGlyphs = UniTextArrayPool<PositionedGlyph>.Rent(glyphCapacity);
        glyphColors = UniTextArrayPool<Color32>.Rent(glyphCapacity);

        isRented = true;
        Reset();
    }


    public void ReturnBuffers()
    {
        if (!isRented) return;

        if (codepoints != null)
        {
            UniTextArrayPool<int>.Return(codepoints);
            codepoints = null;
        }

        if (bidiLevels != null)
        {
            UniTextArrayPool<byte>.Return(bidiLevels);
            bidiLevels = null;
        }

        if (bidiParagraphs != null)
        {
            UniTextArrayPool<BidiParagraph>.Return(bidiParagraphs);
            bidiParagraphs = null;
        }

        if (scripts != null)
        {
            UniTextArrayPool<UnicodeScript>.Return(scripts);
            scripts = null;
        }

        if (startMargins != null)
        {
            startMargins.AsSpan().Clear();
            UniTextArrayPool<float>.Return(startMargins);
            startMargins = null;
        }

        if (runs != null)
        {
            UniTextArrayPool<TextRun>.Return(runs);
            runs = null;
        }

        if (shapedRuns != null)
        {
            UniTextArrayPool<ShapedRun>.Return(shapedRuns);
            shapedRuns = null;
        }

        if (shapedGlyphs != null)
        {
            UniTextArrayPool<ShapedGlyph>.Return(shapedGlyphs);
            shapedGlyphs = null;
        }

        if (glyphDataCache != null)
        {
            UniTextArrayPool<CachedGlyphData>.Return(glyphDataCache);
            glyphDataCache = null;
        }

        hasValidGlyphCache = false;
        if (lines != null)
        {
            UniTextArrayPool<TextLine>.Return(lines);
            lines = null;
        }

        if (orderedRuns != null)
        {
            UniTextArrayPool<ShapedRun>.Return(orderedRuns);
            orderedRuns = null;
        }

        if (positionedGlyphs != null)
        {
            UniTextArrayPool<PositionedGlyph>.Return(positionedGlyphs);
            positionedGlyphs = null;
        }

        if (glyphColors != null)
        {
            UniTextArrayPool<Color32>.Return(glyphColors);
            glyphColors = null;
        }

        ReturnAllAttributes();

        isRented = false;

        if (Current == this)
            Current = null;
    }

    public void Reset()
    {
        var cpCount = codepointCount;
        if (cpCount > peakCodepointCount) peakCodepointCount = cpCount;
        if (runCount > peakRunCount) peakRunCount = runCount;
        if (shapedGlyphCount > peakGlyphCount) peakGlyphCount = shapedGlyphCount;

        if (startMargins != null && cpCount > 0)
            startMargins.AsSpan(0, cpCount).Clear();

        ClearAllAttributes();

        codepointCount = 0;
        bidiParagraphCount = 0;
        runCount = 0;
        shapedRunCount = 0;
        shapedGlyphCount = 0;
        hasValidGlyphCache = false;
        lineCount = 0;
        orderedRunCount = 0;
        positionedGlyphCount = 0;
        baseDirection = TextDirection.LeftToRight;
    }


    public void LogPeakUsage()
    {
        Debug.Log(
            $"[SharedTextBuffers] Peak usage: codepoints={peakCodepointCount}, runs={peakRunCount}, glyphs={peakGlyphCount}");
        Debug.Log(
            $"[SharedTextBuffers] Buffer sizes: codepoints={codepoints?.Length ?? 0}, runs={runs?.Length ?? 0}, glyphs={shapedGlyphs?.Length ?? 0}");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCodepointCapacity(int required)
    {
        if (codepoints.Length < required)
            BufferUtils.GrowCodepointBuffers(ref codepoints, ref bidiLevels, ref scripts, ref startMargins,
                codepointCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBidiCapacity(int required)
    {
        if (bidiLevels.Length < required)
            BufferUtils.Grow(ref bidiLevels, bidiLevels.Length, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBidiParagraphCapacity(int required)
    {
        if (bidiParagraphs.Length < required)
            BufferUtils.Grow(ref bidiParagraphs, bidiParagraphCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureScriptCapacity(int required)
    {
        if (scripts.Length < required)
            BufferUtils.Grow(ref scripts, scripts.Length, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureRunCapacity(int required)
    {
        BufferUtils.EnsureCapacity(ref runs, runCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedRunCapacity(int required)
    {
        BufferUtils.EnsureCapacity(ref shapedRuns, shapedRunCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedGlyphCapacity(int required)
    {
        BufferUtils.EnsureCapacity(ref shapedGlyphs, shapedGlyphCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureGlyphCacheCapacity(int required)
    {
        if (glyphDataCache == null || glyphDataCache.Length < required)
            BufferUtils.Grow(ref glyphDataCache, glyphDataCache?.Length ?? 0, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureLineCapacity(int required)
    {
        BufferUtils.EnsureCapacity(ref lines, lineCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureOrderedRunCapacity(int required)
    {
        BufferUtils.EnsureCapacity(ref orderedRuns, orderedRunCount, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsurePositionedGlyphCapacity(int required)
    {
        if (positionedGlyphs.Length < required)
            BufferUtils.GrowPositionedGlyphBuffers(ref positionedGlyphs, ref glyphColors, positionedGlyphCount,
                required);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        Current = null;
    }
}


public static class SharedFontCache
{
    private readonly struct FontCacheEntry
    {
        public readonly int codepoint;
        public readonly int preferredFontId;
        public readonly int resultFontId;

        public FontCacheEntry(int codepoint, int preferredFontId, int resultFontId)
        {
            this.codepoint = codepoint;
            this.preferredFontId = preferredFontId;
            this.resultFontId = resultFontId;
        }
    }

    private const int CacheSize = 4096;
    private const int CacheMask = CacheSize - 1;
    private static FontCacheEntry[] cache = new FontCacheEntry[CacheSize];

    static SharedFontCache()
    {
        Clear();
    }

    public static bool TryGet(int codepoint, int preferredFontId, out int resultFontId)
    {
        var index = (codepoint ^ (preferredFontId << 16)) & CacheMask;
        ref var entry = ref cache[index];

        if (entry.codepoint == codepoint && entry.preferredFontId == preferredFontId)
        {
            resultFontId = entry.resultFontId;
            return true;
        }

        resultFontId = -1;
        return false;
    }

    public static void Set(int codepoint, int preferredFontId, int resultFontId)
    {
        var index = (codepoint ^ (preferredFontId << 16)) & CacheMask;
        cache[index] = new FontCacheEntry(codepoint, preferredFontId, resultFontId);
    }

    public static void Clear()
    {
        for (var i = 0; i < CacheSize; i++)
            cache[i] = new FontCacheEntry(-1, -1, -1);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        Clear();
    }
}


public static class SharedMeshPool
{
    private static readonly List<Mesh> available = new(16);
    private static bool initialized;

    public static Mesh Acquire(string name)
    {
        EnsureInitialized();

        while (available.Count > 0)
        {
            var lastIndex = available.Count - 1;
            var mesh = available[lastIndex];
            available.RemoveAt(lastIndex);

            if (mesh != null)
            {
                mesh.Clear();
                mesh.name = name;
                return mesh;
            }
        }

        var newMesh = new Mesh();
        newMesh.name = name;
        return newMesh;
    }

    public static void Release(Mesh mesh)
    {
        if (mesh == null) return;
        mesh.Clear();
        available.Add(mesh);
    }

    public static void Release(List<Mesh> meshes)
    {
        if (meshes == null) return;

        foreach (var mesh in meshes)
            if (mesh != null)
            {
                mesh.Clear();
                available.Add(mesh);
            }
    }

    public static void ClearUnused()
    {
        for (var i = available.Count - 1; i >= 0; i--)
        {
            var mesh = available[i];
            if (mesh != null)
                UnityEngine.Object.Destroy(mesh);
        }

        available.Clear();
    }

    public static int PoolSize => available.Count;

    private static void EnsureInitialized()
    {
        if (initialized) return;
        initialized = true;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        ClearUnused();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        available.Clear();
        initialized = false;
    }
}