using LSCore.ConfigModule;

namespace LSCore.ObjectModule
{
    public class LifecycleConfigManager<T> : JTokenGameConfigManager
    {
        public static string Path(string path) => ConfigPaths.Game.Dynamic(ConfigPathAttribute.Path<T>(), path);
        protected override string GetPath(string path) => Path(path);
    }
}