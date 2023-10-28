using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;
using UnityEngine.Serialization;

namespace Battle.Data
{
    public class UnlockedLevels : BaseConfig<UnlockedLevels>
    {
        [Serializable]
        public struct UpgradeData
        {
            public string id;
            public int level;
        }
        
        [JsonProperty("upgrades")] private List<UpgradeData> entityIdByUpgradesOrder = new();
        [JsonProperty("entitiesLevel")] private Dictionary<string, int> levelById = new();
        public static List<UpgradeData> IdByUpgradesOrder => Config.entityIdByUpgradesOrder;
        public static Dictionary<string, int> LevelById => Config.levelById;
        
        public static void UpgradeLevel(LevelConfig levelConfig)
        {
            var data = (levelConfig.Id, levelConfig.Level);
            IdByUpgradesOrder.Add(new UpgradeData(){id = data.Id, level = data.Level});
            LevelById[data.Id] = data.Level;
        }
    }
}