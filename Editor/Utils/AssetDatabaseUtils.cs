using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static partial class AssetDatabaseUtils
{
    public static void ForceSave(this Object target)
    {
        EditorUtility.SetDirty(target);
        AssetDatabase.SaveAssetIfDirty(target);
    }
    
    public static bool IsFolder(this Object obj) => obj.IsFolder(out _);

    public static bool IsFolder(this Object obj, out string path)
    {
        path = AssetDatabase.GetAssetPath(obj);
        return AssetDatabase.IsValidFolder(path);
    }

    public static string GetFolderPath(this Object target)
    {
        return Path.GetDirectoryName(AssetDatabase.GetAssetPath(target));
    }
    
    public static string GetFolderName(this Object target)
    {
        return Path.GetFileName(Path.GetDirectoryName(AssetDatabase.GetAssetPath(target)));
    }

    public static string RenameFolder(string folderPath, string newFolderName)
    {
        return AssetDatabase.MoveAsset(folderPath, folderPath.Replace(Path.GetFileName(folderPath), newFolderName));
    }

    public static string[] GetPaths<T>(string filter = "", params string[] paths) where T : Object
    {
        return GetPaths(typeof(T), filter, paths);
    }

    public static string[] GetPaths(Type type, string filter = "", params string[] paths)
    {
        var guids = GetGUIDs(type, filter, paths);
        var result = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            result[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
        }

        return result;
    }
    
    public static string[] GetGUIDs<T>(string filter = "", params string[] paths) where T : Object
    {
        return GetGUIDs(typeof(T), filter, paths);
    }
    
    public static string[] GetGUIDs(Type type, string filter = "", params string[] paths)
    {
        return AssetDatabase.FindAssets($"t:{type.Name} {filter}", paths);
    }
    
    public static List<T> LoadAllAssets<T>(string filter = "", params string[] paths) where T : Object
    {
        return LoadAllAssets(typeof(T), filter, paths).Cast<T>().ToList();
    }
    
    public static List<GameObject> LoadAllGameObjects(Type type, string filter = "", params string[] paths)
    {
        return LoadAllAssets(typeof(GameObject), filter, paths).Cast<GameObject>().Where(x => x.TryGetComponent(type, out _)).ToList();
    }
    
    public static List<T> LoadAllGameObjects<T>(string filter = "", params string[] paths) where T : Object
    {
        return LoadAllAssets(typeof(GameObject), filter, paths).Cast<GameObject>().Select(x => x.GetComponent<T>()).ToList();
    }
    
    public static List<Object> LoadAllAssets(Type type, string filter = "", params string[] paths)
    {
        var list = new List<Object>();
        var allPaths = GetPaths(type, filter, paths);

        for (int i = 0; i < allPaths.Length; i++)
        {
            var asset = AssetDatabase.LoadAssetAtPath(allPaths[i], type);
            list.Add(asset);
        }

        return list;
    }

    public static T LoadAny<T>(string filter = "", params string[] paths) where T : Object
    {
        return (T)LoadAny(typeof(T), filter, paths);
    }
    
    public static Object LoadAny(Type type, string filter = "", params string[] paths)
    {
        var allPaths = GetPaths(type, filter, paths);
        Object asset = null;
        
        for (int i = 0; i < allPaths.Length; i++)
        {
            asset = AssetDatabase.LoadAssetAtPath(allPaths[i], type);
            break;
        }
        
        return asset;
    }
}
