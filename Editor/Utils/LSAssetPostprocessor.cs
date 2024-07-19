using System;
using UnityEditor;
using UnityEngine;

public class LSAssetPostprocessor : AssetPostprocessor
{
    public static event Action<string[]> Imported;
    public static event Action<string[]> Deleted;
    public static event Action<string[]> Moved;
    public static event Action<string[]> MovedFromAssetPaths;
    
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (importedAssets.Length > 0) Imported?.Invoke(importedAssets);
        if (deletedAssets.Length > 0) Deleted?.Invoke(deletedAssets);
        if (movedAssets.Length > 0) Moved?.Invoke(movedAssets);
        if (movedFromAssetPaths.Length > 0) MovedFromAssetPaths?.Invoke(movedFromAssetPaths);
    }
}