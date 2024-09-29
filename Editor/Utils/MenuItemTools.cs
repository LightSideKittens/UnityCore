using System.Collections.Generic;
using UnityEditor;
using MenuItem = UnityEditor.MenuItem;
using UnityEngine;

public class MenuItemTools : MonoBehaviour
{
    [MenuItem("Assets/Force Save")]
    private static void ForceSaveAssets()
    {
        var objs = Selection.objects;

        foreach (var obj in objs)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (AssetDatabase.IsValidFolder(path))
            {
                ForceSaveAssetsInFolder(path);
            }
            else
            {
                obj.ForceSave();
            }
        }
    }

    private static void ForceSaveAssetsInFolder(string folderPath)
    {
        var assetPaths = AssetDatabase.FindAssets("", new[] { folderPath });

        foreach (var assetGUID in assetPaths)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGUID);
            var obj = AssetDatabase.LoadMainAssetAtPath(assetPath);

            if (obj != null)
            {
                obj.ForceSave();
            }
        }
    }
    
    [MenuItem(LSPaths.MenuItem.Tools + "/Clear Cache")]
    private static void ClearCache()
    {
        if (Caching.ClearCache())
        {
            Debug.Log("Cache cleared successfully!");
        }
        else
        {
            Debug.LogError("Failed to clear Cache!");
        }
    }

    [MenuItem(LSPaths.MenuItem.Tools + "/Anchors to Corners %[")]
    private static void AnchorsToCorners()
    {
        var rects = Selection.transforms;

        for (var i = 0; i < rects.Length; i++)
        {
            var t = rects[i] as RectTransform;
            var pt = t.parent as RectTransform;

            if (t == null || pt == null) return;

            var rect = pt.rect;
            var newAnchorsMin = new Vector2(t.anchorMin.x + t.offsetMin.x / rect.width,
                t.anchorMin.y + t.offsetMin.y / rect.height);
            var newAnchorsMax = new Vector2(t.anchorMax.x + t.offsetMax.x / rect.width,
                t.anchorMax.y + t.offsetMax.y / rect.height);

            t.anchorMin = newAnchorsMin;
            t.anchorMax = newAnchorsMax;
            t.offsetMin = t.offsetMax = new Vector2(0, 0);
            EditorUtility.SetDirty(t);
        }
    }
}