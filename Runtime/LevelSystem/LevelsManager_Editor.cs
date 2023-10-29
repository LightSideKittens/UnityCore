#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LSCore.LevelSystem
{
    public partial class LevelsManager
    {
        private IEnumerable<LevelsContainer> AvailableContainer => AssetDatabaseUtils.LoadAllAssets<LevelsContainer>(paths: folderPath);
        private IEnumerable<IdGroup> AvailableGroups => AssetDatabaseUtils.LoadAllAssets<IdGroup>();
        private IEnumerable<Id> AvailableIds => Groups.SelectMany(group => group.Ids);
        
        [field: NonSerialized] public HashSet<Id> Ids { get; private set; } = new();
        private string folderPath;

        [OnInspectorInit]
        private void OnInit()
        {
            folderPath = this.GetFolderPath();
            Ids.Clear();
            foreach (var level in AvailableContainer)
            {
                Ids.Add(level.Id);
            }
        }
        
        [Button]
        private void CreateContainer()
        {
            PopupWindow.Show(GUILayoutUtility.GetLastRect(), new LevelsPopup {manager = this});
        }

        public class LevelsPopup : PopupWindowContent
        {
            public LevelsManager manager;
            
            public override void OnGUI(Rect rect)
            {
                foreach (var id in manager.AvailableIds)
                {
                    if (manager.Ids.Contains(id))
                    {
                        continue;
                    }

                    if (GUILayout.Button(id))
                    {
                        var container = CreateInstance<LevelsContainer>();
                        container.Id = id;
                        container.Manager = manager;
                        container.name = id; 
                        AssetDatabase.CreateFolder(manager.folderPath, id);
                        AssetDatabase.CreateAsset(container, $"{manager.folderPath}/{id}/{id}_Container.asset");
                        manager.Ids.Add(id);
                    }
                }
            }
        }
    }
}
#endif