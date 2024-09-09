namespace LSCore.ConfigModule
{
    public class GameConfigManager<T> : AutoSaveConfigManager<T> where T : LocalDynamicConfig, new()
    {
        public new static GameConfigManager<T> Get(string path) =>
            ConfigMaster<GameConfigManager<T>>.Get(path); 
        
        protected override string GetPath(string path)
        {
            return ConfigPaths.Game.Dynamic(path);
        }
    }
}