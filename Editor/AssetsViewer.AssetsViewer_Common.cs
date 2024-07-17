using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

internal partial class AssetsViewer : OdinEditorWindow
{
    private enum FilterType
    {
        None,
        Type,
        Size
    }
    
    [Flags]
    private enum ColumnFlags
    {
        All = -1,
        Path = 0,
        Type = 2,
        Size = 4,
        LastModified = 8,
        Guid = 16,
        Default = Type
    }
    
    [OnValueChanged("FilterAllAssets")]
    [SerializeField] private FilterType currentFilterType = FilterType.None;
    
    [OnValueChanged("UpdateColumns")]
    [SerializeField] private ColumnFlags columnFlags = ColumnFlags.Default;
    
    [OnValueChanged("FilterAllAssets")]
    [SerializeField] private bool ascendingOrder;

    private static AssetsViewer Instance { get;  set; }
    
    [MenuItem(LSPaths.Windows.AssetsViewer)]
    private static void OpenWindow()
    {
        GetWindow<AssetsViewer>().Show();
    }
    
    private void UpdateColumns()
    {
        
    }
    
     private void FilterAllAssets()
    {
        FilterBuildAssets();
        FilterAssetsUsages();
    }
    
    private void FilterAssets(List<AssetInfo> src, List<AssetInfo> dst)
    {
        IEnumerable<AssetInfo> sorted = src;
        
        if (currentFilterType == FilterType.Type)
        {
            sorted = ascendingOrder 
                ? src.OrderBy(a => a.Type) 
                : src.OrderByDescending(a => a.Type);
        }
        else if (currentFilterType == FilterType.Size)
        {
            sorted = ascendingOrder 
                ? src.OrderBy(a => a.size) 
                : src.OrderByDescending(a => a.size);
        }

        const string Type = "t:";

        if (!string.IsNullOrEmpty(searchText))
        {
            if (searchText.StartsWith(Type))
            {
                var search = searchText[2..];
                sorted = sorted.Where(x => x.Type.Contains(search));
            }
            else
            {
                sorted = sorted.Where(x => Path.GetFileName(x.Path).Contains(searchText));
            }
        }
        
        if (sorted != null)
        {
            dst.Clear();
            dst.AddRange(sorted);
        }
    }

    private AssetInfo BuildInfo(string assetPath)
    {
        var assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath).Name;
        var assetFileInfo = new FileInfo(assetPath);
        var assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
        var assetSize = assetFileInfo.Exists ? assetFileInfo.Length : 0;
        var assetLastModified = assetFileInfo.Exists ? assetFileInfo.LastWriteTime : DateTime.MinValue;

        return new AssetInfo
        {
            Name = Path.GetFileNameWithoutExtension(assetPath),
            Path = assetPath.Replace("Assets/", string.Empty),
            Type = assetType,
            Size = assetSize.ToNiceSize(),
            size = assetSize,
            LastModified = assetLastModified,
            GUID = assetGuid
        };
    }
    
    private AssetInfo BuildInfo(Object obj)
    {
        string objName;
        string assetPath;
        string assetType;
        string assetGuid;
        long assetSize;
        DateTime assetLastModified;

        if (AssetDatabase.Contains(obj))
        {
            assetPath = AssetDatabase.GetAssetPath(obj);
            objName = Path.GetFileNameWithoutExtension(assetPath);
            assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath).Name;
            var assetFileInfo = new FileInfo(assetPath);
            assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            assetSize = assetFileInfo.Exists ? assetFileInfo.Length : 0;
            assetLastModified = assetFileInfo.Exists ? assetFileInfo.LastWriteTime : DateTime.MinValue;
            assetPath = assetPath.Replace("Assets/", string.Empty);
        }
        else
        {
            objName = obj.name;
            assetPath = string.Empty;
            assetType = obj.GetType().Name;
            assetGuid = string.Empty;
            assetSize = 0;
            assetLastModified = DateTime.MinValue;
        }

        return new AssetInfo
        {
            Name = objName,
            Path = assetPath,
            Type = assetType,
            Size = assetSize.ToNiceSize(),
            size = assetSize,
            LastModified = assetLastModified,
            GUID = assetGuid
        };
    }
    
    private string searchText;
    
    protected override void OnImGUI()
    {
        Search();
        base.OnImGUI();
    }

    private void Search()
    {
        if (LSSearchField.Draw(ref searchText))
        {
            FilterAllAssets();
        }
    }

    protected override void Initialize()
    {
        Instance = this;
        base.Initialize();
    }

    protected override void OnEnable()
    {
        RefreshList();
        UpdateReferences();
        base.OnEnable();
    }

    public class AssetInfo
    {
        [PropertyOrder(-1)]
        [TableColumnWidth(30, false)]
        [Button(SdfIconType.Apple, "")]
        public void Ping()
        {
            EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath($"Assets/{Path}"));
        }
        
        [TableColumnWidth(100)] [DisplayAsString] public string Name;
        [TableColumnWidth(200)] [DisplayAsString] public string Path;
        [TableColumnWidth(100)] [DisplayAsString] public string Type;
        [TableColumnWidth(60, false)] [DisplayAsString] public string Size;
        [TableColumnWidth(150, false)] [DisplayAsString] public DateTime LastModified;
        [TableColumnWidth(250, false)] [DisplayAsString] public string GUID;
        [NonSerialized] public long size;

        public static bool ShowPath => HasFlag(ColumnFlags.Path);
        public static bool ShowType => HasFlag(ColumnFlags.Type);
        public static bool ShowSize => HasFlag(ColumnFlags.Size);
        public static bool ShowLastModified => HasFlag(ColumnFlags.LastModified);
        public static bool ShowGuid => HasFlag(ColumnFlags.Guid);
        
        private static bool HasFlag(ColumnFlags flag) => Instance.columnFlags.HasFlag(flag);
    }
}