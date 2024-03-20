using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
#if UNITY_EDITOR
    //[InitializeOnLoad]
#endif
    public class IdGroup : SerializedScriptableObject, IEnumerable<Id>
    {
        [OdinSerialize]
        [HideReferenceObjectPicker]
        [DisableIf("includeAll")]
        [ValueDropdown("AllIds", IsUniqueList = true)]
        private HashSet<Id> ids = new();

        public HashSet<Id> Ids => ids;

        public bool Contains(Id id) => ids.Contains(id);
        public IEnumerator<Id> GetEnumerator() => ids.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

#if UNITY_EDITOR
        
        [SerializeField] [OnValueChanged("OnInit")]
        private bool includeAll;

        private FileSystemWatcher watcher;
        private SynchronizationContext context;
        
        static IdGroup()
        {
            EditorApplication.update += Init;
            World.Created += OnWorldCreated;
            World.Destroyed += OnWorldCreated;
        }

        private static void OnWorldCreated()
        {
            foreach (var group in AssetDatabaseUtils.LoadAllAssets<IdGroup>())
            {
                var set = new HashSet<Id>();
                
                foreach (var id in group.ids)
                {
                    set.Add(id);
                }

                group.ids = set;
            }
        }

        private static void Init()
        {
            foreach (var group in AssetDatabaseUtils.LoadAllAssets<IdGroup>())
            {
                group.OnInit();
            }
            
            EditorApplication.update -= Init;
        }

        private void Listen()
        {
            watcher?.Dispose();
            watcher = new FileSystemWatcher();
            context = SynchronizationContext.Current;
            watcher.Path = Path.GetFullPath(this.GetFolderPath().AssetsPathToFull());
            watcher.NotifyFilter = NotifyFilters.LastWrite
                                   | NotifyFilters.FileName
                                   | NotifyFilters.DirectoryName;
            watcher.Filter = "*.id";
            watcher.IncludeSubdirectories = true;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.EnableRaisingEvents = true;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            context.Post(_ => { Setup(); }, null);
        }


        [OnInspectorInit]
        private void OnInit()
        {
            Listen();
            Setup();
        }

        private void Setup()
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
                if (id == null || !AllIds.Contains(id))
                {
                    toRemove.Add(id);
                    EditorUtility.SetDirty(this);
                }
            }

            ids.ExceptWith(toRemove);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        private HashSet<Id> AllIds
        {
            get
            {
                var path = this.GetFolderPath();
                var allIds = AssetDatabaseUtils.LoadAllAssets<Id>(paths: path).ToHashSet();

                return allIds;
            }
        }
#endif

    }
}