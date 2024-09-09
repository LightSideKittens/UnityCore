namespace LSCore.ConfigModule
{
    public static class ConfigMaster
    {
        public static void Load<T>(LocalDynamicConfigManager<T> manager) where T : LocalDynamicConfig, new()
        {
            manager.Load();
        }
        
        public static void Save<T>(LocalDynamicConfigManager<T> manager) where T : LocalDynamicConfig, new()
        {
            manager.Save();
        }
        
        public static void Delete<T>(LocalDynamicConfigManager<T> manager) where T : LocalDynamicConfig, new()
        {
            manager.Delete();
        }
    }
}