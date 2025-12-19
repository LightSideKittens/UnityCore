using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Profiling;
using Debug = UnityEngine.Debug;


public class UniTextPerformanceTest : MonoBehaviour
{
    [Header("Test Settings")] [Tooltip("Prefab with UniText component to instantiate")]
    public GameObject prefab;

    [Tooltip("Parent transform for instantiated objects (optional)")]
    public Transform parent;

    [Tooltip("Number of objects to create per iteration")] [Min(1)]
    public int objectsPerIteration = 100;

    [Tooltip("Number of test iterations")] [Min(1)]
    public int iterations = 10;

    [Tooltip("Frames to wait between creation and destruction")] [Min(1)]
    public int framesBetween = 1;

    [Header("CPU Results")] [SerializeField]
    private float lastInstantiateTimeMs;

    [SerializeField] private float lastDestroyTimeMs;

    [SerializeField] private float avgInstantiateTimeMs;

    [SerializeField] private float avgDestroyTimeMs;

    [SerializeField] private float totalTestTimeMs;

    [Header("Memory Results")] [SerializeField]
    private long lastInstantiateAllocBytes;

    [SerializeField] private long lastDestroyAllocBytes;

    [SerializeField] private long avgInstantiateAllocBytes;

    [SerializeField] private long avgDestroyAllocBytes;

    [SerializeField] private int gcCollectionsDuringTest;

    [SerializeField] private bool isRunning;

    private GameObject[] instances;
    private readonly Stopwatch stopwatch = new();

    private double totalInstantiateTime;
    private double totalDestroyTime;
    private long totalInstantiateAlloc;
    private long totalDestroyAlloc;
    private int completedIterations;

    [ContextMenu("Run Test")]
    public void RunTest()
    {
        if (isRunning)
        {
            Debug.LogWarning("Test already running!");
            return;
        }

        if (prefab == null)
        {
            Debug.LogError("UniTextPerformanceTest: Prefab not assigned!");
            return;
        }

        StartCoroutine(RunTestCoroutine());
    }

    [ContextMenu("Stop Test")]
    public void StopTest()
    {
        if (!isRunning) return;

        StopAllCoroutines();
        CleanupInstances();
        isRunning = false;
        Debug.Log("Test stopped.");
    }

