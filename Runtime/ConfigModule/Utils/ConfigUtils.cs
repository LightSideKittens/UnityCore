using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public static partial class ConfigUtils
    {
        [Conditional("UNITY_EDITOR")]
        public static void Save<T>() where T : BaseConfig<T>, new()
        {
            BaseConfig<T>.Save();
        }
        
        public static JToken GetJToken<T>() where T : BaseConfig<T>, new()
        {
            return BaseConfig<T>.GetJToken();
        }
    }
}