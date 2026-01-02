using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;


public interface IAttributeData
{
    void Reset();
    void Release();
}


public sealed class PooledArrayAttribute<T> : IAttributeData
{
    public PooledBuffer<T> buffer;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => buffer.count;
    }

    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref buffer[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) => buffer.Add(item);
    public void EnsureCount(int required)
    {
        var needClear = buffer.data == null;
        buffer.EnsureCount(required);
        if (needClear)
        { 
            buffer.ClearData();
        }
    }

    public void Reset() => buffer.ClearData();
    public void Release() => buffer.Return();
}


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

    private Dictionary<string, IAttributeData> attributeData;

    public T GetOrCreateAttributeData<T>(string key) where T : class, IAttributeData, new()
    {
        attributeData ??= new Dictionary<string, IAttributeData>(8);

        if (attributeData.TryGetValue(key, out var existing))
            return (T)existing;

        var data = new T();
        attributeData[key] = data;
        return data;
    }

    public T GetAttributeData<T>(string key) where T : class, IAttributeData
    {
        if (attributeData != null && attributeData.TryGetValue(key, out var data))
            return (T)data;
        return null;
    }

    public void ReleaseAttributeData(string key)
    {
        if (attributeData == null || !attributeData.TryGetValue(key, out var data))
            return;

        data.Release();
        attributeData.Remove(key);
    }

    private void ResetAllAttributeData()
    {
        if (attributeData == null) return;
        foreach (var data in attributeData.Values)
            data.Reset();
    }

    private void ReleaseAllAttributeData()
    {
        if (attributeData == null) return;
        foreach (var data in attributeData.Values)
            data.Release();
        attributeData.Clear();
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

        bidiLevels.Rent(codepointCapacity);
        scripts.Rent(codepointCapacity);
        startMargins.EnsureCount(codepointCapacity);
        startMargins.ClearData();
        glyphDataCache.Rent(glyphCapacity);

        isRented = true;
        Reset();
    }


    public void EnsureReturnBuffers()
    {
        if (!isRented) return;
        
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

        ReleaseAllAttributeData();

        isRented = false;
    }

    public void Reset()
    {
        var cpCount = codepoints.count;

        if (startMargins.Capacity > 0 && cpCount > 0)
            startMargins.data.AsSpan(0, cpCount).Clear();

        ResetAllAttributeData();

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