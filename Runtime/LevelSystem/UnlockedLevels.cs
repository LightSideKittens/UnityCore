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
        
        [JsonProperty("upgrades")] private List<UpgradeData> entityIdByUpgradesOrder = new();
        [JsonProperty("entitiesLevel")] private Dictionary<string, int> levelById = new();
        public static List<UpgradeData> IdByUpgradesOrder => Config.entityIdByUpgradesOrder;
        public static Dictionary<string, int> LevelById => Config.levelById;
        
        internal static void UpgradeLevel(LevelConfig levelConfig)
        {
            var data = (levelConfig.Id, levelConfig.Level);
            IdByUpgradesOrder.Add(new UpgradeData(){id = data.Id, level = data.Level});
            LevelById[data.Id] = data.Level;
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