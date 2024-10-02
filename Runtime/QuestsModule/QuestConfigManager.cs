using LSCore.ConfigModule;

namespace LSCore.QuestModule
{
    public class QuestConfigManager : JTokenGameConfigManager
    {
        public static string Path(string path) => ConfigPaths.Game.Dynamic(path);
        protected override string GetPath(string path) => Path(path);
    }
}