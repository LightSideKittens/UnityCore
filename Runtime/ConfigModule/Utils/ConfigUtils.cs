using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public static class ConfigUtils
    {
        [Conditional("UNITY_EDITOR")]
        public static void Save<T>() where T : BaseSingleConfig<T>, new()
        {
            BaseSingleConfig<T>.Config.Save();
        }
        
        [Conditional("UNITY_EDITOR")]
        public static void Save(BaseConfig config)
        {
            config.Save();
        }
        
        public static JToken GetJToken<T>() where T : BaseSingleConfig<T>, new()
        {
            return BaseSingleConfig<T>.Config.GetJToken();
        }
    }
}