using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

public static class AssetDatabaseUtils
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
    
    public static string[] FindAssets<T>(params string[] path) where T : Object
    {
        return AssetDatabase.FindAssets("t:" + typeof(T).Name, path);
    }
    
    public static List<T> LoadAllAssets<T>(string filter = "", params string[] paths) where T : Object
    {
        return LoadAllAssets(typeof(T), filter, paths).Cast<T>().ToList();
    }
    
    public static List<Object> LoadAllAssets(Type type, string filter = "", params string[] paths)
    {
        var list = new List<Object>();
        var assetType = type.Name;
        var guids = AssetDatabase.FindAssets($"t:{assetType} {filter}", paths);

        for (int i = 0; i < guids.Length; i++)
        {
            var guid = guids[i];
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath(assetPath, type);
            list.Add(asset);
        }

        return list;
    }

    public static T LoadAny<T>(string filter = "", params string[] paths) where T : Object
    {
        return LoadAllAssets<T>(filter, paths).FirstOrDefault();
    }
}
