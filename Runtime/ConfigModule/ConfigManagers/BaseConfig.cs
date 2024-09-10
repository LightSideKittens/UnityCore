namespace LSCore.ConfigModule
{
    public abstract class BaseConfig
    {
        protected internal virtual void OnDeserializing(){}
        protected internal virtual void OnDeserialized(){}
        protected internal virtual void OnSerializing(){}
        protected internal virtual void OnSerialized(){}
    }
    
    public abstract class LocalDynamicConfig : BaseConfig
    {
        protected internal virtual void AddMigrations(){}
        protected internal virtual void SetDefault(){}
        protected internal virtual void OnDeleting() {}
        protected internal virtual void OnDeleted() {}
    }
    
    public abstract class GameConfig<T> : LocalDynamicConfig where T : GameConfig<T>, new()
    {
        public static T Get(string path) => GetManager(path).Config;
        protected static GameConfigManager<T> GetManager(string path) => ConfigMaster<GameConfigManager<T>>.Get(path);
    }
    
    public abstract class GameSingleConfig<T> : LocalDynamicConfig where T : GameSingleConfig<T>, new()
    {
        private static readonly GameConfigManager<T> manager = ConfigMaster<GameConfigManager<T>>.Get(typeof(T).Name);
        public static T Config => manager.Config;
    }
}