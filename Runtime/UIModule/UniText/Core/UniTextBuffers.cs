using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;


public sealed class UniTextBuffers
{
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
    public PooledBuffer<float> cpWidths;

    public PooledBuffer<bool> breakOpportunities;
    public PooledBuffer<TextLine> lines;
    public PooledBuffer<ShapedRun> orderedRuns;
    public PooledBuffer<PositionedGlyph> positionedGlyphs;

    public PooledBuffer<uint> virtualCodepoints;

    public PooledBuffer<byte> bidiLevels;
    public PooledBuffer<UnicodeScript> scripts;
    public PooledBuffer<float> startMargins;

    public TextDirection baseDirection;
    public float shapingFontSize;

    public PooledBuffer<CachedGlyphData> glyphDataCache;
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
        UniTextDebug.Increment(ref UniTextDebug.Buffers_RentCount);

        var codepointCapacity = EstimateCapacity(textLength, MinCodepointCapacity);
        var glyphCapacity = EstimateCapacity(textLength, MinGlyphCapacity);

        codepoints.Rent(codepointCapacity);
        bidiParagraphs.Rent(MinParagraphCapacity);
        runs.Rent(MinRunCapacity);
        shapedRuns.Rent(MinRunCapacity);
        shapedGlyphs.Rent(glyphCapacity);
        cpWidths.Rent(codepointCapacity);
        breakOpportunities.Rent(codepointCapacity + 1);
        lines.Rent(MinLineCapacity);
        orderedRuns.Rent(MinRunCapacity);
        positionedGlyphs.Rent(glyphCapacity);
        virtualCodepoints.Rent(MinCodepointCapacity);

        bidiLevels.Rent(codepointCapacity);
        scripts.Rent(codepointCapacity);
        startMargins.Rent(codepointCapacity);
        startMargins.Span.Clear();
        glyphDataCache.Rent(glyphCapacity);

        isRented = true;
        Reset();
    }


    public void EnsureReturnBuffers()
    {
        if (!isRented) return;

        startMargins.Span.Clear();
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
        virtualCodepoints.Return();

        bidiLevels.Return();
        scripts.Return();
        startMargins.Return();
        glyphDataCache.Return();

        ReturnAllAttributes();

        isRented = false;
    }

    public void Reset()
    {
        var cpCount = codepoints.count;

        if (startMargins.Capacity > 0 && cpCount > 0)
            startMargins.data.AsSpan(0, cpCount).Clear();

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
        virtualCodepoints.FakeClear();
        bidiLevels.FakeClear();
        scripts.FakeClear();
        glyphDataCache.FakeClear();

        hasValidGlyphCache = false;
        baseDirection = TextDirection.LeftToRight;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCodepointCapacity(int required)
    {
        if (codepoints.Capacity < required)
            GrowCodepointBuffers(required);
    }

    private void GrowCodepointBuffers(int required)
    {
        var newSize = Math.Max(required, codepoints.Capacity * 2);

        codepoints.EnsureCapacity(newSize);
        bidiLevels.EnsureCapacity(newSize);
        scripts.EnsureCapacity(newSize);

        var oldCapacity = startMargins.Capacity;
        startMargins.EnsureCapacity(newSize);
        if (startMargins.Capacity > oldCapacity)
            startMargins.data.AsSpan(oldCapacity).Clear();
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
    [ThreadStatic] private static FontCacheEntry[] cache;

    private static void EnsureInitialized()
    {
        if (cache != null) return;
        cache = new FontCacheEntry[CacheSize];
        for (var i = 0; i < CacheSize; i++)
            cache[i] = new FontCacheEntry(-1, -1, -1);
    }

    public static bool TryGet(int codepoint, int preferredFontId, out int resultFontId)
    {
        EnsureInitialized();
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
        EnsureInitialized();
        var index = (codepoint ^ (preferredFontId << 16)) & CacheMask;
        cache[index] = new FontCacheEntry(codepoint, preferredFontId, resultFontId);
    }

    public static void Clear()
    {
        if (cache == null) return;
        for (var i = 0; i < CacheSize; i++)
            cache[i] = new FontCacheEntry(-1, -1, -1);
    }
}


public static class SharedMeshPool
{
    private static readonly List<Mesh> available = new(16);

    public static Mesh Acquire(string name)
    {
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

    public static void Release(List<Mesh> meshes)
    {
        if (meshes == null) return;

        foreach (var mesh in meshes)
        {
            if (mesh != null)
            {
                mesh.Clear();
                available.Add(mesh);
            }
        }
    }
}