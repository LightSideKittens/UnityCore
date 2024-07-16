using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;

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
        
        string selectionPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        Fill(AssetDatabaseUtils.GetUsed, fullUsedDirect);
        Fill(AssetDatabaseUtils.GetUsedIndirect, fullUsedIndirect);
        
        Fill(AssetDatabaseUtils.GetUses, fullUsesDirect);
        Fill(AssetDatabaseUtils.GetUsesIndirect, fullUsesIndirect);

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
    }

    private void FilterAssetsUsages()
    {
        FilterAssets(fullUsedDirect, usedDirect);
        FilterAssets(fullUsedIndirect, usedIndirect);
        
        FilterAssets(fullUsesDirect, usesDirect);
        FilterAssets(fullUsesIndirect, usesIndirect);
    }
}

