using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace LSCore.LevelSystem
{
    public partial class LevelsManager : SerializedScriptableObject
    {
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
                var id = levelContainer.Id;
                var levels = levelContainer.levels;
                
                levelsById.Add(id, levels);
                
                if (UnlockedLevels.TryGetLevel(id, out var level))
                {
                    level = Mathf.Clamp(level, 1, levelsById.Count);
                    UnlockedLevels.SetLevel(id, level);
                }
            }
        }

        public bool CanUpgrade(Id id)
        {
            UnlockedLevels.TryGetLevel(id, out var currentLevel);
            return currentLevel < levelsById[id].Count;
        }

        public void SetLevel(Id id, int level)
        {
            var levels = levelsById[id];
            UnlockedLevels.SetLevel(id, 
                level < levels.Count 
                ? level 
                : levels.Count);
        }

        public int UpgradeLevel(Id id)
        {
            UnlockedLevels.TryGetLevel(id, out var currentLevel);
            var levels = levelsById[id];

            if (currentLevel < levels.Count)
            {
                currentLevel++;
                UnlockedLevels.SetLevel(id, currentLevel);
                Burger.Log($"{id} Upgraded to {currentLevel}");
            }

            return currentLevel;
            
        }

        public Dictionary<Type, Prop> GetProps(Id id)
        {
            UnlockedLevels.TryGetLevel(id, out var level);
            level = Mathf.Clamp(level, 1, levelsById.Count);
            return levelsById[id][level - 1].Props.ByType;
        }
    }
}