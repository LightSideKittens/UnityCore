using Newtonsoft.Json;

namespace LSCore
{
    public partial class LSDebugData : BaseDebugData<LSDebugData>
    {
        protected override string FileName => nameof(LSDebugData);
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