using System;
using System.Runtime.CompilerServices;
using UnityEngine;


public static class UniTextPoolStats
{
    public static void LogAll()
    {
        if (CommonData.instanceCount > 0 || CommonData.rentBuffersCallCount > 0)
        {
            Debug.Log(
                $"[CommonData] Instances:{CommonData.instanceCount} RentBuffers:{CommonData.rentBuffersCallCount}");
            CommonData.instanceCount = 0;
            CommonData.rentBuffersCallCount = 0;
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