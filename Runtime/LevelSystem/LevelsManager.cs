using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace LSCore.LevelSystem
{
    public partial class LevelsManager : SerializedScriptableObject
    {
        public event Action LevelUpgraded;

        [IdGroup]
        [OdinSerialize]
        [HideReferenceObjectPicker]
        public LevelIdGroup Group { get; private set; }
        
        [TableList, OdinSerialize, ValueDropdown("AvailableContainer", IsUniqueList = true)]
        [HideReferenceObjectPicker]
        private HashSet<LevelsContainer> levelsContainers = new();
        
        [IdGroup]
        [OdinSerialize]
        [HideReferenceObjectPicker]
        public CurrencyIdGroup CurrencyGroup { get; private set; }
        
        private readonly Dictionary<Id, List<LevelConfig>> levelsById = new();

        public void Init()
        {
            Burger.Log($"[{nameof(LevelsManager)}] Init");
            levelsById.Clear();

            foreach (var levelContainer in levelsContainers)
            {
                levelsById.Add(levelContainer.Id, levelContainer.levels);
            }
        }

        public bool CanUpgrade(Id id, out LevelConfig level)
        {
            UnlockedLevels.TryGetLevel(id, out var currentLevel);
            var levels = levelsById[id];
            if (currentLevel < levels.Count)
            {
                level = levels[currentLevel];
                return true;
            }

            level = null;
            return false;
        }

        public bool TryUpgradeLevel(Id id)
        {
            if (CanUpgrade(id, out var level))
            {
                UnlockedLevels.UpgradeLevel(id);
                
                LevelUpgraded?.Invoke();
                Burger.Log($"{id} Upgraded to {level.Id}");
                return true;
            }

            return false;
        }

        public Dictionary<Type, Prop> GetProps(Id id)
        {
            UnlockedLevels.TryGetLevel(id, out var level);
            return levelsById[id][level - 1].Props.ByType;
        }
    }
}