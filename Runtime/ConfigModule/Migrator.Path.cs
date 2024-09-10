using System.Collections.Generic;

namespace LSCore.ConfigModule
{
    public static partial class Migrator
    {
        //TODO: Implement cool and convenient logic of path migration
        /*public static class Path
        {
            private class Versions : LocalDynamicConfig
            {
                public Dictionary<string, int> versions = new();
            }
            
            private static Dictionary<string, List<string>> byKey = new();
            private static AutoSaveConfigManager<Versions> Manager => ConfigMaster<AutoSaveConfigManager<Versions>>.Get(ConfigPaths.Root + "/PathVersions");
            private static Versions Config => Manager.Config;
        
            public static void Add(string key, string path)
            {
                if (!byKey.TryGetValue(key, out var history))
                {
                    history = new List<string>();
                    byKey.Add(key, history);
                }
            
                history.Add(path);
            }

            internal static bool TryGet(string key, out (string current, string target) data)
            {
                if (byKey.TryGetValue(key, out var paths))
                {
                    if (paths.Count > 0)
                    {
                        var versions = Config.versions;
                        int version = versions.GetValueOrDefault(key);
                        var path = paths[version];
                        version = paths.Count - 1;
                        versions[key] = version;

                        data = (path, paths[version]);
                        return data.current != data.target;
                    }
                }
                
                data = default;
                return false;
            }
        }*/
    }
}