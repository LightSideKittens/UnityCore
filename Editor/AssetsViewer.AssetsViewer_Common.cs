using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LSCore.ReferenceFrom.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

internal partial class AssetsViewer : OdinEditorWindow
{
    [Flags]
    private enum FilterType
    {
        None,
        InSceneOrPrefab
    }
    
    private enum SortType
    {
        None,
        Type,
        Size,
    }
    
    [Flags]
    private enum ColumnFlags
    {
        All = -1,
        Path = 0,
        Type = 1,
        Size = 2,
        LastModified = 4,
        Guid = 8
    }
    
    [OnValueChanged("FilterAllAssets")]
    [SerializeField] private FilterType currentFilterType = FilterType.None;
    
    [OnValueChanged("FilterAllAssets")]
    [SerializeField] private SortType sortType = SortType.None;
    
    [OnValueChanged("UpdateColumns")]
    [SerializeField] private ColumnFlags columnFlags = ColumnFlags.Type;
    
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
        
        if (currentFilterType == FilterType.InSceneOrPrefab)
        {
            sorted = ascendingOrder 
                ? src.Where(a => a.inSceneOfPrefab) 
                : src.Where(a => a.inSceneOfPrefab).Reverse();
        }
        
        if (sortType == SortType.Type)
        {
            sorted = ascendingOrder 
                ? src.OrderBy(a => a.Type) 
                : src.OrderByDescending(a => a.Type);
        }
        else if (sortType == SortType.Size)
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
                var split = searchText.Split(' ');
                var type = split[0][2..];
                sorted = sorted.Where(x => x.Type.Contains(type));
                if (split.Length > 1)
                {
                    sorted = sorted.Where(x => x.Name.Contains(split[1]));
                }
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
        
        Repaint();
    }
    
    private AssetInfo BuildInfo(Object obj, int subAssetIndex = -1)
    {
        string objName = obj.name;
        string assetPath;
        string assetType = obj.GetType().Name;
        string assetGuid;
        long assetSize;
        bool isSceneOrPrefab = !AssetDatabase.Contains(obj);
        DateTime assetLastModified;
        
        if (!isSceneOrPrefab)
        {
            assetPath = AssetDatabase.GetAssetPath(obj);
            var assetFileInfo = new FileInfo(assetPath);
            assetGuid = AssetDatabase.AssetPathToGUID(assetPath);
            
            if (subAssetIndex > -1)
            {
                assetGuid = $"{assetGuid}_{subAssetIndex}";
            }
            
            assetSize = assetFileInfo.Exists ? assetFileInfo.Length : 0;
            assetLastModified = assetFileInfo.Exists ? assetFileInfo.LastWriteTime : DateTime.MinValue;
            assetPath = assetPath.Replace("Assets/", string.Empty);
        }
        else
        {
            assetPath = string.Empty;
            assetGuid = string.Empty;
            assetSize = 0;
            assetLastModified = DateTime.MinValue;

            if (obj is GameObject go)
            {
                assetPath = go.GetPath();
                assetGuid = go.GetInstanceID().ToString();
            }
            else if(obj is Component comp)
            {
                assetPath = PrefabStageUtility.GetCurrentPrefabStage() == null ? $"{comp.gameObject.scene.name}/{comp.GetPath()}" : comp.GetPath();
                assetGuid = comp.GetInstanceID().ToString();
            }
        }

        return new AssetInfo
        {
            Name = objName,
            Path = assetPath,
            Type = assetType,
            Size = assetSize.ToNiceSize(),
            size = assetSize,
            LastModified = assetLastModified,
            GUID = assetGuid,
            inSceneOfPrefab = isSceneOrPrefab,
            subAssetIndex = subAssetIndex
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
        Selection.selectionChanged += UpdateReferences;
        GameObjectUtils.GraphUpdated += UpdateReferences;
        UpdateReferences();
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameObjectUtils.GraphUpdated -= UpdateReferences;
        Selection.selectionChanged -= UpdateReferences;
    }

    public class AssetInfo
    {
        [PropertyOrder(-1)]
        [TableColumnWidth(30, false)]
        [Button(SdfIconType.Apple, "")]
        public void Ping()
        {
            var assetPath = $"Assets/{Path}";
            Object obj;
            if (AssetDatabaseUtils.AssetExists(assetPath))
            {
                if (subAssetIndex > -1)
                {
                    obj = AssetDatabase.LoadAllAssetsAtPath(assetPath)[subAssetIndex];
                }
                else
                {
                    obj = AssetDatabase.LoadMainAssetAtPath(assetPath);
                }
            }
            else
            {
                var newPath = RemoveUpToFirstSlash(Path);
                obj = GameObject.Find(newPath);

                if (obj == null)
                {
                    PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
                    if (prefabStage != null)
                    {
                        GameObject prefabRoot = prefabStage.prefabContentsRoot;
                        obj = prefabRoot.transform.Find(newPath);
                    }
                }
            }
            
            EditorGUIUtility.PingObject(obj);
            return;

            static string RemoveUpToFirstSlash(string path)
            {
                int slashIndex = path.IndexOf('/');
                return slashIndex != -1 ? path[(slashIndex + 1)..] : path;
            }
        }
        
        [TableColumnWidth(100)] [DisplayAsString] [GUIColor("GetColor")] public string Name;
        [TableColumnWidth(200)] [DisplayAsString] [GUIColor("GetColor")] public string Path;
        [TableColumnWidth(100)] [DisplayAsString] [GUIColor("GetColor")] public string Type;
        [TableColumnWidth(60, false)] [DisplayAsString] [GUIColor("GetColor")] public string Size;
        [TableColumnWidth(150, false)] [DisplayAsString] [GUIColor("GetColor")] public DateTime LastModified;
        [TableColumnWidth(250, false)] [DisplayAsString] [GUIColor("GetColor")] public string GUID;
        [NonSerialized] public long size;
        [NonSerialized] public bool inSceneOfPrefab;
        [NonSerialized] public int subAssetIndex = -1;

        private Color GetColor()
        {
            return inSceneOfPrefab ? new Color(0.51f, 0.94f, 1f) : Color.white;
        }
        
        public static bool ShowPath => HasFlag(ColumnFlags.Path);
        public static bool ShowType => HasFlag(ColumnFlags.Type);
        public static bool ShowSize => HasFlag(ColumnFlags.Size);
        public static bool ShowLastModified => HasFlag(ColumnFlags.LastModified);
        public static bool ShowGuid => HasFlag(ColumnFlags.Guid);
        
        private static bool HasFlag(ColumnFlags flag) => Instance.columnFlags.HasFlag(flag);
    }
}