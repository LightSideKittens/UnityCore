using System.Collections.Generic;
using System.IO;
using UnityEditor;
using MenuItem = UnityEditor.MenuItem;
using UnityEngine;

public class MenuItemTools : MonoBehaviour
{
    [MenuItem("Assets/Force Save")]
    private static void ForceSaveAssets()
    {
        var objs = Selection.objects;

        for (int i = 0; i < objs.Length; i++)
        {
            var obj = objs[i];
            
            if (obj.IsFolder(out var path))
            {
                ForceSaveAssets(AssetDatabaseUtils.LoadAllAssets<Object>(path));
            }
            else
            {
                obj.ForceSave();
            }
        }
    }

    private static void ForceSaveAssets(IEnumerable<Object> objs)
    {
        foreach (var obj in objs)
        {
            obj.ForceSave();
        }
    }
    
    [MenuItem(LSConsts.Path.MenuItem.Tools + "/Clear Cache")]
    private static void ClearCache()
    {
        if (Caching.ClearCache())
        {
            Debug.Log("Addressables Cache cleared successfully!");
        }
        else
        {
            Debug.LogError("Failed to clear Addressables Cache!");
        }
    }

    [MenuItem(LSConsts.Path.MenuItem.Tools + "/Anchors to Corners %[")]
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
        }
    }
}