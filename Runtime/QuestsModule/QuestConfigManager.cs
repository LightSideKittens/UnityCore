using LSCore.ConfigModule;

namespace LSCore.QuestModule
{
    public class QuestConfigManager : GameConfigManager
    {
        public static string Path(string path) => ConfigPaths.Game.Dynamic("Quests", path);
        protected override string GetPath(string path) => Path(path);
    }
}