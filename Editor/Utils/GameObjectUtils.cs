using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class GameObjectUtils
{
    private static Dictionary<Object, DependenciesData> graph;
    private static Dictionary<Object, DependenciesData> sceneGraph = new();
    private static Dictionary<Object, DependenciesData> prefabGraph = new();
    private static Dictionary<Scene, HashSet<Object>> objectsByScenes = new();
    private static Dictionary<PrefabStage, HashSet<Object>> objectsByPrefab = new();
    public static event Action GraphUpdated;
    private static bool isGraphUpdated;
    
    public static void GetDependencies(Object obj, HashSet<Object> result,
        bool indirect = false,
        bool used = false, bool includeDirect = false)
    {
        var set = new HashSet<Object>();
        if (obj is GameObject go)
        {
            go.GetComponents(comps);
            foreach (var comp in comps)
            {
                if(comp == null) continue;
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

        Undo.postprocessModifications += OnSetDirty;
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

    private static UndoPropertyModification[] OnSetDirty(UndoPropertyModification[] modifications)
    {
        for (int i = 0; i < modifications.Length; i++)
        {
            OnSetDirty(modifications[i]);
        }
        
        return modifications;
    }

    private static void OnSetDirty(UndoPropertyModification mod)
    {
        var obj = mod.currentValue.target;
        if (!AssetDatabase.Contains(obj))
        {
            HashSet<Object> deps = null;
            Object target = null;
            
            if (obj is GameObject go)
            {
                target = go;
                deps = GetDependenciesForGameObject(go);
            }
            else if(obj is Component comp)
            {
                target = comp;
                deps = GetDependenciesForComponent(comp);
            }

            if (target is not null && graph.ContainsKey(target)) //TODO: Remove "&& graph.ContainsKey(target)" and implement GameObject creating and deleting tracking 
            {
                var uses = graph[target].uses;
                graph[target].uses = deps;
                
                foreach (var entry in uses)
                {
                    graph[entry].usedBy.Remove(target);
                }
                
                foreach (var entry in deps)
                {
                    if (!graph.ContainsKey(entry))
                    {
                        graph[entry] = new DependenciesData();
                    }
                    
                    graph[entry].usedBy.Add(target);
                }
            }
        }
    }

    private static HashSet<Object> GetDependenciesForGameObject(Object go)
    {
        var dependencies = new HashSet<Object>();
        ((GameObject)go).GetComponents(comps);
        for (int i = 0; i < comps.Count; i++)
        {
            var comp = comps[i];
            if (comp != null)
            {
                dependencies.Add(comp);
            }
        }
        return dependencies;
    }
    
    
    private static HashSet<Object> GetDependenciesForAsset(Object asset)
    {
        var set = new HashSet<string>();
        AssetDatabaseUtils.GetUsesIndirect(AssetDatabase.GetAssetPath(asset), set, true);
        return set.Select(AssetDatabase.LoadMainAssetAtPath).ToHashSet();
    }
    

    private static HashSet<Object> GetDependenciesForComponent(Object obj)
    {
        var dependencies = new HashSet<Object>();
        
        SerializedObject serializedObject = new SerializedObject(obj);
        SerializedProperty property = serializedObject.GetIterator();

        while (property.NextVisible(true))
        {
            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                Object referencedObject = property.objectReferenceValue;
                if (referencedObject != null)
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
        objects = new HashSet<Object>();
        objectsByPrefab[stage] = objects;
        GetAllChildren(stage.prefabContentsRoot, OnImported);
    }

    private static void OnPrefabStageClosed(PrefabStage stage)
    {
        if (!objectsByPrefab.TryGetValue(stage, out var objs)) return;
        
        foreach (var obj in objs)
        {
            OnDeleted(obj);
        }

        objectsByPrefab.Remove(stage);
        
        if (PrefabStageUtility.GetCurrentPrefabStage() == null)
        {
            graph = sceneGraph;
        }
    }
    
    private static HashSet<Object> objects;

    private static void OnSceneLoaded(Scene scene, OpenSceneMode _)
    {
        graph = sceneGraph;
        objects = new HashSet<Object>();
        objectsByScenes[scene] = objects;
        HandleObjectsForScene(scene, OnImported);
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        if (!objectsByScenes.TryGetValue(scene, out var objs)) return;
        
        foreach (var obj in objs)
        {
            OnDeleted(obj);
        }

        objectsByScenes.Remove(scene);
    }
    
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
    
    private static List<Component> comps = new();
    
    private static void OnImported(GameObject obj)
    {
        if (!graph.ContainsKey(obj))
        {
            graph[obj] = new DependenciesData();
        }

        FillDeps(obj, GetDependenciesForGameObject);
        
        obj.GetComponents(comps);

        for (int i = 0; i < comps.Count; i++)
        {
            var comp = comps[i];
            if (comp == null) continue;
            
            var compDeps = FillDeps(comps[i], GetDependenciesForComponent);

            foreach (var dep in compDeps)
            {
                if (AssetDatabase.Contains(dep))
                {
                    FillDeps(dep, GetDependenciesForAsset);
                }
            }
        }

        SetGraphUpdated();
        return;

        HashSet<Object> FillDeps(Object target, Func<Object, HashSet<Object>> getDependencies)
        {
            objects.Add(target);
            var dependencies = getDependencies(target);

            foreach (var dependency in dependencies)
            {
                objects.Add(dependency);
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

    private static void OnDeleted(Object target)
    {
        if (graph.TryGetValue(target, out var data))
        {
            foreach (var dep in data.uses)
            {
                graph[dep].usedBy.Remove(target);
            }
            
            foreach (var dep in data.usedBy)
            {
                graph[dep].uses.Remove(target);
            }

            graph.Remove(target);
        }
    }
    
    
    [System.Serializable]
    private class DependenciesData
    {
        public HashSet<Object> usedBy = new();
        public HashSet<Object> uses = new();
    }
}