#if UNITY_EDITOR
namespace LSCore.ConfigModule
{
    public class EditorConfigManager<T> : LocalDynamicConfigManager<T> where T : LocalDynamicConfig, new()
    {
        public new static EditorConfigManager<T> Get(string path) =>
            ConfigMaster<EditorConfigManager<T>>.Get(path); 
        
        protected override string GetPath(string path)
        {
            return ConfigPaths.Editor.Library(path);
        }
    }
}
#endif
