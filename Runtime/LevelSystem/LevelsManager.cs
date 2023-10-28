using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Battle.Data
{
    public partial class LevelsManager : SerializedScriptableObject
    {
        public event Action LevelUpgraded;

        [ValueDropdown("AvailableGroups", IsUniqueList = true)]
        [OdinSerialize]
        [HideReferenceObjectPicker]
        public HashSet<IdGroup> Groups { get; private set; } = new();
        
        [TableList, OdinSerialize, ValueDropdown("AvailableContainer", IsUniqueList = true)]
        [HideReferenceObjectPicker]
        private HashSet<LevelsContainer> levelsContainers = new();
        
        private readonly Dictionary<string, List<LevelConfig>> levelsById = new();

        public void Init()
        {
            Burger.Log($"[{nameof(LevelsManager)}] Init");
            levelsById.Clear();

            foreach (var levelContainer in levelsContainers)
            {
                levelsById.Add(levelContainer.Id, levelContainer.levels);
            }

            RecomputeAllLevels();
        }

        public bool CanUpgrade(Id id)
        {
            UnlockedLevels.LevelById.TryGetValue(id, out var currentLevel);
            var levels = levelsById[id];
            return currentLevel < levels.Count;
        }

        public void UpgradeLevel(Id id)
        {
            if (CanUpgrade(id))
            {
                var levelById = UnlockedLevels.LevelById;
                levelById.TryGetValue(id, out var currentLevel);
                var level = levelsById[id][currentLevel];
                
                level.Apply();
                UnlockedLevels.UpgradeLevel(level);
                
                LevelUpgraded?.Invoke();
                Burger.Log($"{id} Upgraded to {currentLevel}");
            }
        }

        private void RecomputeAllLevels()
        {
            Burger.Log($"[{nameof(LevelsManager)}] RecomputeAllLevels");
            EntiProps.Clear();

            var entityIds = UnlockedLevels.IdByUpgradesOrder;
            
            for (int i = 0; i < entityIds.Count; i++)
            {
                var data = entityIds[i];
                var level = levelsById[data.id][data.level-1];
                level.Apply();
            }
        }
    }
}