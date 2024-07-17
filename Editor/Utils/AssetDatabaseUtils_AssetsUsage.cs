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
        var newSet = result.Select(AssetDatabase.GUIDToAssetPath).ToList();
        result.Clear();
        result.AddRange(newSet);
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
        
        var dependencies = used
            ? graph[assetGuid].usedBy
            : graph[assetGuid].uses;

        if(dependencies.Count == 0) return;
        
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

        SaveGraphData();
    }

    private static void OnDeleted(string[] assetPaths)
    {
        for (int i = 0; i < assetPaths.Length; i++)
        {
            RemoveAssetFromGraph(assetPaths[i]);
        }

        SaveGraphData();
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
    }

    private static void RemoveAssetFromGraph(string assetPath)
    {
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        if (graph.ContainsKey(assetGuid))
        {
            foreach (var dependencyGuid in graph[assetGuid].uses)
            {
                graph[dependencyGuid].usedBy.Remove(assetGuid);
            }

            graph.Remove(assetGuid);
        }

        foreach (var assetData in graph.Values)
        {
            assetData.uses.Remove(assetGuid);
        }
    }

    private static void SaveGraphData()
    {
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
