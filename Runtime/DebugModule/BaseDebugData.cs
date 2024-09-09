using LSCore.ConfigModule;

namespace LSCore
{
    public abstract class BaseDebugData<T> : LocalDynamicConfig where T : BaseDebugData<T>, new()
    {
        public static T Config => Manager.Config;
        public static DebugConfigManager<T> Manager => GetManager(typeof(T).Name);
        protected static DebugConfigManager<T> GetManager(string path) => ConfigMaster<DebugConfigManager<T>>.Get(path);
    }
}