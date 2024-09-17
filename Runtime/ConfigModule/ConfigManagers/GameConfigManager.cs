using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public class GameConfigManager : GameConfigManager<GameConfig> { }

    public class GameConfigManager<T> : AutoSaveConfigManager<T> where T : LocalDynamicConfig, new()
    {
        protected override string GetPath(string path)
        {
            return ConfigPaths.Game.Dynamic(path);
        }
    }
    
    public class GameConfig : LocalDynamicConfig
    {
        public JToken data = new JObject();
        public static JToken Get(string path) => GetManager(path).Config.data;
        protected static GameConfigManager<GameConfig> GetManager(string path) => ConfigMaster<GameConfigManager<GameConfig>>.Get(path);
    }
    
    public abstract class GameConfig<T> : LocalDynamicConfig where T : GameConfig<T>, new()
    {
        public static T Get(string path) => GetManager(path).Config;
        protected static GameConfigManager<T> GetManager(string path) => ConfigMaster<GameConfigManager<T>>.Get(path);
    }
    
    public abstract class GameSingleConfig<T> : LocalDynamicConfig where T : GameSingleConfig<T>, new()
    {
        private static GameConfigManager<T> Manager => ConfigMaster<GameConfigManager<T>>.Default;
        public static T Config => Manager.Config;
    }
}