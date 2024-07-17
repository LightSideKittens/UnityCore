using System.Collections.Generic;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class GameObjectUtils
{
    private static Dictionary<Object, DependenciesData> graph = new();
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
        AddObjectsForScene(SceneManager.GetActiveScene(), default);
        EditorSceneManager.sceneOpened += AddObjectsForScene;
        EditorSceneManager.sceneClosed += RemoveObjectsForScene;
    }

    private static List<Object> GetDependencies(Object comp)
    {
        List<Object> dependencies = new List<Object>();
        if (comp is GameObject go)
        {
            dependencies.AddRange(go.GetComponents<Component>());
            return dependencies;
        }
        
        SerializedObject serializedObject = new SerializedObject(comp);
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

    private static List<GameObject> allObjects = new();

    private static void AddObjectsForScene(Scene scene, OpenSceneMode _) => HandleObjectsForScene(scene, OnImported);
    private static void RemoveObjectsForScene(Scene scene) => HandleObjectsForScene(scene, OnDeleted);

    private static void HandleObjectsForScene(Scene scene, System.Action<GameObject> action)
    {
        if (scene.isLoaded)
        {
            GameObject[] sceneObjects = scene.GetRootGameObjects();
            foreach (GameObject rootObject in sceneObjects)
            {
                GetAllChildren(rootObject, allObjects, action);
            }
        }
    }
    
    private static void GetAllChildren(GameObject parent, List<GameObject> objects, System.Action<GameObject> action)
    {
        objects.Add(parent);
        action(parent);
        
        foreach (Transform child in parent.transform)
        {
            GetAllChildren(child.gameObject, objects, action);
        }
    }
    
    private static void OnImported(GameObject obj)
    {
        if (!graph.ContainsKey(obj))
        {
            graph[obj] = new DependenciesData();
        }

        FillDeps(obj);
        
        foreach (var comp in obj.GetComponents<Component>())
        {
            FillDeps(comp);
        }

        void FillDeps(Object target)
        {
            var dependencies = GetDependencies(target);

            foreach (var dependency in dependencies)
            {
                if (!graph.ContainsKey(dependency))
                {
                    graph[dependency] = new DependenciesData();
                }

                graph[target].uses.Add(dependency);

                graph[dependency].usedBy.Add(target);
            }
        }
    }

    private static void OnDeleted(GameObject obj)
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
    
    [System.Serializable]
    private class DependenciesData
    {
        public HashSet<Object> usedBy = new();
        public HashSet<Object> uses = new();
    }
}