using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

internal partial class AssetsViewer
{
    [TabGroup("Tabs", "Uses")]
    [TabGroup("Tabs", "Used")]
    [ValueDropdown("selector")]
    [ShowIf("ShowSelector")]
    [OnValueChanged("OnInspectedObjectChanged")]
    public Object inspectedObject;

    private readonly ValueDropdownList<Object> selector = new();
    private bool ShowSelector => selector.Count > 0;
    
    private void OnInspectedObjectChanged()
    {
        ClearAllUsages();
        FillGo(GameObjectUtils.GetUsed, fullUsedDirect);
        FillGo(GameObjectUtils.GetUsedIndirect, fullUsedIndirect);
        FillGo(GameObjectUtils.GetUses, fullUsesDirect);
        FillGo(GameObjectUtils.GetUsesIndirect, fullUsesIndirect);
        FilterAssetsUsages();
    }
    
    
    [TabGroup("Tabs", "Used")] 
    [TableList(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 10)] 
    public List<AssetInfo> usedDirect = new();
    
    [TabGroup("Tabs", "Used")] 
    [TableList(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 10)] 
    public List<AssetInfo> usedIndirect = new();
    
    [TabGroup("Tabs", "Uses")] 
    [TableList(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 10)] 
    public List<AssetInfo> usesDirect = new();
    
    [TabGroup("Tabs", "Uses")] 
    [TableList(IsReadOnly = true, ShowPaging = true, NumberOfItemsPerPage = 10)] 
    public List<AssetInfo> usesIndirect = new();

    private readonly List<AssetInfo> fullUsedDirect = new();
    private readonly List<AssetInfo> fullUsedIndirect = new();
    
    private readonly List<AssetInfo> fullUsesDirect = new();
    private readonly List<AssetInfo> fullUsesIndirect = new();
    
    private void UpdateReferences()
    {
        ClearAllUsages();   
        selector.Clear();

        if (Selection.activeObject == null)
        {
            selectedObject = null;
            return;
        }

        selectedObject = Selection.activeObject;
        string selectionPath = AssetDatabase.GetAssetPath(selectedObject);

        if (AssetDatabase.Contains(selectedObject))
        {
            Fill(AssetDatabaseUtils.GetUsed, fullUsedDirect);
            Fill(AssetDatabaseUtils.GetUsedIndirect, fullUsedIndirect);

            inspectedObject = selectedObject;
            FillGo(GameObjectUtils.GetUsed, fullUsedDirect);
            FillGo(GameObjectUtils.GetUsedIndirect, fullUsedIndirect);
        
            Fill(AssetDatabaseUtils.GetUses, fullUsesDirect);
            Fill(AssetDatabaseUtils.GetUsesIndirect, fullUsesIndirect);
        }
        else
        {
            if (selectedObject is GameObject go)
            {
                inspectedObject = go;
                selector.Add("GameObject", go);
                foreach (var comp in go.GetComponents<Component>())
                {
                    selector.Add(comp.GetType().Name, comp);
                }
            }

            OnInspectedObjectChanged();
            return;
        }

        FilterAssetsUsages(); 
        
        return;
        void Fill(Action<string, HashSet<string>> action, List<AssetInfo> full)
        {
            HashSet<string> set = new HashSet<string>();
            action(selectionPath, set);
        
            foreach (string assetPath in set)
            {
                var objs = AssetDatabaseUtils.LoadAllSubAsset(assetPath, true);
                for (int i = 0; i < objs.Count; i++)
                {
                    full.Add(BuildInfo(objs[i], i-1));
                }
            }
        }
    }
    
    private void FillGo(Action<Object, HashSet<Object>> action, List<AssetInfo> full)
    {
        HashSet<Object> list = new HashSet<Object>();
        action(inspectedObject, list);
        
        foreach (var obj in list)
        {
            if (AssetDatabase.IsSubAsset(obj))
            {
                var main = AssetDatabase.GetAssetPath(obj);
                var objs = AssetDatabaseUtils.LoadAllSubAsset(main, false);
                
                for (int i = 0; i < objs.Count; i++)
                {
                    var sub = objs[i];
                    if (sub == obj)
                    {
                        full.Add(BuildInfo(obj, i));
                        break;
                    }
                }
                
                continue;
            }
            
            full.Add(BuildInfo(obj));
        }
    }

    private void ClearAllUsages()
    {
        usedDirect.Clear();
        usedIndirect.Clear();
        usesDirect.Clear();
        usesIndirect.Clear();
        fullUsedDirect.Clear();
        fullUsedIndirect.Clear();
        fullUsesDirect.Clear();
        fullUsesIndirect.Clear();
        Repaint();
    }

    private void FilterAssetsUsages()
    {
        FilterAssets(fullUsedDirect, usedDirect);
        FilterAssets(fullUsedIndirect, usedIndirect);
        
        FilterAssets(fullUsesDirect, usesDirect);
        FilterAssets(fullUsesIndirect, usesIndirect);
        Repaint();
    }
}

