using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

public static class UniTextPoolStats
{
    public static void LogAll()
    {
        var instanceCount = Interlocked.Exchange(ref UniTextBuffers.instanceCount, 0);
        var rentBuffersCallCount = Interlocked.Exchange(ref UniTextBuffers.rentBuffersCallCount, 0);
        if (instanceCount > 0 || rentBuffersCallCount > 0)
        {
            Debug.Log($"[CommonData] Instances:{instanceCount} RentBuffers:{rentBuffersCallCount}");
        }

        var processCallCount = Interlocked.Exchange(ref TextProcessor.processCallCount, 0);
        var ensureShapingCallCount = Interlocked.Exchange(ref TextProcessor.ensureShapingCallCount, 0);
        var doFullShapingCallCount = Interlocked.Exchange(ref TextProcessor.doFullShapingCallCount, 0);
        if (processCallCount > 0)
        {
            Debug.Log($"[TextProcessor] Process:{processCallCount} EnsureShaping:{ensureShapingCallCount} DoFullShaping:{doFullShapingCallCount}");
        }

        var bidiProcessCallCount = Interlocked.Exchange(ref BidiEngine.processCallCount, 0);
        var buildIsoRunSeqCallCount = Interlocked.Exchange(ref BidiEngine.buildIsoRunSeqCallCount, 0);
        var buildIsoRunSeqForParagraphCallCount = Interlocked.Exchange(ref BidiEngine.buildIsoRunSeqForParagraphCallCount, 0);
        if (bidiProcessCallCount > 0 || buildIsoRunSeqCallCount > 0)
        {
            Debug.Log($"[BidiEngine] ProcessInternal:{bidiProcessCallCount} BuildIsoRunSeq:{buildIsoRunSeqCallCount} BuildIsoRunSeqForParagraph:{buildIsoRunSeqForParagraphCallCount}");
        }

        UniTextArrayPool<int>.LogStats();
        UniTextArrayPool<uint>.LogStats();
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

        UniTextArrayPool<Vector3>.LogStats();
        UniTextArrayPool<Vector4>.LogStats();
        UniTextArrayPool<Vector2>.LogStats();
        UniTextArrayPool<Color32>.LogStats();
    }

    /// <summary>
    /// Log only pools with active leaks (rents > returns).
    /// </summary>
    public static void LogLeaks()
    {
        Debug.Log("=== UniText Pool Leak Check ===");

        UniTextArrayPool<int>.LogLeaks();
        UniTextArrayPool<uint>.LogLeaks();
        UniTextArrayPool<byte>.LogLeaks();
        UniTextArrayPool<float>.LogLeaks();
        UniTextArrayPool<bool>.LogLeaks();

        UniTextArrayPool<BidiParagraph>.LogLeaks();
        UniTextArrayPool<UnicodeScript>.LogLeaks();
        UniTextArrayPool<TextRun>.LogLeaks();
        UniTextArrayPool<ShapedRun>.LogLeaks();
        UniTextArrayPool<ShapedGlyph>.LogLeaks();
        UniTextArrayPool<CachedGlyphData>.LogLeaks();
        UniTextArrayPool<TextLine>.LogLeaks();
        UniTextArrayPool<PositionedGlyph>.LogLeaks();

        UniTextArrayPool<Vector3>.LogLeaks();
        UniTextArrayPool<Vector4>.LogLeaks();
        UniTextArrayPool<Vector2>.LogLeaks();
        UniTextArrayPool<Color32>.LogLeaks();

        Debug.Log("=== End Leak Check ===");
    }
}

public static class UniTextArrayPool<T>
{
    private const int BucketCount = 12;
    private const int MinBucketSize = 32;
    private const int MaxArraysPerThreadLocal = 8;
    private const int MaxArraysPerShared = 1024;

    [ThreadStatic] private static T[][] threadLocalArrays;
    [ThreadStatic] private static int[] threadLocalCounts;

    private static readonly ConcurrentQueue<T[]>[] sharedBuckets;
    private static readonly int[] sharedCounts;

    public static int totalRents;
    public static int poolHits;
    public static int poolMisses;
    public static int sharedHits;
    public static int totalReturns;
    public static int returnRejectedTooLarge;
    public static int returnRejectedWrongSize;
    public static int returnRejectedPoolFull;

    public static int cumulativeRents;
    public static int cumulativeReturns;
    public static int cumulativeAllocations;
    public static int largestRentRequested;

