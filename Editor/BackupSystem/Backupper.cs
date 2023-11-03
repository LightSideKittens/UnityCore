using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LSCore.ConfigModule;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LSCore.Editor.BackupSystem
{
    public class Backupper : SerializedScriptableObject
    {
        private const string DateTimeSeparator = "_@#$";
        private const int TimeThreshold = 1;
        private static int editCount;
        private static TimeSpan delay;
        private static bool canSave;
        private static CancellationTokenSource source = new();
        private static HashSet<string> setToRemove = new();


        [Serializable]
        private struct Data
        {
            [DisplayAsString] [TableColumnWidth(100)] public string name;
            [DisplayAsString] [TableColumnWidth(70, false)] public string dateTime;
            [DisplayAsString] [TableColumnWidth(500)] public string assetPath;
            private string Source => $"{BackupPath}/{name}{DateTimeSeparator}{dateTime}";
            
            [Button]
            [TableColumnWidth(50, false)]
            public void Apply()
            {
                var source = Source;
                
                if (File.Exists(assetPath))
                {
                    File.Copy(source, assetPath, true);
                }
                else
                {
                    File.Move(source, assetPath);
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

        [SerializeField, Min(5)] [LabelText("Max backups count")] private int maxBackupsCount = 5;


        [SerializeField] 
        [TableList(HideToolbar = true, IsReadOnly = true)]
        private List<Data> backups = new();

        private HashSet<Data> backupsSet = new();

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            Undo.willFlushUndoRecord += OnEdit;
            Undo.undoRedoEvent += OnEdit;
            delay = TimeSpan.FromMinutes(Window.saveInterval);
            CheckForCanSave();
            if (!Directory.Exists(BackupPath))
            {
                Directory.CreateDirectory(BackupPath);
            }
        }
        
        
        [MenuItem(LSPaths.Windows.Backuper)]
        private static void OpenWindow()
        {
            LSPropertyEditor.Show(Window);
        }

        private static Backupper Window
        {
            get
            {
                var directory = $"Assets/{LSPaths.Backuper}";
                var assetPath = $"{directory}/Backups.asset";
                var instance = AssetDatabase.LoadAssetAtPath<Backupper>(assetPath);
                if (instance == null)
                {
                    instance = CreateInstance<Backupper>();
                }
                else
                {
                    return instance;
                }
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                AssetDatabase.CreateAsset(instance, assetPath);
                AssetDatabase.SaveAssets();
                return instance;
            }
        }

        [OnInspectorInit]
        private void Init()
        {
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

            ConfigUtils.Save<Linker>();
            setToRemove.Clear();
        }

        private void OnSaveIntervalChanged()
        {
            source.Cancel();
            source = new CancellationTokenSource();
            delay = TimeSpan.FromMinutes(saveInterval);
            CheckForCanSave();
        }

        private void TryAdd(string backupPath)
        {
            var fileName = Path.GetFileName(backupPath);
            var split = fileName.Split(DateTimeSeparator);
            
            if (Linker.PathByName.TryGetValue(fileName, out var assetPath))
            {
                var data = new Data{ name = split[0], dateTime = split[1], assetPath = assetPath };
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
            if (canSave)
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
                ConfigUtils.Save<Linker>();
                Window.TryAdd(backupPath);
            }
        }

        private static string BackupPath => $"{Application.persistentDataPath}/Backups";
        private static string DateKey => $"{DateTimeSeparator}{DateTime.Now:dd-hh-mm}";
        
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
                if (File.Exists(backupPath))
                {
                    File.Copy(prefabAssetPath, backupPath, true);
                    File.Delete(prefabAssetPath);
                }
                else
                {
                    File.Move(prefabAssetPath, backupPath);
                }
                
                File.Delete($"{prefabAssetPath}.meta");
                Linker.PathByName[fileName] = prefabStage.assetPath.AssetsPathToFull();
                ConfigUtils.Save<Linker>();
                Window.TryAdd(backupPath);
                return true;
            }

            return false;
        }
        
        private class Linker : BaseDebugData<Linker>
        {
            [JsonProperty] private readonly Dictionary<string, string> pathByName = new ();
            public static Dictionary<string, string> PathByName => Config.pathByName;
        }
    }
}

