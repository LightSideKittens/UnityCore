using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

public partial class AssetsViewer
{
    [TabGroup("Tabs", "Asset References")]
    public List<Object> AssetReferences = new();
    
    protected override void OnDisable()
    {
        base.OnDisable();
        Selection.selectionChanged -= UpdateReferences;
    }

    private void UpdateReferences()
    {
        var selection = Selection.activeObject;
        if (selection == null)
        {
            AssetReferences.Clear();
            return;
        }

        string selectedAssetPath = AssetDatabase.GetAssetPath(selection);
        string[] allAssetPaths = AssetDatabase.GetAllAssetPaths();
        List<Object> references = new List<Object>();

        foreach (string assetPath in allAssetPaths)
        {
            if (AssetDatabase.IsValidFolder(assetPath)) continue;

            string[] dependencies = AssetDatabase.GetDependencies(assetPath, true);
            foreach (string dependency in dependencies)
            {
                if (dependency == selectedAssetPath)
                {
                    var obj = AssetDatabase.LoadMainAssetAtPath(dependency);
                    references.Add(obj);
                    break;
                }
            }
        }

        AssetReferences = references;
    }
}