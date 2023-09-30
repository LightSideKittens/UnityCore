using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore
{
    public partial class DebugData : BaseConfig<DebugData>
    {
        public string country;
        [JsonProperty] private string environment = LSConsts.Env.Dev;

        public static string Country
        {
            get => Config.country;
            set => Config.country = value;
        }
        
        public static string Environment
        {
            get => Config.environment;
            set => Config.environment = value;
        }
    }
}