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
    [Serializable]
    public class Profiles 
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
                var destinationDirectory = $"{BaseConfig.ConfigsPath}/{FolderNames.DynamicData}";
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
                instance.data.Remove(this);
            }
        }
        
        [Title("Profiles")]
        [SerializeField] 
        [TableList(HideToolbar = true, IsReadOnly = true, AlwaysExpanded = true)]
        private List<Data> data = new();
        
        private static string FileName => $"profile_{DateTime.Now.Ticks}";
        private static string ProfilesPath => $"{Application.persistentDataPath}/Profiles";
        
        public Profiles()
        {
            data.Clear();
            Directory.CreateDirectory(ProfilesPath);
            var files = Directory.GetFiles(ProfilesPath, "*.tar.gz");
            foreach (var file in files)
            {
                Add(file);
            }
        }

        private static readonly Profiles instance = new();
        
#if UNITY_EDITOR
        private static PropertyTree tree = PropertyTree.Create(instance);
        
        static Profiles() => AssemblyReloadEvents.beforeAssemblyReload += OnBeforeRecompile;
        private static void OnBeforeRecompile() => tree.Dispose();

        private static void OnGui(string s)
        {
            tree.BeginDraw(false);
            tree.Draw(false);
            tree.EndDraw();
        }
    
        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider()
        {
            var provider = new SettingsProvider("Preferences/Light Side Core/Profiles", SettingsScope.User)
            {
                label = "Profiles",
                guiHandler = OnGui,
                keywords = new HashSet<string>(new[] { "Profiles" })
            };

            return provider;
        }
        
        [Button(30, Icon = SdfIconType.XCircleFill)]
        private void DeleteCurrent()
        {
            var path = Path.Combine("Assets", FolderNames.Configs, FolderNames.DynamicData);
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            AssetDatabase.DeleteAssets(directories.Concat(files).ToArray(), new List<string>());
            AssetDatabase.Refresh();
        }
        
        [Button("Save", 40, Icon = SdfIconType.Save2Fill)]
        private void Savee() => Internal_Save();
#endif

        public static string Save() => instance.Internal_Save();
        
        private void Add(string filePath)
        {
            data.Add(new Data(Regex.Replace(Path.GetFileName(filePath), @"\..+", string.Empty), filePath));
        }

        private string Internal_Save()
        {
            var directoryPath = Path.Combine("Assets", FolderNames.Configs, FolderNames.DynamicData);
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