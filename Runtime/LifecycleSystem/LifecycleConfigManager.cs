using LSCore.ConfigModule;

namespace LSCore.LifecycleSystem
{
    public class LifecycleConfigManager : JTokenGameConfigManager
    {
        public static string Path(string path) => ConfigPaths.Game.Dynamic(path);
        protected override string GetPath(string path) => Path(path);
    }
}