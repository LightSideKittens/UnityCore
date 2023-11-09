using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore.LevelSystem
{
    public class UnlockedLevels : BaseConfig<UnlockedLevels>
    {
        [Serializable]
        [JsonConverter(typeof(UpgradeDataConverter))]
        public struct UpgradeData
        {
            public string id;
            public int level;
            public override string ToString()
            {
                return $"{id}_{level}";
            }
        }
        
        [JsonProperty("upgrades")] private Dictionary<string, List<UpgradeData>> upgradesByGroupName = new();
        [JsonProperty("entitiesLevel")] private Dictionary<string, int> levelById = new();
        
        public static bool TryGetUpgrades(LevelIdGroup group, out List<UpgradeData> upgrades)
        {
            return Config.upgradesByGroupName.TryGetValue(group.name, out upgrades);
        }
        
        public static bool TryGetLevel(Id id, out int level)
        {
            return Config.levelById.TryGetValue(id, out level);
        }
        
        internal static void UpgradeLevel(LevelIdGroup group, LevelConfig levelConfig)
        {
            var data = (levelConfig.Id, levelConfig.Level);
            
            if (!TryGetUpgrades(group, out var upgrades))
            {
                upgrades = new List<UpgradeData>();
                Config.upgradesByGroupName.Add(group.name, upgrades);
            }
            
            upgrades.Add(new UpgradeData(){id = data.Id, level = data.Level});
            
            Config.levelById[data.Id] = data.Level;
        }
        
        private class UpgradeDataConverter : JsonConverter<UpgradeData>
        {
            public override UpgradeData ReadJson(JsonReader reader, Type objectType, UpgradeData existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                if (hasExistingValue)
                {
                    return existingValue;
                }

                var value = (string)reader.Value;
                if (!string.IsNullOrEmpty(value))
                {
                    var splited = value.Split('_');
                    existingValue.id = splited[0];
                    existingValue.level = int.Parse(splited[1]);
                }

                return existingValue;
            }

            public override void WriteJson(JsonWriter writer, UpgradeData value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value.ToString());
            }
        }
    }
}