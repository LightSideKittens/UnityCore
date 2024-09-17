using LSCore.ConfigModule;
using Newtonsoft.Json.Linq;
using static System.IO.Path;

namespace LSCore.QuestModule
{
    public static class QuestConfig
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
            path = QuestConfigManager.Path(Combine(type.ToString(), path));
            FileExt.DeletePath(path);
        }

        private static QuestConfigManager GetManager(Type type, string path) => 
            ConfigMaster<QuestConfigManager>.Get(Combine(type.ToString(), path));
    }
}