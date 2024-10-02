using LSCore.ConfigModule;
using Newtonsoft.Json.Linq;
using static System.IO.Path;

namespace LSCore.LifecycleSystem
{
    public static class LifecycleConfig
    {
        public enum Type
        {
            View,
            Data
        }
        
        public static JToken Get(string systemId, Type type, string path) => GetManager(systemId, type, path).Config.data;
        public static bool Exists(string systemId, Type type, string path) => GetManager(systemId, type, path).Exists();
        public static void Delete(string systemId, Type type, string path) => GetManager(systemId, type, path).Delete();
        public static void DeletePath(string systemId, Type type, string path)
        {
            path = LifecycleConfigManager.Path(Combine(systemId, type.ToString(), path));
            FileExt.DeletePath(path);
        }

        private static LifecycleConfigManager GetManager(string systemId, Type type, string path) => 
            ConfigMaster<LifecycleConfigManager>.Get(Combine(systemId, type.ToString(), path));
    }
}