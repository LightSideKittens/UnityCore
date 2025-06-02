#if UNITY_EDITOR

namespace LSCore.ConfigModule
{
    public class EditorConfigManager<T> : LocalDynamicConfigManager<T> where T : LocalDynamicConfig, new()
    {
        public static EditorConfigManager<T> Get(string path) =>
            ConfigMaster<EditorConfigManager<T>>.Get(path); 
        
        protected override string GetFullPath(string relativePath)
        {
            return ConfigPaths.Editor.Library(relativePath);
        }
    }

    public class EditorConfigManager : EditorConfigManager<EditorConfig>
    {
        public new virtual void Save() => base.Save();
        public new EditorConfig Config => base.Config;
    }
}

#endif
