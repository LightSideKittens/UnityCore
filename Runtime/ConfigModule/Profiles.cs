using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LSCore.ConfigModule
{
    public class Profiles : OdinEditorWindow
    {
        [Serializable]
        private class Data
        {
            [DisplayAsString(18)] public string name;
            [NonSerialized] public string path;
            
            [Button(25, Icon = SdfIconType.CheckSquareFill)]
            [TableColumnWidth(100, false)]
            public void Apply()
            {
                var destinationDirectory = $"{Application.dataPath}/{FolderNames.Configs}/{FolderNames.SaveData}";
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

                AssetDatabase.Refresh();
            }
            
            [Button(25, Icon = SdfIconType.XCircleFill)]
            [TableColumnWidth(30, false)]
            public void Delete()
            {
                File.Delete(path);
                Window.data.Remove(this);
            }
        }
        
        [TableList(HideToolbar = true, IsReadOnly = true)]
        [SerializeField] private List<Data> data = new();
        private static string FileName => $"profile_{DateTime.Now.Ticks}";
        private static string ProfilesPath => $"{Application.persistentDataPath}/Profiles";
        private static Profiles Window => GetWindow<Profiles>();
        
        [MenuItem(LSPaths.Windows.Profiles)]
        private static void OpenWindow()
        {
            Window.Show();
        }

        protected override void Initialize()
        {
            data.Clear();
            var files = Directory.GetFiles(ProfilesPath, "*.tar.gz");
            foreach (var file in files)
            {
                Add(file);
            }
        }

        private void Add(string filePath)
        {
            data.Add(new Data(){name = Regex.Replace(Path.GetFileName(filePath), @"\..+", string.Empty), path = filePath});
        }

        [Button(40, Icon = SdfIconType.Save2Fill)]
        private void Save()
        {
            var directoryPath = Path.Combine("Assets", FolderNames.Configs, FolderNames.SaveData);
            var outputDirectory = ProfilesPath;
            var fileName = $"{outputDirectory}/{FileName}";
            var tarFileName = $"{fileName}.tar";
            var tarGzFileName = $"{fileName}.tar.gz";
            Directory.CreateDirectory(outputDirectory);

            var directorySelected = new DirectoryInfo(directoryPath);

            using (Stream stream = File.Create(tarFileName))
            using (var tarArchive = TarArchive.CreateOutputTarArchive(stream))
            {
                AddDirectoryFilesToTar(tarArchive, directorySelected);
            }
            
            
            using (Stream inStream = File.OpenRead(tarFileName))
            using (Stream gzoStream = new GZipOutputStream(File.Create(tarGzFileName)))
            {
                inStream.CopyTo(gzoStream);
            }

            Add(tarGzFileName);
            File.Delete(tarFileName);
        }

        [Button(30, Icon = SdfIconType.XCircleFill)]
        private void DeleteCurrent()
        {
            var path = Path.Combine("Assets", FolderNames.Configs, FolderNames.SaveData);
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            AssetDatabase.DeleteAssets(directories.Concat(files).ToArray(), new List<string>());
            AssetDatabase.Refresh();
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