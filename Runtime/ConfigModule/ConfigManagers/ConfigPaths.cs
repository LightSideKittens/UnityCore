using static System.IO.Path;

namespace LSCore.ConfigModule
{
    public static class ConfigPaths
    {
        private const string Root = "Configs";
        
        public static class Game
        {
            public static string Dynamic(string path, params string[] paths)
            {
                return Combine(Root, "DynamicData", path, Combine(paths));
            }
        }
    }
}