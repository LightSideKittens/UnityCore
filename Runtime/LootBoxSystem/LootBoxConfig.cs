using System.Collections.Generic;
using LSCore.ConfigModule;

namespace LSCore
{
    public class LootBoxConfig : GameSingleConfig<LootBoxConfig>
    {
        public Dictionary<string, int> totalOpenings = new();
        public Dictionary<string, Dictionary<string, int>> totalActionPickings = new();

        public static int OnOpen(string lootBoxId)
        {
            var totalOpenings = Config.totalOpenings;
            totalOpenings.TryGetValue(lootBoxId, out var count);
            totalOpenings[lootBoxId] = ++count;
            return count;
        }
        
        public static int OnActionPick(string lootBoxId, string actionId)
        {
            var totalOpenings = Config.totalActionPickings;
            
            if (!totalOpenings.TryGetValue(lootBoxId, out var picks))
            {
                picks = new();
                totalOpenings[lootBoxId] = picks;
            }
            
            picks.TryGetValue(actionId, out var count);
            picks[actionId] = ++count;
            return count;
        }

        public static bool TryGetOpeningsCount(string lootBoxId, out int count)
        {
            return Config.totalOpenings.TryGetValue(lootBoxId, out count);
        }

        public static bool TryGetActionPickingsCount(string lootBoxId, string actionId, out int count)
        {
            var totalActionPickings = Config.totalActionPickings;
            
            if (totalActionPickings.TryGetValue(lootBoxId, out var picks))
            {
                return picks.TryGetValue(actionId, out count);
            }

            count = 0;
            return false;
        }
    }
}