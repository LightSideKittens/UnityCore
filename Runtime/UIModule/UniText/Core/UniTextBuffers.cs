using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class UniTextBuffers
{
    public static int instanceCount;
    public static int rentBuffersCallCount;

    private const int MinCodepointCapacity = 32;
    private const int MinRunCapacity = 64;
    private const int MinGlyphCapacity = 32;
    private const int MinLineCapacity = 32;
    private const int MinParagraphCapacity = 8;

    public PooledBuffer<int> codepoints;
    public PooledBuffer<BidiParagraph> bidiParagraphs;
    public PooledBuffer<TextRun> runs;
    public PooledBuffer<ShapedRun> shapedRuns;
    public PooledBuffer<ShapedGlyph> shapedGlyphs;
    public PooledBuffer<float> cpWidths;  // Codepoint widths - computed after shaping
    public PooledBuffer<bool> breakOpportunities;  // Line break opportunities - computed after parse
    public PooledBuffer<TextLine> lines;
    public PooledBuffer<ShapedRun> orderedRuns;
    public PooledBuffer<PositionedGlyph> positionedGlyphs;

    public byte[] bidiLevels;
    public UnicodeScript[] scripts;
    public float[] startMargins;

    public TextDirection baseDirection;
    public float shapingFontSize;

    public CachedGlyphData[] glyphDataCache;
    public bool hasValidGlyphCache;

    public bool isRented;

    private Dictionary<string, Array> attributeArrays;

    public T[] GetOrCreateAttributeArray<T>(string key, int minCapacity) where T : unmanaged
    {
        attributeArrays ??= new Dictionary<string, Array>(8);

        if (attributeArrays.TryGetValue(key, out var existing))
            return (T[])existing;

        var arr = UniTextArrayPool<T>.Rent(Math.Max(minCapacity, 32));
        arr.AsSpan().Clear();
        attributeArrays[key] = arr;
        return arr;
    }

    public T[] GetAttributeArray<T>(string key) where T : unmanaged
    {
        if (attributeArrays != null && attributeArrays.TryGetValue(key, out var arr))
            return (T[])arr;
        return null;
    }

    public T[] GrowAttributeArray<T>(string key, int required) where T : unmanaged
    {
        if (attributeArrays == null || !attributeArrays.TryGetValue(key, out var existing))
            return GetOrCreateAttributeArray<T>(key, required);

        var oldArray = (T[])existing;
        if (oldArray.Length >= required)
            return oldArray;

        var newSize = Math.Max(required, oldArray.Length * 2);
        var newArray = UniTextArrayPool<T>.Rent(newSize);
        oldArray.AsSpan().CopyTo(newArray);
        newArray.AsSpan(oldArray.Length).Clear();
        UniTextArrayPool<T>.Return(oldArray);
        attributeArrays[key] = newArray;
        return newArray;
    }

    public void ReleaseAttributeArray(string key)
    {
        if (attributeArrays == null || !attributeArrays.TryGetValue(key, out var arr))
            return;

        if (arr is byte[] byteArr) UniTextArrayPool<byte>.Return(byteArr);
        else if (arr is uint[] uintArr) UniTextArrayPool<uint>.Return(uintArr);
        attributeArrays.Remove(key);
    }

    private void ClearAllAttributes()
    {
        if (attributeArrays == null) return;
        foreach (var arr in attributeArrays.Values)
        {
            if (arr is byte[] byteArr) byteArr.AsSpan().Clear();
            else if (arr is uint[] uintArr) uintArr.AsSpan().Clear();
        }
    }

    private void ReturnAllAttributes()
    {
        if (attributeArrays == null) return;
        foreach (var arr in attributeArrays.Values)
        {
            if (arr is byte[] byteArr) UniTextArrayPool<byte>.Return(byteArr);
            else if (arr is uint[] uintArr) UniTextArrayPool<uint>.Return(uintArr);
        }
        attributeArrays.Clear();
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


    public void EnsureRentBuffers(int textLength)
    {
        if (isRented) return;
        rentBuffersCallCount++;

        var codepointCapacity = EstimateCapacity(textLength, MinCodepointCapacity);
        var glyphCapacity = EstimateCapacity(textLength, MinGlyphCapacity);

        codepoints.Rent(codepointCapacity);
        bidiParagraphs.Rent(MinParagraphCapacity);
        runs.Rent(MinRunCapacity);
        shapedRuns.Rent(MinRunCapacity);
        shapedGlyphs.Rent(glyphCapacity);
        cpWidths.Rent(codepointCapacity);
        breakOpportunities.Rent(codepointCapacity + 1);  // +1 for break after last codepoint
        lines.Rent(MinLineCapacity);
        orderedRuns.Rent(MinRunCapacity);
        positionedGlyphs.Rent(glyphCapacity);

        bidiLevels = UniTextArrayPool<byte>.Rent(codepointCapacity);
        scripts = UniTextArrayPool<UnicodeScript>.Rent(codepointCapacity);
        startMargins = UniTextArrayPool<float>.Rent(codepointCapacity);
        glyphDataCache = UniTextArrayPool<CachedGlyphData>.Rent(glyphCapacity);

        isRented = true;
        Reset();
    }


    public void EnsureReturnBuffers()
    {
        if (!isRented) return;

        startMargins.AsSpan().Clear();
        hasValidGlyphCache = false;

        codepoints.Return();
        bidiParagraphs.Return();
        runs.Return();
        shapedRuns.Return();
        shapedGlyphs.Return();
        cpWidths.Return();
        breakOpportunities.Return();
        lines.Return();
        orderedRuns.Return();
        positionedGlyphs.Return();

        UniTextArrayPool<byte>.Return(bidiLevels);
        UniTextArrayPool<UnicodeScript>.Return(scripts);
        UniTextArrayPool<float>.Return(startMargins);
        UniTextArrayPool<CachedGlyphData>.Return(glyphDataCache);

        bidiLevels = null;
        scripts = null;
        startMargins = null;
        glyphDataCache = null;

        ReturnAllAttributes();

        isRented = false;
    }

    public void Reset()
    {
        var cpCount = codepoints.count;

        if (startMargins != null && cpCount > 0)
            startMargins.AsSpan(0, cpCount).Clear();

        ClearAllAttributes();

        codepoints.FakeClear();
        bidiParagraphs.FakeClear();
        runs.FakeClear();
        shapedRuns.FakeClear();
        shapedGlyphs.FakeClear();
        cpWidths.FakeClear();
        breakOpportunities.FakeClear();
        lines.FakeClear();
        orderedRuns.FakeClear();
        positionedGlyphs.FakeClear();

        hasValidGlyphCache = false;
        baseDirection = TextDirection.LeftToRight;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCodepointCapacity(int required)
    {
        if (codepoints.Capacity < required)
            GrowCodepointBuffers(required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBidiCapacity(int required)
    {
        if (bidiLevels.Length < required)
            Grow(ref bidiLevels, bidiLevels.Length, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureBidiParagraphCapacity(int required)
    {
        bidiParagraphs.EnsureCapacity(required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureScriptCapacity(int required)
    {
        if (scripts.Length < required)
            Grow(ref scripts, scripts.Length, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureRunCapacity(int required)
    {
        runs.EnsureCapacity(required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedRunCapacity(int required)
    {
        shapedRuns.EnsureCapacity(required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureShapedGlyphCapacity(int required)
    {
        shapedGlyphs.EnsureCapacity(required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureGlyphCacheCapacity(int required)
    {
        if (glyphDataCache == null || glyphDataCache.Length < required)
            Grow(ref glyphDataCache, glyphDataCache?.Length ?? 0, required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureLineCapacity(int required)
    {
        lines.EnsureCapacity(required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureOrderedRunCapacity(int required)
    {
        orderedRuns.EnsureCapacity(required);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsurePositionedGlyphCapacity(int required)
    {
        positionedGlyphs.EnsureCapacity(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void Grow<T>(ref T[] buffer, int count, int required)
    {
        var newSize = Math.Max(required, buffer.Length * 2);
        var newBuffer = UniTextArrayPool<T>.Rent(newSize);
        buffer.AsSpan(0, count).CopyTo(newBuffer);
        UniTextArrayPool<T>.Return(buffer);
        buffer = newBuffer;
    }

    private void GrowCodepointBuffers(int required)
    {
        var newSize = Math.Max(required, codepoints.Capacity * 2);
        var cpCount = codepoints.count;

        codepoints.EnsureCapacity(newSize);

        var newBidiLevels = UniTextArrayPool<byte>.Rent(newSize);
        bidiLevels.AsSpan(0, Math.Min(cpCount, bidiLevels.Length)).CopyTo(newBidiLevels);
        UniTextArrayPool<byte>.Return(bidiLevels);
        bidiLevels = newBidiLevels;

        var newScripts = UniTextArrayPool<UnicodeScript>.Rent(newSize);
        scripts.AsSpan(0, Math.Min(cpCount, scripts.Length)).CopyTo(newScripts);
        UniTextArrayPool<UnicodeScript>.Return(scripts);
        scripts = newScripts;

        var newMargins = UniTextArrayPool<float>.Rent(newSize);
        startMargins.AsSpan(0, Math.Min(cpCount, startMargins.Length)).CopyTo(newMargins);
        UniTextArrayPool<float>.Return(startMargins);
        startMargins = newMargins;
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