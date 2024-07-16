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
    private const string GraphFilePath = "Assets/AssetDependencyGraph.json";
    private static AssetDependencyGraphData dependencyGraphData = new AssetDependencyGraphData();

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
    }

    private static void GetDependenciesByGuid(HashSet<string> set, string assetGuid, HashSet<string> result, bool indirect = false,
        bool used = false, bool includeDirect = false)
    {
        if (!set.Add(assetGuid)) return;
        
        var dependencies = used
            ? dependencyGraphData.dependencies[assetGuid].usedBy
            : dependencyGraphData.dependencies[assetGuid].uses;

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
        dependencyGraphData.dependencies.Clear();

        foreach (string assetPath in allAssetPaths)
        {
            string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            if (!dependencyGraphData.dependencies.ContainsKey(assetGuid))
            {
                dependencyGraphData.dependencies[assetGuid] = new AssetDependencyData();
            }

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, false);

            foreach (string dependencyPath in dependencies)
            {
                string dependencyGuid = AssetDatabase.AssetPathToGUID(dependencyPath);
                if (!dependencyGraphData.dependencies.ContainsKey(dependencyGuid))
                {
                    dependencyGraphData.dependencies[dependencyGuid] = new AssetDependencyData();
                }

                dependencyGraphData.dependencies[assetGuid].uses.Add(dependencyGuid);

                dependencyGraphData.dependencies[dependencyGuid].usedBy.Add(assetGuid);
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

        if (!dependencyGraphData.dependencies.ContainsKey(assetGuid))
        {
            dependencyGraphData.dependencies[assetGuid] = new AssetDependencyData();
        }

        dependencyGraphData.dependencies[assetGuid].uses.Clear();

        foreach (string dependencyPath in dependencies)
        {
            string dependencyGuid = AssetDatabase.AssetPathToGUID(dependencyPath);
            if (!dependencyGraphData.dependencies.ContainsKey(dependencyGuid))
            {
                dependencyGraphData.dependencies[dependencyGuid] = new AssetDependencyData();
            }

            dependencyGraphData.dependencies[assetGuid].uses.Add(dependencyGuid);
            dependencyGraphData.dependencies[dependencyGuid].usedBy.Add(assetGuid);
        }
    }

    private static void RemoveAssetFromGraph(string assetPath)
    {
        string assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        if (dependencyGraphData.dependencies.ContainsKey(assetGuid))
        {
            foreach (var dependencyGuid in dependencyGraphData.dependencies[assetGuid].uses)
            {
                dependencyGraphData.dependencies[dependencyGuid].usedBy.Remove(assetGuid);
            }

            dependencyGraphData.dependencies.Remove(assetGuid);
        }

        foreach (var assetData in dependencyGraphData.dependencies.Values)
        {
            assetData.uses.Remove(assetGuid);
        }
    }

    private static void SaveGraphData()
    {
        string json = JsonConvert.SerializeObject(dependencyGraphData, Formatting.Indented);
        File.WriteAllText(GraphFilePath, json);
        AssetDatabase.Refresh();
        Debug.Log($"Asset dependency graph saved at: {GraphFilePath}");
    }

    private static void LoadGraphData()
    {
        if (File.Exists(GraphFilePath))
        {
            string json = File.ReadAllText(GraphFilePath);
            dependencyGraphData = JsonConvert.DeserializeObject<AssetDependencyGraphData>(json);
        }
        else
        {
            GenerateAssetDependencyGraph();
        }
    }
}

[System.Serializable]
public class AssetDependencyData
{
    public HashSet<string> usedBy = new();
    public HashSet<string> uses = new();
}

[System.Serializable]
public class AssetDependencyGraphData
{
    public Dictionary<string, AssetDependencyData> dependencies = new();
}
