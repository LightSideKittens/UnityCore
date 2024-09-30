using LSCore.ConfigModule;
using Newtonsoft.Json.Linq;
using static System.IO.Path;

namespace LSCore.ObjectModule
{
    public class LifecycleConfig<T>
    {
        public enum Type
        {
            View,
            Data
        }
        
        public static JToken Get(Type type, string path) => GetManager(type, path).Config.data;
        public static bool Exists(Type type, string path) => GetManager(type, path).Exists();
        public static void Delete(Type type, string path) => GetManager(type, path).Delete();
        public static void DeletePath(Type type, string path)
        {
            path = LifecycleConfigManager<T>.Path(Combine(type.ToString(), path));
            FileExt.DeletePath(path);
        }

        private static LifecycleConfigManager<T> GetManager(Type type, string path) => 
            ConfigMaster<LifecycleConfigManager<T>>.Get(Combine(type.ToString(), path));
    }
}