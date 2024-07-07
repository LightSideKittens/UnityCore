using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public partial class AssetsViewer : OdinEditorWindow
{
    [MenuItem(LSPaths.Windows.AssetsViewer)]
    private static void OpenWindow()
    {
        GetWindow<AssetsViewer>().Show();
    }
    private enum SortType
    {
        None,
        Type,
        Size
    }

    [TabGroup("Tabs", "Assets in Build")]
    [OnValueChanged("SortAssets")]
    [SerializeField] private SortType currentSortType = SortType.None;
    
    [TabGroup("Tabs", "Assets in Build")]
    [OnValueChanged("SortAssets")]
    [SerializeField] private bool ascendingOrder;
    
    [TabGroup("Tabs", "Assets in Build")]
    [TableList]
    public List<AssetInfo> BuildAssets = new List<AssetInfo>();

    
    [TabGroup("Tabs", "Assets in Build")]
    [Button("Refresh List")]
    private void RefreshList()
    {
        BuildAssets.Clear();
        string[] buildScenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        var dependencies = AssetDatabase.GetDependencies(buildScenes);

        foreach (var dependency in dependencies)
        {
            var assetPath = dependency;
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(dependency).Name;
            var assetFileInfo = new FileInfo(assetPath);
            var assetGuid = AssetDatabase.AssetPathToGUID(dependency);
            var assetSize = assetFileInfo.Exists ? assetFileInfo.Length : 0;
            var assetLastModified = assetFileInfo.Exists ? assetFileInfo.LastWriteTime : DateTime.MinValue;

            var assetInfo = new AssetInfo
            {
                Path = assetPath,
                Type = assetType,
                Size = assetSize.ToNiceSize(),
                size = assetSize,
                LastModified = assetLastModified,
                GUID = assetGuid
            };
            BuildAssets.Add(assetInfo);
        }

        SortAssets();
    }
    
    private void SortAssets()
    {
        if (currentSortType == SortType.Type)
        {
            BuildAssets = ascendingOrder 
                ? BuildAssets.OrderBy(a => a.Type).ToList() 
                : BuildAssets.OrderByDescending(a => a.Type).ToList();
        }
        else if (currentSortType == SortType.Size)
        {
            BuildAssets = ascendingOrder 
                ? BuildAssets.OrderBy(a => a.size).ToList() 
                : BuildAssets.OrderByDescending(a => a.size).ToList();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        RefreshList();
        UpdateReferences();
        Selection.selectionChanged -= UpdateReferences;
        Selection.selectionChanged += UpdateReferences;
    }

    public class AssetInfo
    {
        [TableColumnWidth(250)] [DisplayAsString] public string Path;
        [TableColumnWidth(100)] [DisplayAsString] public string Type;
        [TableColumnWidth(100)] [DisplayAsString] public string Size;
        [TableColumnWidth(150)] [DisplayAsString] public DateTime LastModified;
        [TableColumnWidth(250)] [DisplayAsString] public string GUID;
        [NonSerialized] public long size;
    }
}
