namespace LSCore.ConfigModule
{
    public class DebugConfigManager<T> : LocalDynamicConfigManager<T> where T : LocalDynamicConfig, new()
    {
        public new static DebugConfigManager<T> Get(string path) =>
            ConfigMaster<DebugConfigManager<T>>.Get(path); 
        
        protected override string GetFullPath(string relativePath)
        {
            return ConfigPaths.Game.Debug(relativePath);
        }
    }
}