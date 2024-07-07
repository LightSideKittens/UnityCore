/*using System.Collections.Generic;
using LSCore.ConfigModule;
using UnityEditor;

[InitializeOnLoad]
public class AssetDependencies
{
    static AssetDependencies()
    {
        LSAssetPostprocessor.Imported += Add;
        LSAssetPostprocessor.Deleted += Remove;
    }

    private static void Add(string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            var guid = AssetDatabase.AssetPathToGUID(paths[i]);
            AssetConfig.Get(guid);
        }
    }

    private static void Remove(string[] paths)
    {
        for (int i = 0; i < paths.Length; i++)
        {
            var guid = AssetDatabase.AssetPathToGUID(paths[i]);
            ConfigUtils.Delete<AssetConfig>(guid);
        }
    }
    
    public class AssetConfig : ManualSaveConfig<AssetConfig>
    {
        public HashSet<string> usingAssets = new();
        public HashSet<string> usedByAssets = new();
        protected override string RootPath => $"{ApplicationUtils.LibraryPath}/AssetConfigs";
    }
}*/