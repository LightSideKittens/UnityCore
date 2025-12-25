using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.Collections.Generic;

public static class UniTextPoolStats
{
    public static void LogAll()
    {
        if (UniTextBuffers.instanceCount > 0 || UniTextBuffers.rentBuffersCallCount > 0)
        {
            Debug.Log(
                $"[CommonData] Instances:{UniTextBuffers.instanceCount} RentBuffers:{UniTextBuffers.rentBuffersCallCount}");
            UniTextBuffers.instanceCount = 0;
            UniTextBuffers.rentBuffersCallCount = 0;
        }

        if (TextProcessor.processCallCount > 0)
        {
            Debug.Log(
                $"[TextProcessor] Process:{TextProcessor.processCallCount} EnsureShaping:{TextProcessor.ensureShapingCallCount} DoFullShaping:{TextProcessor.doFullShapingCallCount}");
            TextProcessor.processCallCount = 0;
            TextProcessor.ensureShapingCallCount = 0;
            TextProcessor.doFullShapingCallCount = 0;
        }

        if (BidiEngine.processCallCount > 0 || BidiEngine.buildIsoRunSeqCallCount > 0)
        {
            Debug.Log(
                $"[BidiEngine] ProcessInternal:{BidiEngine.processCallCount} BuildIsoRunSeq:{BidiEngine.buildIsoRunSeqCallCount} BuildIsoRunSeqForParagraph:{BidiEngine.buildIsoRunSeqForParagraphCallCount}");
            BidiEngine.processCallCount = 0;
            BidiEngine.buildIsoRunSeqCallCount = 0;
            BidiEngine.buildIsoRunSeqForParagraphCallCount = 0;
        }

        UniTextArrayPool<int>.LogStats();
        UniTextArrayPool<byte>.LogStats();
        UniTextArrayPool<float>.LogStats();
        UniTextArrayPool<bool>.LogStats();
        UniTextArrayPool<BidiParagraph>.LogStats();
        UniTextArrayPool<UnicodeScript>.LogStats();
        UniTextArrayPool<TextRun>.LogStats();
        UniTextArrayPool<ShapedRun>.LogStats();
        UniTextArrayPool<ShapedGlyph>.LogStats();
        UniTextArrayPool<CachedGlyphData>.LogStats();
        UniTextArrayPool<TextLine>.LogStats();
        UniTextArrayPool<PositionedGlyph>.LogStats();
        UniTextArrayPool<Color32>.LogStats();
    }
}

public static class UniTextArrayPool<T>
{
    private const int BucketCount = 9;
    private const int MinBucketSize = 32;
    private const int MaxArraysPerBucket = 512;

    private static readonly T[][][] buckets = new T[BucketCount][][];
    private static readonly int[] bucketCounts = new int[BucketCount];

    public static int totalRents;
    public static int poolHits;
    public static int poolMisses;
    public static int totalReturns;
    public static int returnRejected;

