using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

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

        public HashSet<Id> AddedIds { get; private set; }

        [IdGroup]
        [OdinSerialize]
        [HideReferenceObjectPicker]
        public CurrencyIdGroup CurrencyGroup { get; private set; }
        
        private readonly Dictionary<Id, List<Object>> levelsById = new();

        public void Init()
        {
            Burger.Log($"[{nameof(LevelsManager)}] Init");
            levelsById.Clear();
            AddedIds = new();
            
            foreach (var levelContainer in levelsContainers)
            {
                var id = levelContainer.Id;
                AddedIds.Add(id);
                var levels = levelContainer.levels;
                
                levelsById.Add(id, levels);

                if (UnlockedLevels.TryGetLevel(id, out var level))
                {
                    level = Mathf.Clamp(level, 1, levels.Count);
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
        
        public void SetLevel(int level)
        {
            foreach (var (id, levels) in levelsById)
            {
                UnlockedLevels.SetLevel(id, 
                    level < levels.Count 
                        ? level 
                        : levels.Count);
            }
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

        public T GetCurrentLevel<T>(Id id) where T : Object
        {
            UnlockedLevels.TryGetLevel(id, out var level);
            return GetLevel<T>(id, level);
        }
        
        public T GetLevel<T>(Id id, int level) where T : Object
        {
            var levels = levelsById[id];
            level = Mathf.Clamp(level, 1, levels.Count);
            return (T)levels[level - 1];
        }
    }
}