#if UNITY_EDITOR
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public class EditorConfig : LocalDynamicConfig
    {
        public JToken data = new JObject();
        public static EditorConfigManager GetManager(string path) => ConfigMaster<EditorConfigManager>.Get(path);
    }
}
#endif
