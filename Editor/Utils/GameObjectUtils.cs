using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public static class GameObjectUtils
{
    private static Dictionary<Object, DependenciesData> graph;
    private static Dictionary<Object, DependenciesData> sceneGraph = new();
    private static Dictionary<Object, DependenciesData> prefabGraph = new();
    public static event Action GraphUpdated;
    private static bool isGraphUpdated;
    
    public static void GetDependencies(Object obj, HashSet<Object> result,
        bool indirect = false,
        bool used = false, bool includeDirect = false)
    {
        var set = new HashSet<Object>();
        if (obj is GameObject go)
        {
            foreach (var comp in go.GetComponents<Component>())
            {
                GetDependencies(set, comp, result, indirect, used, includeDirect);
            }
        }
        else
        {
            GetDependencies(set, obj, result, indirect, used, includeDirect);
        }
        
    }

    private static void GetDependencies(HashSet<Object> set, Object obj, HashSet<Object> result, bool indirect = false,
        bool used = false, bool includeDirect = false)
    {
        if (!set.Add(obj)) return;
        if (!graph.ContainsKey(obj)) return;
        
        var dependencies = used
            ? graph[obj].usedBy
            : graph[obj].uses;

        if(dependencies.Count == 0) return;
        
        if (indirect)
        {
            foreach (var dependency in dependencies)
            {
                GetDependencies(set, dependency, result, true, used, true);
            }
        }

        if (includeDirect || !indirect)
        {
            result.AddRange(dependencies);
        }
    }
    
    public static void GetUses(Object obj, HashSet<Object> result) => GetDependencies(obj, result);
    public static void GetUsed(Object obj, HashSet<Object> result) => GetDependencies(obj, result, used: true);
    
    public static void GetUsesIndirect(Object obj, HashSet<Object> result, bool includeDirect) => GetDependencies(obj, result, indirect: true, includeDirect: includeDirect);
    public static void GetUsedIndirect(Object obj, HashSet<Object> result, bool includeDirect) => GetDependencies(obj, result, indirect: true, used: true, includeDirect: includeDirect);
    
    public static void GetUsesIndirect(Object obj, HashSet<Object> result) => GetDependencies(obj, result, indirect: true, includeDirect: false);
    public static void GetUsedIndirect(Object obj, HashSet<Object> result) => GetDependencies(obj, result, indirect: true, used: true, includeDirect: false);

    static GameObjectUtils()
    {
        OnSceneLoaded(SceneManager.GetActiveScene(), default);
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            OnPrefabStageOpened(stage);
        }

        PrefabStage.prefabStageOpened += OnPrefabStageOpened;
        PrefabStage.prefabStageClosing += OnPrefabStageClosed;

        EditorSceneManager.sceneOpened += OnSceneLoaded;
        EditorSceneManager.sceneClosed += OnSceneUnloaded;
        
        LSEditorUtility.DirtySet += OnSetDirty;
    }

    
    private static void SetGraphUpdated()
    {
        EditorApplication.update -= OnEditorUpdate;
        EditorApplication.update += OnEditorUpdate;
    }
    
    private static void OnEditorUpdate()
    {
        EditorApplication.update -= OnEditorUpdate;
        GraphUpdated?.Invoke();
    }

    private static void OnSetDirty(Object obj)
    {
        if (!AssetDatabase.Contains(obj))
        {
            var go = obj as GameObject;
            if (go is null)
            {
                if (obj is Component comp)
                {
                    go = comp.gameObject;
                }
                else
                {
                    return;
                }
            }
            
            OnDeleted(go);
            OnImported(go);
        }
    }

    private static List<Object> GetDependenciesForGameObject(Object go)
    {
        List<Object> dependencies = new List<Object>();
        dependencies.AddRange(((GameObject)go).GetComponents<Component>());
        return dependencies;
    }
    
    
    private static List<Object> GetDependenciesForAsset(Object asset)
    {
        var set = new HashSet<string>();
        AssetDatabaseUtils.GetUsesIndirect(AssetDatabase.GetAssetPath(asset), set, true);
        return set.Select(AssetDatabase.LoadMainAssetAtPath).ToList();
    }

    private static List<Object> GetDependenciesForComponent(Object obj)
    {
        List<Object> dependencies = new List<Object>();
        
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty property = serializedObject.GetIterator();

        while (property.NextVisible(true))
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object referencedObject = property.objectReferenceValue;
                if (referencedObject != null && !dependencies.Contains(referencedObject))
                {
                    dependencies.Add(referencedObject);
                }
            }
        }
        
        return dependencies;
    }

    private static void OnPrefabStageOpened(PrefabStage stage)
    {
        graph = prefabGraph;
        GetAllChildren(stage.prefabContentsRoot, OnImported);
    }

    private static void OnPrefabStageClosed(PrefabStage stage)
    {
        graph = sceneGraph;
        GetAllChildren(stage.prefabContentsRoot, OnDeleted);
    }
    
    private static void OnSceneLoaded(Scene scene, OpenSceneMode _)
    {
        graph = sceneGraph;
        HandleObjectsForScene(scene, OnImported);
    }

    private static void OnSceneUnloaded(Scene scene) => HandleObjectsForScene(scene, OnDeleted);

    private static void HandleObjectsForScene(Scene scene, Action<GameObject> action)
    {
        if (scene.isLoaded)
        {
            GameObject[] sceneObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in sceneObjects)
            {
                GetAllChildren(rootObject, action);
            }
        }
    }
    
    private static void GetAllChildren(GameObject parent, Action<GameObject> action)
    {
        action(parent);
        
        foreach (Transform child in parent.transform)
        {
            GetAllChildren(child.gameObject, action);
        }
    }
    
    private static void OnImported(GameObject obj)
    {
        if (!graph.ContainsKey(obj))
        {
            graph[obj] = new DependenciesData();
        }

        FillDeps(obj, GetDependenciesForGameObject);

        var objs = obj.GetComponents<Component>();

        for (int i = 0; i < objs.Length; i++)
        {
            var deps = FillDeps(objs[i], GetDependenciesForComponent);

            for (int j = 0; j < deps.Count; j++)
            {
                var dep = deps[j];
                if (AssetDatabase.Contains(dep))
                {
                    FillDeps(dep, GetDependenciesForAsset);
                }
            }
        }

        SetGraphUpdated();
        return;

        List<Object> FillDeps(Object target, Func<Object, List<Object>> getDependencies)
        {
            var dependencies = getDependencies(target);

            foreach (var dependency in dependencies)
            {
                if (!graph.ContainsKey(dependency))
                {
                    graph[dependency] = new DependenciesData();
                }

                graph[target].uses.Add(dependency);

                graph[dependency].usedBy.Add(target);
            }

            return dependencies;
        }
    }

    private static void OnDeleted(GameObject target)
    {
        Delete(target);

        foreach (var comp in target.GetComponents<Component>())
        {
            Delete(comp);
        }

        SetGraphUpdated();
        return;

        void Delete(Object obj)
        {
            if (graph.ContainsKey(obj))
            {
                foreach (var dependencyGuid in graph[obj].uses)
                {
                    graph[dependencyGuid].usedBy.Remove(obj);
                }

                graph.Remove(obj);
            }

            foreach (var assetData in graph.Values)
            {
                assetData.uses.Remove(obj);
            }
        }
    }
    
    [System.Serializable]
    private class DependenciesData
    {
        public HashSet<Object> usedBy = new();
        public HashSet<Object> uses = new();
    }
}