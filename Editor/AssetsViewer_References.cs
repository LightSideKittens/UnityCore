using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using Object = UnityEngine.Object;

internal  partial class AssetsViewer
{
    [TabGroup("Tabs", "Used")] [TableList(IsReadOnly = true)] public List<AssetInfo> usedDirect = new();
    [TabGroup("Tabs", "Used")] [TableList(IsReadOnly = true)] public List<AssetInfo> usedIndirect = new();
    
    [TabGroup("Tabs", "Uses")] [TableList(IsReadOnly = true)] public List<AssetInfo> usesDirect = new();
    [TabGroup("Tabs", "Uses")] [TableList(IsReadOnly = true)] public List<AssetInfo> usesIndirect = new();

    private readonly List<AssetInfo> fullUsedDirect = new();
    private readonly List<AssetInfo> fullUsedIndirect = new();
    
    private readonly List<AssetInfo> fullUsesDirect = new();
    private readonly List<AssetInfo> fullUsesIndirect = new();

    private void UpdateReferences()
    {
        Selection.selectionChanged -= UpdateReferences;
        Selection.selectionChanged += UpdateReferences;
        fullUsedDirect.Clear();
        fullUsedIndirect.Clear();
        
        fullUsesDirect.Clear();
        fullUsesIndirect.Clear();

        if (Selection.activeObject == null)
        {
            return;
        }

        var selectedObject = Selection.activeObject;
        string selectionPath = AssetDatabase.GetAssetPath(selectedObject);

        if (AssetDatabase.Contains(Selection.activeObject))
        {
            Fill(AssetDatabaseUtils.GetUsed, fullUsedDirect);
            Fill(AssetDatabaseUtils.GetUsedIndirect, fullUsedIndirect);
        
            Fill(AssetDatabaseUtils.GetUses, fullUsesDirect);
            Fill(AssetDatabaseUtils.GetUsesIndirect, fullUsesIndirect);
        }
        else
        {
            FillGo(GameObjectUtils.GetUsed, fullUsedDirect);
            FillGo(GameObjectUtils.GetUsedIndirect, fullUsedIndirect);
            FillGo(GameObjectUtils.GetUses, fullUsesDirect);
            FillGo(GameObjectUtils.GetUsesIndirect, fullUsesIndirect);
        }


        FilterAssetsUsages();

        return;
        void Fill(Action<string, HashSet<string>> action, List<AssetInfo> full)
        {
            HashSet<string> list = new HashSet<string>();
            action(selectionPath, list);
        
            foreach (string assetPath in list)
            {
                full.Add(BuildInfo(assetPath));
            }
        }
        
        void FillGo(Action<Object, HashSet<Object>> action, List<AssetInfo> full)
        {
            HashSet<Object> list = new HashSet<Object>();
            action(selectedObject, list);
        
            foreach (var obj in list)
            {
                full.Add(BuildInfo(obj));
            }
        }
    }

    private void FilterAssetsUsages()
    {
        FilterAssets(fullUsedDirect, usedDirect);
        FilterAssets(fullUsedIndirect, usedIndirect);
        
        FilterAssets(fullUsesDirect, usesDirect);
        FilterAssets(fullUsesIndirect, usesIndirect);
    }
}

