using System.Collections.Generic;
using LSCore.ConfigModule.Old;
using Newtonsoft.Json;

namespace LSCore.LevelSystem
{
    public class UnlockedLevels : BaseSingleConfig<UnlockedLevels>
    {
        [JsonProperty("entitiesLevel")] private Dictionary<string, int> levelById = new();
        
        public static bool TryGetLevel(Id id, out int level)
        {
            return Config.levelById.TryGetValue(id, out level);
        }
        
        internal static int UpgradeLevel(Id id)
        {
            TryGetLevel(id, out var level);
            level++;
            SetLevel(id, level);
            return level;
        }

        public static void SetLevel(Id id, int level)
        {
            Config.levelById[id] = level;
        }
    }
}