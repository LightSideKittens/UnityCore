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
        
        public static JToken Get(string systemId, Type type, string path) => GetManager(systemId, type, path).Config.data;
        public static bool Exists(string systemId, Type type, string path) => GetManager(systemId, type, path).Exists();
        public static void Delete(string systemId, Type type, string path) => GetManager(systemId, type, path).Delete();
        public static void DeletePath(string systemId, Type type, string path)
        {
            path = QuestConfigManager.Path(Combine(systemId, type.ToString(), path));
            FileExt.DeletePath(path);
        }

        private static QuestConfigManager GetManager(string systemId, Type type, string path) => 
            ConfigMaster<QuestConfigManager>.Get(Combine(systemId, type.ToString(), path));
    }
}