#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;

namespace LSCore.ConfigModule
{
    public static partial class ConfigUtils
    {
        [MenuItem(LSPaths.MenuItem.Configs + "/Delete Save Data")]
        private static void DeleteSaveData()
        {
            var path = Path.Combine("Assets", FolderNames.Configs, FolderNames.SaveData);
            var directories = Directory.GetDirectories(path);
            var files = Directory.GetFiles(path);
            AssetDatabase.DeleteAssets(directories.Concat(files).ToArray(), new List<string>());
            AssetDatabase.Refresh();
        }
    }
}
#endif