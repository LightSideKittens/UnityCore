using System;

namespace LSCore.ConfigModule
{
    [Serializable]
    public abstract class Config<T> : BaseConfig where T : BaseConfig, new()
    {
        public static T Get(string name, bool isAutoSave = true) => ByName<T>.Get(name, isAutoSave);
        public static void LoadOnNextAccess(string name) => ByName<T>.LoadOnNextAccess(name);
    }
}