    private IEnumerator RunTestCoroutine()
    {
        isRunning = true;
        totalInstantiateTime = 0;
        totalDestroyTime = 0;
        totalInstantiateAlloc = 0;
        totalDestroyAlloc = 0;
        completedIterations = 0;

        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();

        var gcCountBefore = System.GC.CollectionCount(0);
        var totalStopwatch = Stopwatch.StartNew();

        Debug.Log($"═══════════════════════════════════════════════════════════════");
        Debug.Log($"  UniText Performance Test Started");
        Debug.Log($"  Objects per iteration: {objectsPerIteration}");
        Debug.Log($"  Iterations: {iterations}");
        Debug.Log($"  Frames between create/destroy: {framesBetween}");
        Debug.Log($"═══════════════════════════════════════════════════════════════");

        instances = new GameObject[objectsPerIteration];
        var parentTransform = parent != null ? parent : transform;

        for (var iter = 0; iter < iterations; iter++)
        {
            var allocBefore = Profiler.GetTotalAllocatedMemoryLong();
            stopwatch.Restart();

            for (var i = 0; i < objectsPerIteration; i++) instances[i] = Instantiate(prefab, parentTransform);

            stopwatch.Stop();
            var allocAfter = Profiler.GetTotalAllocatedMemoryLong();

            var instantiateMs = stopwatch.Elapsed.TotalMilliseconds;
            var instantiateAlloc = allocAfter - allocBefore;

            totalInstantiateTime += instantiateMs;
            totalInstantiateAlloc += instantiateAlloc;
            lastInstantiateTimeMs = (float)instantiateMs;
            lastInstantiateAllocBytes = instantiateAlloc;

            for (var f = 0; f < framesBetween; f++) yield return null;

            allocBefore = Profiler.GetTotalAllocatedMemoryLong();
            stopwatch.Restart();

            for (var i = 0; i < objectsPerIteration; i++)
                if (instances[i] != null)
                    Destroy(instances[i]);

            stopwatch.Stop();
            allocAfter = Profiler.GetTotalAllocatedMemoryLong();

            var destroyMs = stopwatch.Elapsed.TotalMilliseconds;
            var destroyAlloc = allocAfter - allocBefore;

            totalDestroyTime += destroyMs;
            totalDestroyAlloc += destroyAlloc;
            lastDestroyTimeMs = (float)destroyMs;
            lastDestroyAllocBytes = destroyAlloc;

            completedIterations++;

            Debug.Log(
                $"[{iter + 1}/{iterations}] Create: {instantiateMs:F2}ms ({FormatBytes(instantiateAlloc)}) | Destroy: {destroyMs:F2}ms ({FormatBytes(destroyAlloc)})");

            yield return null;
        }

        totalStopwatch.Stop();
        totalTestTimeMs = (float)totalStopwatch.Elapsed.TotalMilliseconds;
        gcCollectionsDuringTest = System.GC.CollectionCount(0) - gcCountBefore;

        avgInstantiateTimeMs = (float)(totalInstantiateTime / iterations);
        avgDestroyTimeMs = (float)(totalDestroyTime / iterations);
        avgInstantiateAllocBytes = totalInstantiateAlloc / iterations;
        avgDestroyAllocBytes = totalDestroyAlloc / iterations;

        var avgTimePerObject = avgInstantiateTimeMs / objectsPerIteration;
        var avgDestroyTimePerObject = avgDestroyTimeMs / objectsPerIteration;
        var avgAllocPerObject = avgInstantiateAllocBytes / objectsPerIteration;

        BufferUtils.LogGrowStats();
        UniTextPoolStats.LogAll();

        Debug.Log($"═══════════════════════════════════════════════════════════════");
        Debug.Log($"  Test Complete!");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"  Total time: {totalTestTimeMs:F2}ms");
        Debug.Log($"  GC collections during test: {gcCollectionsDuringTest}");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"  INSTANTIATE:");
        Debug.Log($"    Avg time: {avgInstantiateTimeMs:F2}ms ({avgTimePerObject:F4}ms per object)");
        Debug.Log(
            $"    Avg alloc: {FormatBytes(avgInstantiateAllocBytes)} ({FormatBytes(avgAllocPerObject)} per object)");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"  DESTROY:");
        Debug.Log($"    Avg time: {avgDestroyTimeMs:F2}ms ({avgDestroyTimePerObject:F4}ms per object)");
        Debug.Log($"    Avg alloc: {FormatBytes(avgDestroyAllocBytes)}");
        Debug.Log($"───────────────────────────────────────────────────────────────");
        Debug.Log($"  Total objects: {objectsPerIteration * iterations}");
        Debug.Log($"  Total alloc: {FormatBytes(totalInstantiateAlloc + totalDestroyAlloc)}");
        Debug.Log($"═══════════════════════════════════════════════════════════════");

        instances = null;
        isRunning = false;
    }

    private static string FormatBytes(long bytes)
    {
        if (bytes < 0) return $"-{FormatBytes(-bytes)}";
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024f:F1} KB";
        return $"{bytes / (1024f * 1024f):F2} MB";
    }

    private void CleanupInstances()
    {
        if (instances == null) return;

        for (var i = 0; i < instances.Length; i++)
            if (instances[i] != null)
                Destroy(instances[i]);

        instances = null;
    }

    private void OnDisable()
    {
        StopTest();
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("UniText/Run Performance Test")]
    private static void RunFromMenu()
    {
        var test = FindFirstObjectByType<UniTextPerformanceTest>();
        if (test != null)
            test.RunTest();
        else
            Debug.LogError("No UniTextPerformanceTest found in scene. Add the component to a GameObject first.");
    }
#endif
}