using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LSCore.ConfigModule;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using static System.IO.Path;
using Debug = UnityEngine.Debug;

namespace LSCore.Editor.BackupSystem
{
    [Serializable]
    public class Backuper
    {
        private const string DateTimeSeparator = "_@#$";
        private const int TimeThreshold = 1;
        private static Backuper instance = new();
        private static PropertyTree tree = PropertyTree.Create(instance);
        private static int editCount;
        private static TimeSpan delay;
        private static bool canSave;
        private static CancellationTokenSource source = new();


        [Serializable]
        private class Data
        {
            [DisplayAsString] [TableColumnWidth(10)] public string name;
            [DisplayAsString] [TableColumnWidth(10)] public string dateTime;
            [DisplayAsString] [TableColumnWidth(100)] public string assetPath;
            [NonSerialized] public string backupPath;
            
            private string Source => $"{backupPath}";
            public string FullAssetPath => $"{Application.dataPath}/{assetPath}";
            
            [Button(25, Icon = SdfIconType.Eye)]
            [TableColumnWidth(80, false)]
            public void Show()
            {
                Process.Start(GetDirectoryName(Source));
            }
            
            [Button(25, Icon = SdfIconType.CheckSquareFill)]
            [TableColumnWidth(80, false)]
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
            
            [Button(25, Icon = SdfIconType.XCircleFill)]
            [TableColumnWidth(30, false)]
            public void Delete()
            {
                File.Delete(Source);
                instance.backups.Remove(this);
                instance.backupsSet.Remove(this);
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
            delay = TimeSpan.FromMinutes(BackuperSettings.Config.saveInterval);
            CheckForCanSave();
            if (!Directory.Exists(BackupPath))
            {
                Directory.CreateDirectory(BackupPath);
            }
        }
        
        private static void OnGui(string s)
        {
            tree.BeginDraw(false);
            tree.Draw(false);
            tree.EndDraw();
        }
    
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Light Side Core/Backuper", SettingsScope.User)
            {
                label = "Backuper",
                guiHandler = OnGui,
                keywords = new HashSet<string>(new[] { "Backuper" }),
                activateHandler = Init
            };

            return provider;
        }
        

        [PropertySpace(20)]
        [Button("Open Backups Location", 30, Icon = SdfIconType.FolderFill)]
        private void OpenBackupFolder()
        {
            Process.Start(BackupPath);
        }
        
        private static void Init(string s, VisualElement v)
        {
            instance.Init();
        }
        
        private void Init()
        {
            if (BackuperSettings.Config.maxBackupsCount > 0)
            {
                maxBackupsCount = BackuperSettings.Config.maxBackupsCount;
            }

            if (BackuperSettings.Config.saveInterval > 0)
            {
                saveInterval = BackuperSettings.Config.saveInterval;
            }
            
            backups.Clear();
            backupsSet.Clear();
            var allFiles = new DirectoryInfo(BackupPath).GetFiles("*", SearchOption.AllDirectories);
            var sortedFiles = allFiles.OrderBy(f => f.LastWriteTime).Select(x => x.FullName);
            
            foreach (var backupPath in sortedFiles)
            {
                TryAdd(backupPath);
            }
        }

        
        private void OnSaveIntervalChanged()
        {
            source.Cancel();
            source = new CancellationTokenSource();
            delay = TimeSpan.FromMinutes(saveInterval);
            CheckForCanSave();
            BackuperSettings.Config.saveInterval = saveInterval;
            ConfigUtils.Save<BackuperSettings>();
        }

        private void OnMaxBackupsCountChanged()
        {
            BackuperSettings.Config.maxBackupsCount = maxBackupsCount;
            ConfigUtils.Save<BackuperSettings>();
        }

        private void TryAdd(string backupPath)
        {
            var fileName = GetFileName(backupPath);
            var split = fileName.Split(DateTimeSeparator);

            var name = split[0];
            var ticks = long.Parse(GetFileNameWithoutExtension(split[1]));
            var format = CultureInfo.CurrentCulture.DateTimeFormat;
            var dateTime = new DateTime(ticks).ToString($"{format.ShortDatePattern} {format.ShortTimePattern}");
            
            var assetPath = $"{GetDirectoryName(backupPath)}/{name}{GetExtension(backupPath)}"[(BackupPath.Length + 1)..];
            
            var data = new Data{ name = name, dateTime = dateTime, assetPath = assetPath, backupPath = backupPath};

            if (backupsSet.Add(data))
            {
                backups.Insert(0, data);
            }

            if (backupsSet.Count > maxBackupsCount)
            {
                var index = backups.Count - 1;
                data = backups[index];
                data.Delete();
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
            var fileName = GetFileNameWithoutExtension(scene.path);
            var fileExt = GetExtension(scene.path);
            var relativeFolderPath = GetDirectoryName(scene.path)[7..];
            var filePath = $"{relativeFolderPath}/{fileName}{DateKey}{fileExt}";
            Debug.Log($"Saving {filePath}");
            var backupPath = $"{BackupPath}/{filePath}";
            
            Directory.CreateDirectory($"{BackupPath}/{relativeFolderPath}");
            
            if (EditorSceneManager.SaveScene(scene, backupPath, true))
            {
                //Linker.PathByName[fileName] = scene.path.AssetsPathToFull();
                instance?.TryAdd(backupPath);
                File.Delete($"{backupPath}.meta");
            }
        }

        private static string BackupPath => $"{Application.persistentDataPath}/Backups";
        private static string DateKey => $"{DateTimeSeparator}{DateTime.Now.Ticks}";
        
        private static bool TrySaveCurrentPrefab()
        {
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                GameObject prefabRoot = prefabStage.prefabContentsRoot;
                var fileName = $"{prefabRoot.name}{DateKey}.prefab";
                Debug.Log($"Saving {fileName}");
                string prefabAssetPath = $"{Application.dataPath}/{fileName}";
                string backupPath = $"{BackupPath}/{fileName}";

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);

                if (!File.Exists(backupPath)) File.Create(backupPath).Dispose();

                File.Copy(prefabAssetPath, backupPath, true);
                AssetDatabase.DeleteAsset($"Assets/{fileName}");
                //Linker.PathByName[fileName] = prefabStage.assetPath.AssetsPathToFull();
                instance?.TryAdd(backupPath);
                return true;
            }

            return false;
        }
    }
}
