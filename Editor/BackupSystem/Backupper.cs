using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LSCore.ConfigModule;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LSCore.Editor.BackupSystem
{
    public class Backupper : OdinEditorWindow, IHasCustomMenu
    {
        private const string DateTimeSeparator = "_@#$";
        private const int TimeThreshold = 1;
        private static Backupper instance;
        private static int editCount;
        private static TimeSpan delay;
        private static bool canSave;
        private static CancellationTokenSource source = new();
        private static HashSet<string> setToRemove = new();


        [Serializable]
        private struct Data
        {
            [DisplayAsString] [TableColumnWidth(100)] public string name;
            [DisplayAsString] [TableColumnWidth(100, false)] public string dateTime;
            [DisplayAsString] [TableColumnWidth(300)] public string assetPath;
            private string Source => $"{BackupPath}/{name}{DateTimeSeparator}{realDateTime}";
            [HideInInspector] public string realDateTime;
            public string FullAssetPath => $"{Application.dataPath}/{assetPath}";
            
            [Button]
            [TableColumnWidth(50, false)]
            public void Apply()
            {
                var source = Source;
                
                if (File.Exists(FullAssetPath))
                {
                    File.Copy(source, FullAssetPath, true);
                }
                else
                {
                    File.Move(source, FullAssetPath);
                }
                
                AssetDatabase.Refresh();
            }

            public void Delete()
            {
                File.Delete(Source);
            }
        }

        [SerializeField, Min(1)] [LabelText("Save Interval in minutes")]
        [OnValueChanged(nameof(OnSaveIntervalChanged))]
        private int saveInterval = 1;

        [SerializeField, Min(5)] 
        [LabelText("Max backups count")]
        [OnValueChanged(nameof(OnMaxBackupsCountChanged))]
        private int maxBackupsCount = 30;
        
        [Title("Backups")]
        [SerializeField] 
        [TableList(HideToolbar = true, IsReadOnly = true, AlwaysExpanded = true)]
        private List<Data> backups = new();

        private HashSet<Data> backupsSet = new();

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            Undo.willFlushUndoRecord += OnEdit;
            Undo.undoRedoEvent += OnEdit;
            delay = TimeSpan.FromMinutes(Linker.Config.saveInterval);
            CheckForCanSave();
            if (!Directory.Exists(BackupPath))
            {
                Directory.CreateDirectory(BackupPath);
            }
        }
        
        
        [MenuItem(LSPaths.Windows.Backuper)]
        private static void OpenWindow()
        {
            GetWindow<Backupper>().Show();
        }
        
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddItem(new GUIContent("Clear"), false, () =>
            {
                foreach (var data in backups)
                {
                    data.Delete();
                }
                
                backups.Clear();
            });
        }
        

        [OnInspectorInit]
        private void Init()
        {
            instance = this;
            backups.Clear();
            backupsSet.Clear();
            var allFiles = Directory.GetFiles(BackupPath);
            setToRemove.UnionWith(Linker.PathByName.Keys);
            
            foreach (var backupPath in allFiles)
            {
                TryAdd(backupPath);
            }
            
            foreach (var toRemove in setToRemove)
            {
                Linker.PathByName.Remove(toRemove);
            }

            Save();
            setToRemove.Clear();
        }

        private void OnSaveIntervalChanged()
        {
            source.Cancel();
            source = new CancellationTokenSource();
            delay = TimeSpan.FromMinutes(saveInterval);
            CheckForCanSave();
            Linker.Config.saveInterval = saveInterval;
            Save();
        }

        private void OnMaxBackupsCountChanged()
        {
            Linker.Config.maxBackupsCount = maxBackupsCount;
            Save();
        }

        private void TryAdd(string backupPath)
        {
            var fileName = Path.GetFileName(backupPath);
            var split = fileName.Split(DateTimeSeparator);
            
            if (Linker.PathByName.TryGetValue(fileName, out var assetPath))
            {
                var date = Path.GetFileNameWithoutExtension(split[1]).Split('-');
                
                var data = new Data{ name = split[0], dateTime = $"{date[0]}/{date[1]} {date[2]}:{date[3]}", realDateTime = split[1], assetPath = assetPath.Replace($"{Application.dataPath}/", string.Empty) };
                setToRemove.Remove(fileName);

                if (backupsSet.Add(data))
                {
                    backups.Insert(0, data);
                }

                if (backupsSet.Count > maxBackupsCount)
                {
                    var index = backups.Count - 1;
                    data = backups[index];
                    backupsSet.Remove(data);
                    backups.RemoveAt(index);
                    data.Delete();
                }
            }
        }

        private static void OnEdit(in UndoRedoInfo _) => OnEdit();

        private static void OnEdit()
        {
            if (canSave && !Application.isPlaying)
            {
                if (!TrySaveCurrentPrefab())
                {
                    TrySaveCurrentScene();
                }
                
                canSave = false;
            }
        }
        
        private static async void CheckForCanSave()
        {
            try
            {
                await Task.Delay(delay, source.Token);
            }
            catch { }
            
            if (source.IsCancellationRequested)
            {
                return;
            }
            canSave = true;
            CheckForCanSave();
        }
        
        private static void TrySaveCurrentScene()
        {
            var scene = SceneManager.GetActiveScene();
            var fileName = $"{scene.name}{DateKey}.unity";
            var backupPath = $"{BackupPath}/{fileName}";
            
            if (EditorSceneManager.SaveScene(scene, backupPath, true))
            {
                Linker.PathByName[fileName] = scene.path.AssetsPathToFull();
                Save();
                instance?.TryAdd(backupPath);
                File.Delete($"{backupPath}.meta");
            }
        }

        private static string BackupPath => $"{Application.persistentDataPath}/Backups";
        private static string DateKey => $"{DateTimeSeparator}{DateTime.Now:MM-dd-hh-mm}";
        
        private static bool TrySaveCurrentPrefab()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                GameObject prefabRoot = prefabStage.prefabContentsRoot;
                var fileName = $"{prefabRoot.name}{DateKey}.prefab";
                string prefabAssetPath = $"{Application.dataPath}/{fileName}";
                string backupPath = $"{BackupPath}/{fileName}";

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);
                
                if (!File.Exists(backupPath)) File.Create(backupPath).Dispose();

                File.Copy(prefabAssetPath, backupPath, true);
                AssetDatabase.DeleteAsset($"Assets/{fileName}");
                
                Linker.PathByName[fileName] = prefabStage.assetPath.AssetsPathToFull();
                Save();
                instance?.TryAdd(backupPath);
                return true;
            }

            return false;
        }

        private static void Save()
        {
            ConfigUtils.Save<Linker>();
        }
        
        private class Linker : BaseDebugData<Linker>
        {
#if UNITY_EDITOR
            protected override bool LogEnabled => false;
#endif
            
            [JsonProperty] private readonly Dictionary<string, string> pathByName = new ();
            public int maxBackupsCount = 5;
            public int saveInterval = 1;
            public static Dictionary<string, string> PathByName => Config.pathByName;
        }
    }
}

