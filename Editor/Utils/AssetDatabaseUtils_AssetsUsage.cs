using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static partial class AssetDatabaseUtils
{
    private const string GraphFilePath = "Assets/AssetsUsages.json";
    private static Dictionary<string, DependenciesData> graph = new();

    static AssetDatabaseUtils()
    {
        LSAssetPostprocessor.Imported += OnImported;
        LSAssetPostprocessor.Deleted += OnDeleted;
        LoadGraphData();
    }

    public static void GetDependencies(string assetPath, HashSet<string> result, bool indirect = false, bool used = false,
        bool includeDirect = false)
    {
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        GetDependenciesByGuid(assetGuid, result, indirect, used, includeDirect);
        var paths = new List<string>();
        
        foreach (var guid in result)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path))
            {
                RemoveAssetFromGraphByGuid(guid);
                continue;
            }
            paths.Add(path);
        }

        result.Clear();
        result.AddRange(paths);
    }

    public static void GetDependenciesByGuid(string assetGuid, HashSet<string> result,
        bool indirect = false,
        bool used = false, bool includeDirect = false)
    {
        GetDependenciesByGuid(new HashSet<string>(), assetGuid, result, indirect, used, includeDirect);
        result.Remove(assetGuid);
    }

    private static void GetDependenciesByGuid(HashSet<string> set, string assetGuid, HashSet<string> result, bool indirect = false,
        bool used = false, bool includeDirect = false)
    {
        if (!set.Add(assetGuid)) return;
        
        HashSet<string> dependencies = null;

        if (graph.TryGetValue(assetGuid, out var data))
        {
            dependencies = used ? data.usedBy : data.uses;
        }

        if(dependencies == null || dependencies.Count == 0) return;
        
        if (indirect)
        {
            foreach (var dependency in dependencies)
            {
                GetDependenciesByGuid(set, dependency, result, true, used, true);
            }
        }

        if (includeDirect || !indirect)
        {
            result.AddRange(dependencies);
        }
    }
    
    public static void GetUses(string assetPath, HashSet<string> result) => GetDependencies(assetPath, result);
    public static void GetUsesByGuid(string assetGuid, HashSet<string> result) => GetDependenciesByGuid(assetGuid, result);
    public static void GetUsed(string assetPath, HashSet<string> result) => GetDependencies(assetPath, result, used: true);
    public static void GetUsedByGuid(string assetGuid, HashSet<string> result) => GetDependenciesByGuid(assetGuid, result, used: true);
    
    
    public static void GetUsesIndirect(string assetPath, HashSet<string> result, bool includeDirect) => GetDependencies(assetPath, result, indirect: true, includeDirect: includeDirect);
    public static void GetUsesIndirectByGuid(string assetGuid, HashSet<string> result, bool includeDirect) => GetDependenciesByGuid(assetGuid, result, indirect: true, includeDirect: includeDirect);
    
    public static void GetUsedIndirect(string assetPath, HashSet<string> result, bool includeDirect) => GetDependencies(assetPath, result, indirect: true, used: true, includeDirect: includeDirect);
    public static void GetUsedIndirectByGuid(string assetGuid, HashSet<string> result, bool includeDirect) => GetDependenciesByGuid(assetGuid, result, indirect: true, used: true, includeDirect: includeDirect);
    
    
    public static void GetUsesIndirect(string assetPath, HashSet<string> result) => GetDependencies(assetPath, result, indirect: true, includeDirect: false);
    public static void GetUsesIndirectByGuid(string assetGuid, HashSet<string> result) => GetDependenciesByGuid(assetGuid, result, indirect: true, includeDirect: false);
    
    public static void GetUsedIndirect(string assetPath, HashSet<string> result) => GetDependencies(assetPath, result, indirect: true, used: true, includeDirect: false);
    public static void GetUsedIndirectByGuid(string assetGuid, HashSet<string> result) => GetDependenciesByGuid(assetGuid, result, indirect: true, used: true, includeDirect: false);


    private static void GenerateAssetDependencyGraph()
    {
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        graph.Clear();

        foreach (string assetPath in allAssetPaths)
        {
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!graph.ContainsKey(assetGuid))
            {
                graph[assetGuid] = new DependenciesData();
            }

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

            foreach (string dependencyPath in dependencies)
            {
                string dependencyGuid = AssetDatabase.AssetPathToGUID(dependencyPath);
                if (!graph.ContainsKey(dependencyGuid))
                {
                    graph[dependencyGuid] = new DependenciesData();
                }

                graph[assetGuid].uses.Add(dependencyGuid);
                graph[dependencyGuid].usedBy.Add(assetGuid);
            }
        }

        SaveGraphData();
    }

    private static void OnImported(string[] assetPaths)
    {
        for (int i = 0; i < assetPaths.Length; i++)
        {
            UpdateGraphForAsset(assetPaths[i]);
        }
    }

    private static void OnDeleted(string[] assetPaths)
    {
        for (int i = 0; i < assetPaths.Length; i++)
        {
            if (assetPaths[i] == GraphFilePath)
            {
                GenerateAssetDependencyGraph();
                return;
            }
            
            RemoveAssetFromGraph(assetPaths[i]);
        }
    }

    private static void UpdateGraphForAsset(string assetPath)
    {
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

        if (!graph.ContainsKey(assetGuid))
        {
            graph[assetGuid] = new DependenciesData();
        }

        graph[assetGuid].uses.Clear();

        foreach (string dependencyPath in dependencies)
        {
            string dependencyGuid = AssetDatabase.AssetPathToGUID(dependencyPath);
            if (!graph.ContainsKey(dependencyGuid))
            {
                graph[dependencyGuid] = new DependenciesData();
            }

            graph[assetGuid].uses.Add(dependencyGuid);
            graph[dependencyGuid].usedBy.Add(assetGuid);
        }

        SetGraphDirty();
    }

    private static void RemoveAssetFromGraph(string assetPath)
    {
        if(string.IsNullOrEmpty(assetPath)) return;
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        RemoveAssetFromGraphByGuid(assetGuid);
    }

    private static void RemoveAssetFromGraphByGuid(string guid)
    {
        var data = graph[guid];
        
        foreach (var dependencyGuid in data.uses)
        {
            graph[dependencyGuid].usedBy.Remove(guid);
        }
            
        foreach (var dependencyGuid in data.usedBy)
        {
            graph[dependencyGuid].uses.Remove(guid);
        }

        graph.Remove(guid);
        SetGraphDirty();
    }

    private static void SetGraphDirty()
    {
        EditorApplication.update -= SaveGraphData;
        EditorApplication.update += SaveGraphData;
    }

    private static void SaveGraphData()
    {
        EditorApplication.update -= SaveGraphData;
        string json = JsonConvert.SerializeObject(graph, Formatting.None);
        File.WriteAllText(GraphFilePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Asset usages saved at: {GraphFilePath}");
    }

    private static void LoadGraphData()
    {
        if (File.Exists(GraphFilePath))
        {
            string json = File.ReadAllText(GraphFilePath);
            graph = JsonConvert.DeserializeObject<Dictionary<string, DependenciesData>>(json);
        }
        else
        {
            GenerateAssetDependencyGraph();
        }
    }
    
    [System.Serializable]
    private class DependenciesData
    {
        public HashSet<string> usedBy = new();
        public HashSet<string> uses = new();
    }
}