    static UniTextArrayPool()
    {
        for (var i = 0; i < BucketCount; i++)
            buckets[i] = new T[MaxArraysPerBucket][];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Rent(int minimumLength)
    {
        var bucketIndex = GetBucketIndex(minimumLength);
        if (bucketIndex < 0)
        {
            poolMisses++;
            return new T[minimumLength];
        }

        var bucketSize = MinBucketSize << bucketIndex;
        ref var count = ref bucketCounts[bucketIndex];
        var bucket = buckets[bucketIndex];

        totalRents++;
        if (count > 0)
        {
            count--;
            var arr = bucket[count];
            bucket[count] = null;
            poolHits++;
            return arr;
        }

        poolMisses++;
        return new T[bucketSize];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(T[] array)
    {
        if (array == null) return;

        totalReturns++;
        var bucketIndex = GetBucketIndex(array.Length);
        if (bucketIndex < 0)
        {
            returnRejected++;
            return;
        }

        var bucketSize = MinBucketSize << bucketIndex;
        if (array.Length != bucketSize)
        {
            returnRejected++;
            return;
        }

        ref var count = ref bucketCounts[bucketIndex];
        if (count < MaxArraysPerBucket)
        {
            buckets[bucketIndex][count] = array;
            count++;
        }
        else
        {
            returnRejected++;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetBucketIndex(int size)
    {
        if (size <= MinBucketSize) return 0;
        if (size > MinBucketSize << (BucketCount - 1)) return -1;

        var shifted = (size - 1) / MinBucketSize;
        var index = 0;
        while (shifted > 0)
        {
            shifted >>= 1;
            index++;
        }

        return index;
    }


    public static void Clear()
    {
        for (var i = 0; i < BucketCount; i++)
        {
            Array.Clear(buckets[i], 0, bucketCounts[i]);
            bucketCounts[i] = 0;
        }
    }


    public static void LogStats()
    {
        if (totalRents == 0 && poolMisses == 0) return;
        Debug.Log(
            $"[Pool<{typeof(T).Name}>] Rents:{totalRents} Hits:{poolHits} Misses:{poolMisses} | Returns:{totalReturns} Rejected:{returnRejected}");
        totalRents = 0;
        poolHits = 0;
        poolMisses = 0;
        totalReturns = 0;
        returnRejected = 0;
    }


    public static string GetStats()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"UniTextArrayPool<{typeof(T).Name}>:");
        for (var i = 0; i < BucketCount; i++)
        {
            var size = MinBucketSize << i;
            sb.AppendLine($"  [{size}]: {bucketCounts[i]}/{MaxArraysPerBucket}");
        }

        return sb.ToString();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        Clear();
    }
}

public struct PooledBuffer<T>
{
    public T[] data;
    public int count;

    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => count;
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => data?.Length ?? 0;
    }

    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref data[i];
    }

    public Span<T> Span
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => data.AsSpan(0, count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Rent(int capacity)
    {
        if (capacity > 0)
            data = UniTextArrayPool<T>.Rent(capacity);
        count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return()
    {
        if (data != null && data.Length > 0)
        {
            UniTextArrayPool<T>.Return(data);
            data = null;
        }
        count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FakeClear()
    {
        count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (count > 0)
        {
            data.AsSpan(0, count).Clear();
            count = 0;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int required)
    {
        if (Capacity < required)
            Grow(required);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void Grow(int required)
    {
        var oldLen = data?.Length ?? 0;
        var newSize = oldLen == 0 ? Math.Max(required, 4) : Math.Max(required, oldLen * 2);
        var newData = UniTextArrayPool<T>.Rent(newSize);
        if (oldLen > 0)
        {
            data.AsSpan(0, count).CopyTo(newData);
            UniTextArrayPool<T>.Return(data);
        }
        data = newData;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (count >= Capacity)
            EnsureCapacity(count + 1);
        data[count++] = item;
    }

    public void RemoveRange(int index, int removeCount)
    {
        if (removeCount <= 0) return;
        count -= removeCount;
        if (index < count)
            Array.Copy(data, index + removeCount, data, index, count - index);
    }

    public void RemoveAt(int index)
    {
        count--;
        if (index < count)
            Array.Copy(data, index + 1, data, index, count - index);
    }

    public void Sort(Comparison<T> comparison)
    {
        if (count > 1)
            Array.Sort(data, 0, count, Comparer<T>.Create(comparison));
    }

    public void Sort(int index, int length, IComparer<T> comparer)
    {
        if (length > 1)
            Array.Sort(data, index, length, comparer);
    }
}

public sealed class PooledList<T>
{
    public PooledBuffer<T> buffer;

    public PooledList()
    {
        buffer = default;
    }

    public PooledList(int capacity)
    {
        buffer = default;
        buffer.Rent(capacity);
    }
    
    public int Count
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => buffer.count;
    }

    public int Capacity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => buffer.Capacity;
    }

    public ref T this[int i]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref buffer[i];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item) => buffer.Add(item);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear() => buffer.Clear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FakeClear() => buffer.FakeClear();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return() => buffer.Return();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacity(int required) => buffer.EnsureCapacity(required);

    public void RemoveRange(int index, int removeCount) => buffer.RemoveRange(index, removeCount);

    public void RemoveAt(int index) => buffer.RemoveAt(index);

    public void Sort(Comparison<T> comparison) => buffer.Sort(comparison);

    public void Sort(int index, int length, IComparer<T> comparer) => buffer.Sort(index, length, comparer);
}
