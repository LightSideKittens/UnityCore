using System.Collections.Generic;
using System.IO;
using System.Threading;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

public class IdGroup : SerializedScriptableObject
{
    [OdinSerialize] [HideReferenceObjectPicker]
    [DisableIf("includeAll")]
    [ValueDropdown("AllIds", IsUniqueList = true)]
    private HashSet<Id> ids = new();

    public HashSet<Id> Ids => ids;
    
    public bool Contains(Id id) => ids.Contains(id);
    
#if UNITY_EDITOR
    [SerializeField] [OnValueChanged("OnInit")] private bool includeAll;
    private FileSystemWatcher watcher = new();
    private SynchronizationContext context;
    
    [InitializeOnLoadMethod]
    private static void Init()
    {
        foreach (var group in AssetDatabaseUtils.LoadAllAssets<IdGroup>())
        {
            group.Listen();
            group.OnInit();
        }
    }

    private void Listen()
    {
        context = SynchronizationContext.Current;
        watcher.Path = Path.GetFullPath(this.GetFolderPath().AssetsPathToFull());
        watcher.NotifyFilter = NotifyFilters.LastWrite
                               | NotifyFilters.FileName
                               | NotifyFilters.DirectoryName;
        watcher.Filter = "*.id";
        watcher.IncludeSubdirectories = true;
        watcher.Created += OnCreated;
        watcher.Deleted += OnDeleted;
        watcher.EnableRaisingEvents = true;
    }

    private void OnCreated(object sender, FileSystemEventArgs e)
    {
        context.Post(_ =>
        {
            OnInit();
        }, null);
    }

    private void OnDeleted(object sender, FileSystemEventArgs e)
    {
        context.Post(_ =>
        {
            OnInit();
        }, null);
    }


    [OnInspectorInit]
    private void OnInit()
    {
        if (includeAll)
        {
            foreach (var id in AllIds)
            {
                if (ids.Add(id))
                {
                    EditorUtility.SetDirty(this);
                }
            }
        }
        
        var toRemove = new List<Id>();
            
        foreach (var id in ids)
        {
            if (id == null)
            {
                toRemove.Add(id);
                EditorUtility.SetDirty(this);
            }
        }

        ids.ExceptWith(toRemove);
        AssetDatabase.SaveAssetIfDirty(this);
    }
    
    private IEnumerable<Id> AllIds
    {
        get
        {
            var path = this.GetFolderPath();
            var allIds = AssetDatabaseUtils.LoadAllAssets<Id>(paths: path);
            
            
            foreach (var id in allIds)
            {
                yield return id;
            }
        }
    }
#endif
}