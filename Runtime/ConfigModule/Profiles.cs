using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

using UnityEditor;
using UnityEngine;

namespace LSCore.ConfigModule
{
    public class Profiles 
#if UNITY_EDITOR
        : OdinEditorWindow
#endif
    {
        [Serializable]
        private class Data
        {
            [DisplayAsString(18)] 
            [ShowInInspector] 
            private string name;
            
            [NonSerialized] private string path;

            public Data(string name, string path)
            {
                this.name = name;
                this.path = path;
            }
            
            [Button(25, Icon = SdfIconType.Eye)]
            [TableColumnWidth(80, false)]
            public void Show()
            {
                Process.Start(Path.GetDirectoryName(path));
            }

            [Button(25, Icon = SdfIconType.CheckSquareFill)]
            [TableColumnWidth(100, false)]
            public void Apply()
            {
                var destinationDirectory = $"{BaseConfig.DataPath}/{FolderNames.Configs}/{FolderNames.SaveData}";
                Directory.Delete(destinationDirectory, true);
                Directory.CreateDirectory(destinationDirectory);
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                using var gziStream = new GZipInputStream(fs);
                using var tarInputStream = new TarInputStream(gziStream, Encoding.UTF8);
                while (tarInputStream.GetNextEntry() is { } entry)
                {
                    var destinationPath = Path.Combine(destinationDirectory, entry.Name);
                    
                    var entryDirPath = Path.GetDirectoryName(destinationPath);
                    if (entryDirPath != null)
                    {
                        Directory.CreateDirectory(entryDirPath);
                    }
                    
                    using var outFileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write);
                    tarInputStream.CopyEntryContents(outFileStream);
                }
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
            }
            
            [Button(25, Icon = SdfIconType.XCircleFill)]
            [TableColumnWidth(30, false)]
            public void Delete()
            {
                File.Delete(path);
                Instance.data.Remove(this);
            }
        }
        
        [TableList(HideToolbar = true, IsReadOnly = true)]
        [SerializeField] private List<Data> data = new();
        
        private static string FileName => $"profile_{DateTime.Now.Ticks}";
        private static string ProfilesPath => $"{Application.persistentDataPath}/Profiles";
        
        
#if !UNITY_EDITOR
        private static Profiles instance = new Profiles();

        public Profiles()
        {
            Init();
        }
#endif

        
        private static Profiles Instance
        {
            get
            {
#if UNITY_EDITOR
                return GetWindow<Profiles>();
#else
                return instance;
#endif
            }
        }

        private void Init()
        {
            data.Clear();
            Directory.CreateDirectory(ProfilesPath);
            var files = Directory.GetFiles(ProfilesPath, "*.tar.gz");
            foreach (var file in files)
            {
                Add(file);
            }
        }
        
#if UNITY_EDITOR
        [MenuItem(LSPaths.Windows.Profiles)]
        private static void OpenWindow()
        {
            Instance.Show();
        }

        protected override void Initialize() => Init();
        
        [Button(30, Icon = SdfIconType.XCircleFill)]
        private void DeleteCurrent()
        {
            var path = Path.Combine("Assets", FolderNames.Configs, FolderNames.SaveData);
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            AssetDatabase.DeleteAssets(directories.Concat(files).ToArray(), new List<string>());
            AssetDatabase.Refresh();
        }
        
        [Button("Save", 40, Icon = SdfIconType.Save2Fill)]
        private void Savee() => Internal_Save();
#endif

        public static string Save() => Instance.Internal_Save();
        
        private void Add(string filePath)
        {
            data.Add(new Data(Regex.Replace(Path.GetFileName(filePath), @"\..+", string.Empty), filePath));
        }

        private string Internal_Save()
        {
            var directoryPath = Path.Combine("Assets", FolderNames.Configs, FolderNames.SaveData);
            var outputDirectory = ProfilesPath;
            var tarGzFileName = $"{outputDirectory}/{FileName}.tar.gz";
            Directory.CreateDirectory(outputDirectory);

            var directorySelected = new DirectoryInfo(directoryPath);

            using (Stream stream = new MemoryStream())
            {
                using (var tarArchive = TarArchive.CreateOutputTarArchive(stream))
                {
                    tarArchive.IsStreamOwner = false;
                    AddDirectoryFilesToTar(tarArchive, directorySelected);
                }

                stream.Seek(0, SeekOrigin.Begin);
                
                using (Stream gzoStream = new GZipOutputStream(File.Create(tarGzFileName)))
                {
                    stream.CopyTo(gzoStream);
                }            
            }
            
            Add(tarGzFileName);
            return tarGzFileName;
        }

        private static void AddDirectoryFilesToTar(TarArchive tarArchive, DirectoryInfo directoryInfo)
        {
            var offset = directoryInfo.FullName.Length + 1;
            foreach (var fileInfo in directoryInfo.GetFiles("*.json", SearchOption.AllDirectories))
            {
                var entryName = fileInfo.FullName[offset..];
                TarEntry entry = TarEntry.CreateEntryFromFile(fileInfo.FullName);
                entry.Name = entryName;
                tarArchive.WriteEntry(entry, true);
            }
        }
    }
}