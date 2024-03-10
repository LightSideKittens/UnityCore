using System;

namespace LSCore.ConfigModule
{
    [Serializable]
    public abstract class BaseConfig<T> : BaseConfig where T : BaseConfig, new()
    {
        static BaseConfig()
        {
            foreach (var action in onStaticConstructor)
            {
                action();
            }
        }
        
        public static T Get(string name, bool isAutoSave = true) => ByName<T>.Get(name, isAutoSave);
        public static void LoadOnNextAccess(string name) => ByName<T>.LoadOnNextAccess(name);
    }
}