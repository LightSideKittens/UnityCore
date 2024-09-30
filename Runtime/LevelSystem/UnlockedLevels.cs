using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore.LevelSystem
{
    public class UnlockedLevels : GameSingleConfig<UnlockedLevels>
    {
        internal event Action<Id, int> LevelChanged;
        internal Dictionary<Id, Action<int>> LevelChangedById { get; } = new();
        
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

        internal static void SetLevel(Id id, int level)
        {
            TryGetLevel(id, out var oldLevel);
            Config.levelById[id] = level;
            if (level != oldLevel)
            {
                Config.LevelChanged?.Invoke(id, level);
                if (Config.LevelChangedById.TryGetValue(id, out var action))
                {
                    action(level);
                }
            }
        }

        internal static void ClearEvents()
        {
            Config.LevelChanged = null;
            Config.LevelChangedById.Clear();
        }
    }
}