    static UniTextArrayPool()
    {
        sharedBuckets = new ConcurrentQueue<T[]>[BucketCount];
        sharedCounts = new int[BucketCount];
        for (var i = 0; i < BucketCount; i++)
            sharedBuckets[i] = new ConcurrentQueue<T[]>();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void EnsureThreadLocalInitialized()
    {
        if (threadLocalArrays != null) return;
        threadLocalArrays = new T[BucketCount][];
        threadLocalCounts = new int[BucketCount];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T[] Rent(int minimumLength)
    {
        int current;
        while ((current = largestRentRequested) < minimumLength)
            Interlocked.CompareExchange(ref largestRentRequested, minimumLength, current);

        var bucketIndex = GetBucketIndex(minimumLength);
        if (bucketIndex < 0)
        {
            Interlocked.Increment(ref poolMisses);
            Interlocked.Increment(ref cumulativeRents);
            Interlocked.Increment(ref cumulativeAllocations);
            return new T[minimumLength];
        }

        Interlocked.Increment(ref totalRents);
        Interlocked.Increment(ref cumulativeRents);
        var bucketSize = MinBucketSize << bucketIndex;

        EnsureThreadLocalInitialized();
        if (threadLocalCounts[bucketIndex] > 0)
        {
            threadLocalCounts[bucketIndex] = 0;
            var arr = threadLocalArrays[bucketIndex];
            threadLocalArrays[bucketIndex] = null;
            Interlocked.Increment(ref poolHits);
            return arr;
        }

        if (sharedBuckets[bucketIndex].TryDequeue(out var sharedArr))
        {
            Interlocked.Decrement(ref sharedCounts[bucketIndex]);
            Interlocked.Increment(ref sharedHits);
            return sharedArr;
        }

        Interlocked.Increment(ref poolMisses);
        Interlocked.Increment(ref cumulativeAllocations);
        return new T[bucketSize];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Return(T[] array)
    {
        if (array == null) return;

        Interlocked.Increment(ref cumulativeReturns);

        var bucketIndex = GetBucketIndex(array.Length);
        if (bucketIndex < 0)
        {
            Interlocked.Increment(ref returnRejectedTooLarge);
            return;
        }

        var bucketSize = MinBucketSize << bucketIndex;
        if (array.Length != bucketSize)
        {
            Interlocked.Increment(ref returnRejectedWrongSize);
            return;
        }

        Interlocked.Increment(ref totalReturns);

        EnsureThreadLocalInitialized();
        if (threadLocalCounts[bucketIndex] == 0)
        {
            threadLocalArrays[bucketIndex] = array;
            threadLocalCounts[bucketIndex] = 1;
            return;
        }

        if (sharedCounts[bucketIndex] < MaxArraysPerShared)
        {
            sharedBuckets[bucketIndex].Enqueue(array);
            Interlocked.Increment(ref sharedCounts[bucketIndex]);
        }
        else
        {
            Interlocked.Increment(ref returnRejectedPoolFull);
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
        if (threadLocalArrays != null)
        {
            Array.Clear(threadLocalArrays, 0, BucketCount);
            Array.Clear(threadLocalCounts, 0, BucketCount);
        }

        for (var i = 0; i < BucketCount; i++)
        {
            while (sharedBuckets[i].TryDequeue(out _)) { }
            sharedCounts[i] = 0;
        }
    }

    public static void LogStats()
    {
        var rents = Interlocked.Exchange(ref totalRents, 0);
        var hits = Interlocked.Exchange(ref poolHits, 0);
        var shared = Interlocked.Exchange(ref sharedHits, 0);
        var misses = Interlocked.Exchange(ref poolMisses, 0);
        var returns = Interlocked.Exchange(ref totalReturns, 0);
        var rejTooLarge = Interlocked.Exchange(ref returnRejectedTooLarge, 0);
        var rejWrongSize = Interlocked.Exchange(ref returnRejectedWrongSize, 0);
        var rejPoolFull = Interlocked.Exchange(ref returnRejectedPoolFull, 0);

        if (rents == 0 && misses == 0) return;

        var activeRents = cumulativeRents - cumulativeReturns;
        var totalRejected = rejTooLarge + rejWrongSize + rejPoolFull;

        var msg = $"[Pool<{typeof(T).Name}>] Rents:{rents} Hits:{hits} SharedHits:{shared} Misses:{misses} | Returns:{returns} Active:{activeRents}";
        if (totalRejected > 0)
            msg += $" | Rejected: TooLarge:{rejTooLarge} WrongSize:{rejWrongSize} PoolFull:{rejPoolFull}";
        if (largestRentRequested > 8192)
            msg += $" | LargestRequest:{largestRentRequested}";

        Debug.Log(msg);
    }

    public static void LogLeaks()
    {
        var activeRents = cumulativeRents - cumulativeReturns;
        if (activeRents > 0)
        {
            Debug.LogWarning($"[Pool<{typeof(T).Name}>] LEAK: {activeRents} arrays not returned (Rents:{cumulativeRents} Returns:{cumulativeReturns} Allocations:{cumulativeAllocations})");
        }
    }

    public static void ResetLeakTracking()
    {
        Interlocked.Exchange(ref cumulativeRents, 0);
        Interlocked.Exchange(ref cumulativeReturns, 0);
        Interlocked.Exchange(ref cumulativeAllocations, 0);
        Interlocked.Exchange(ref largestRentRequested, 0);
    }

    public static string GetStats()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine($"UniTextArrayPool<{typeof(T).Name}>:");

        for (var i = 0; i < BucketCount; i++)
        {
            var size = MinBucketSize << i;
            var threadLocal = threadLocalArrays != null && threadLocalCounts[i] > 0 ? 1 : 0;
            var shared = sharedCounts[i];
            sb.AppendLine($"  [{size}]: ThreadLocal={threadLocal} Shared={shared}");
        }

        return sb.ToString();
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

    /// <summary>
    /// Ensure capacity and reset count to 0. Uses direct allocation (no pooling).
    /// Safe for cross-thread use where buffer lives longer than a single operation.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void EnsureCapacityAndClear(int required)
    {
        if (Capacity < required)
            GrowDirect(required);
        count = 0;
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

    /// <summary>
    /// Grow using direct allocation without pooling.
    /// </summary>
    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowDirect(int required)
    {
        var oldLen = data?.Length ?? 0;
        var newSize = oldLen == 0 ? Math.Max(required, 64) : Math.Max(required, oldLen * 2);
        var newData = new T[newSize];
        if (oldLen > 0 && count > 0)
            data.AsSpan(0, count).CopyTo(newData);
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
