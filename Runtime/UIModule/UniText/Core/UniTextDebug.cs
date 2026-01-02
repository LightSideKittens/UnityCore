using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine.Profiling;

public static class UniTextDebug
{
    public static bool Enabled;
    public static bool ProfilerEnabled = true;
    public static bool CountersEnabled = true;


    #region Counters

    public static int TextProcessor_ProcessCount;
    public static int TextProcessor_EnsureShapingCount;
    public static int TextProcessor_DoFullShapingCount;

    public static int Buffers_InstanceCount;
    public static int Buffers_RentCount;

    public static int Bidi_ProcessCount;
    public static int Bidi_BuildIsoRunSeqCount;
    public static int Bidi_BuildIsoRunSeqForParagraphCount;

    public static int Pool_TotalRents;
    public static int Pool_PoolHits;
    public static int Pool_PoolMisses;
    public static int Pool_SharedHits;
    public static int Pool_TotalReturns;
    public static int Pool_ReturnRejectedTooLarge;
    public static int Pool_ReturnRejectedWrongSize;
    public static int Pool_ReturnRejectedPoolFull;
    public static int Pool_CumulativeRents;
    public static int Pool_CumulativeReturns;
    public static int Pool_CumulativeAllocations;
    public static int Pool_LargestRentRequested;

    #endregion


    #region Profiler Wrappers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void BeginSample(string name)
    {
        if (Enabled && ProfilerEnabled)
            Profiler.BeginSample(name);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EndSample()
    {
        if (Enabled && ProfilerEnabled)
            Profiler.EndSample();
    }

    #endregion


    #region Counter Wrappers

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Increment(ref int counter)
    {
        if (Enabled && CountersEnabled)
            Interlocked.Increment(ref counter);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void TrackLargest(ref int current, int value)
    {
        if (!Enabled || !CountersEnabled) return;

        int snapshot;
        do
        {
            snapshot = current;
            if (value <= snapshot) return;
        } while (Interlocked.CompareExchange(ref current, value, snapshot) != snapshot);
    }

    #endregion


    #region Reset & Reporting

    public static void ResetAllCounters()
    {
        TextProcessor_ProcessCount = 0;
        TextProcessor_EnsureShapingCount = 0;
        TextProcessor_DoFullShapingCount = 0;

        Buffers_InstanceCount = 0;
        Buffers_RentCount = 0;

        Bidi_ProcessCount = 0;
        Bidi_BuildIsoRunSeqCount = 0;
        Bidi_BuildIsoRunSeqForParagraphCount = 0;

        Pool_TotalRents = 0;
        Pool_PoolHits = 0;
        Pool_PoolMisses = 0;
        Pool_SharedHits = 0;
        Pool_TotalReturns = 0;
        Pool_ReturnRejectedTooLarge = 0;
        Pool_ReturnRejectedWrongSize = 0;
        Pool_ReturnRejectedPoolFull = 0;
        Pool_CumulativeRents = 0;
        Pool_CumulativeReturns = 0;
        Pool_CumulativeAllocations = 0;
        Pool_LargestRentRequested = 0;
    }

    public static string GetReport()
    {
        return $@"=== UniText Debug Report ===

TextProcessor:
  Process calls: {TextProcessor_ProcessCount}
  EnsureShaping calls: {TextProcessor_EnsureShapingCount}
  DoFullShaping calls: {TextProcessor_DoFullShapingCount}

Buffers:
  Instances: {Buffers_InstanceCount}
  Rent calls: {Buffers_RentCount}

BidiEngine:
  Process calls: {Bidi_ProcessCount}
  BuildIsoRunSeq calls: {Bidi_BuildIsoRunSeqCount}
  BuildIsoRunSeqForParagraph calls: {Bidi_BuildIsoRunSeqForParagraphCount}

ArrayPool:
  Total rents: {Pool_TotalRents}
  Pool hits: {Pool_PoolHits}
  Pool misses: {Pool_PoolMisses}
  Shared hits: {Pool_SharedHits}
  Total returns: {Pool_TotalReturns}
  Rejected (too large): {Pool_ReturnRejectedTooLarge}
  Rejected (wrong size): {Pool_ReturnRejectedWrongSize}
  Rejected (pool full): {Pool_ReturnRejectedPoolFull}
  Cumulative rents: {Pool_CumulativeRents}
  Cumulative returns: {Pool_CumulativeReturns}
  Cumulative allocations: {Pool_CumulativeAllocations}
  Largest rent requested: {Pool_LargestRentRequested}

Pool efficiency: {(Pool_TotalRents > 0 ? (Pool_PoolHits + Pool_SharedHits) * 100f / Pool_TotalRents : 0):F1}%
";
    }

    public static void LogReport()
    {
        UnityEngine.Debug.Log(GetReport());
    }

    #endregion
}