#if UNITY_EDITOR
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public class EditorConfig : LocalDynamicConfig
    {
        public JToken data = new JObject();
        public static EditorConfigManager GetManager(string path, bool enableLog = false)
        {
            var configManager = ConfigMaster<EditorConfigManager>.Get(path);
            configManager.isLogEnabled = enableLog;
            return configManager;
        }
    }
}
#endif
