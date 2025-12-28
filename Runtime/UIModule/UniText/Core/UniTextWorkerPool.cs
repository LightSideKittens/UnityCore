using System;
using System.Threading;
using UnityEngine;
using ThreadPriority = System.Threading.ThreadPriority;

/// <summary>
/// Thread pool with guaranteed thread affinity for UniText components.
/// Same component is always processed by the same thread across all pipeline phases.
/// </summary>
public static class UniTextWorkerPool
{
    private static readonly int ThreadCount = Math.Max(1, Environment.ProcessorCount - 1);
    private const int BarrierTimeoutMs = 5000; // 5 seconds timeout

    private static Thread[] workers;
    private static AutoResetEvent[] workReady;
    private static CountdownEvent barrier;
    private static volatile bool isInitialized;
    private static volatile bool isShuttingDown;

    // Editor domain reload handling
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        // Force shutdown of any existing threads before domain reload
        ForceShutdown();
    }

    private static void ForceShutdown()
    {
        lock (typeof(UniTextWorkerPool))
        {
            isShuttingDown = true;

            if (workReady != null)
            {
                for (var i = 0; i < workReady.Length; i++)
                    workReady[i]?.Set();
            }

            if (workers != null)
            {
                for (var i = 0; i < workers.Length; i++)
                    workers[i]?.Join(50); // Short timeout
            }

            // Dispose resources
            if (workReady != null)
            {
                for (var i = 0; i < workReady.Length; i++)
                {
                    try { workReady[i]?.Dispose(); } catch { }
                }
            }

            try { barrier?.Dispose(); } catch { }

            // Reset all state
            workers = null;
            workReady = null;
            barrier = null;
            threadStartIndices = null;
            threadEndIndices = null;
            threadExceptions = null;
            currentComponents = null;
            currentAction = null;
            isInitialized = false;
            isShuttingDown = false;
        }
    }

    // Current work data (set before signaling workers)
    private static UniText[] currentComponents;
    private static int currentComponentCount;
    private static Action<UniText> currentAction;

    // Per-thread component ranges (fixed distribution)
    private static int[] threadStartIndices;
    private static int[] threadEndIndices;

    // Exception handling
    private static Exception[] threadExceptions;
    private static volatile int exceptionCount;

    public static bool IsParallelSupported => ThreadCount > 1;

    public static void EnsureInitialized()
    {
        if (isInitialized || isShuttingDown) return;

        lock (typeof(UniTextWorkerPool))
        {
            if (isInitialized || isShuttingDown) return;

            isShuttingDown = false; // Ensure clean state

            workers = new Thread[ThreadCount];
            workReady = new AutoResetEvent[ThreadCount];
            threadStartIndices = new int[ThreadCount];
            threadEndIndices = new int[ThreadCount];
            threadExceptions = new Exception[ThreadCount];
            barrier = new CountdownEvent(1);

            for (var i = 0; i < ThreadCount; i++)
            {
                workReady[i] = new AutoResetEvent(false);

                var threadIdx = i;
                workers[i] = new Thread(() => WorkerLoop(threadIdx))
                {
                    IsBackground = true,
                    Name = $"UniTextWorker_{i}",
                    Priority = ThreadPriority.Normal
                };
                workers[i].Start();
            }

            Application.quitting += Shutdown;
            isInitialized = true;
        }
    }

    private static void Shutdown()
    {
        if (!isInitialized) return;

        isShuttingDown = true;

        // Wake up all workers to let them exit
        for (var i = 0; i < ThreadCount; i++)
            workReady[i].Set();

        // Wait for workers to finish (with timeout)
        for (var i = 0; i < ThreadCount; i++)
            workers[i].Join(100);

        isInitialized = false;
    }

    private static void WorkerLoop(int threadIdx)
    {
        while (!isShuttingDown)
        {
            try
            {
                workReady[threadIdx].WaitOne();
            }
            catch (ObjectDisposedException)
            {
                break;
            }

            if (isShuttingDown) break;

            var localBarrier = barrier;
            if (localBarrier == null) break;

            try
            {
                var starts = threadStartIndices;
                var ends = threadEndIndices;
                var components = currentComponents;
                var action = currentAction;

                if (starts != null && ends != null && components != null && action != null)
                {
                    var start = starts[threadIdx];
                    var end = ends[threadIdx];

                    for (var i = start; i < end; i++)
                    {
                        var comp = components[i];
                        if (comp != null)
                            action(comp);
                    }
                }
            }
            catch (Exception ex)
            {
                var exceptions = threadExceptions;
                if (exceptions != null)
                {
                    exceptions[threadIdx] = ex;
                    Interlocked.Increment(ref exceptionCount);
                }
            }

            try
            {
                localBarrier.Signal();
            }
            catch (ObjectDisposedException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Execute action on all components in parallel.
    /// Components are distributed to threads with fixed affinity:
    /// component[i] always goes to thread (i % ThreadCount).
    /// </summary>
    public static void Execute(UniText[] components, int count, Action<UniText> action)
    {
        if (count == 0) return;

        // Fallback to sequential if shutting down or single component
        if (count == 1 || !IsParallelSupported || isShuttingDown)
        {
            // Sequential fallback
            for (var i = 0; i < count; i++)
            {
                var comp = components[i];
                if (comp != null)
                    action(comp);
            }
            return;
        }

        EnsureInitialized();

        // Double-check after initialization (might have failed due to shutdown)
        if (!isInitialized || isShuttingDown)
        {
            for (var i = 0; i < count; i++)
            {
                var comp = components[i];
                if (comp != null)
                    action(comp);
            }
            return;
        }

        // Setup work
        currentComponents = components;
        currentComponentCount = count;
        currentAction = action;
        exceptionCount = 0;

        // Distribute components to threads (contiguous ranges for cache efficiency)
        var perThread = count / ThreadCount;
        var remainder = count % ThreadCount;
        var offset = 0;

        for (var i = 0; i < ThreadCount; i++)
        {
            threadStartIndices[i] = offset;
            var threadCount = perThread + (i < remainder ? 1 : 0);
            offset += threadCount;
            threadEndIndices[i] = offset;
            threadExceptions[i] = null;
        }

        // Reset barrier and signal workers
        barrier.Reset(ThreadCount);

        for (var i = 0; i < ThreadCount; i++)
            workReady[i].Set();

        // Wait for completion with polling to detect shutdown
        try
        {
            var localBarrier = barrier;
            if (localBarrier != null && !isShuttingDown)
            {
                // Poll with short timeout to allow checking shutdown flag
                const int pollIntervalMs = 10;
                var elapsed = 0;
                while (!localBarrier.Wait(pollIntervalMs) && !isShuttingDown && elapsed < BarrierTimeoutMs)
                {
                    elapsed += pollIntervalMs;
                }
            }
        }
        catch (ObjectDisposedException) { }
        catch (InvalidOperationException) { }

        // Clear references
        currentComponents = null;
        currentAction = null;

        // Check for exceptions
        if (exceptionCount > 0)
        {
            for (var i = 0; i < ThreadCount; i++)
            {
                if (threadExceptions[i] != null)
                {
                    Debug.LogException(threadExceptions[i]);
                    threadExceptions[i] = null;
                }
            }
        }
    }

    /// <summary>
    /// Get thread index for a component index.
    /// Use this to ensure same thread affinity across phases.
    /// </summary>
    public static int GetThreadIndexForComponent(int componentIndex, int totalCount)
    {
        if (totalCount <= 1 || !IsParallelSupported) return 0;

        var perThread = totalCount / ThreadCount;
        var remainder = totalCount % ThreadCount;

        // Find which thread owns this index
        var offset = 0;
        for (var i = 0; i < ThreadCount; i++)
        {
            var threadCount = perThread + (i < remainder ? 1 : 0);
            if (componentIndex < offset + threadCount)
                return i;
            offset += threadCount;
        }

        return ThreadCount - 1;
    }
}
