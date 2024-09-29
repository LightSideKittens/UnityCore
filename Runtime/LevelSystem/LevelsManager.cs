using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.LevelSystem
{
    public partial class LevelsManager : SerializedScriptableObject
    {
        public event Action<Id, int> LevelChanged;
        private Dictionary<Id, Action<int>> LevelChangedById { get; } = new();
        
        [IdGroup]
        [OdinSerialize]
        [HideReferenceObjectPicker]
        public LevelIdGroup Group { get; private set; }
        
        [TableList, OdinSerialize, ValueDropdown("AvailableContainer", IsUniqueList = true)]
        [HideReferenceObjectPicker]
        private HashSet<LevelsContainer> levelsContainers = new();

        public IEnumerable<Id> Ids
        {
            get
            {
                foreach (var levelContainer in levelsContainers)
                {
                    var id = levelContainer.Id;
                    yield return id;
                }
            }
        }
        
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

        public void SubAndCallLevelChanged(Id id, Action<int> action)
        {
            LevelChangedById.TryGetValue(id, out var existAction);
            existAction += action;
            LevelChangedById[id] = existAction; 
            action(GetCurrentLevelNum(id));
        }
        
        public void UnSubLevelChanged(Id id, Action<int> action)
        {
            LevelChangedById.TryGetValue(id, out var existAction);
            existAction -= action;
            LevelChangedById[id] = existAction; 
        }

        private void OnSetLevel(Id id, int level)
        {
            UnlockedLevels.TryGetLevel(id, out var oldLevel);
            if (level != oldLevel)
            {
                LevelChanged?.Invoke(id, level);
                if (LevelChangedById.TryGetValue(id, out var action))
                {
                    action(level);
                }
            }
        }

        public void SetLevel(Id id, int level)
        {
            var levels = levelsById[id];
            
            level = level < levels.Count
                ? level
                : levels.Count;

            OnSetLevel(id, level);
            UnlockedLevels.SetLevel(id, level);
        }
        
        public void SetLevelForAll(int level)
        {
            foreach (var (id, levels) in levelsById)
            {
                var newLevel = level < levels.Count
                    ? level
                    : levels.Count;
                
                OnSetLevel(id, level);
                UnlockedLevels.SetLevel(id, newLevel);
            }
        }

        public void UpgradeLevel(Id id)
        {
            UnlockedLevels.TryGetLevel(id, out var currentLevel);
            var levels = levelsById[id];

            if (currentLevel < levels.Count)
            {
                currentLevel++;
                OnSetLevel(id, currentLevel);
                UnlockedLevels.SetLevel(id, currentLevel);
                Burger.Log($"{id} Upgraded to {currentLevel}");
            }
        }

        public T GetCurrentLevel<T>(Id id) where T : Object
        {
            UnlockedLevels.TryGetLevel(id, out var level);
            return GetLevel<T>(id, level);
        }
        
        public int GetCurrentLevelNum(Id id)
        {
            UnlockedLevels.TryGetLevel(id, out var level);
            return level;
        }

        public int GetMaxLevel(Id id) => levelsById[id].Count;
        
        public bool IsMaxLevel(Id id)
        {
            UnlockedLevels.TryGetLevel(id, out var level);
            return level >= levelsById[id].Count;
        }
        
        public bool IsUnlocked(Id id) => UnlockedLevels.TryGetLevel(id, out var level) && level > 0;

        public T GetLevel<T>(Id id, int level) where T : Object
        {
#if UNITY_EDITOR
            if (World.IsEditMode)
            {
                FillLevelsById();
            }
#endif
            var levels = levelsById[id];
            level = Mathf.Clamp(level, 1, levels.Count);
            return (T)levels[level - 1];
        }
        
#if UNITY_EDITOR
        private void FillLevelsById()
        {
            levelsById.Clear();
            
            foreach (var levelContainer in levelsContainers)
            {
                var id = levelContainer.Id;
                var levels = levelContainer.levels;
                
                levelsById.Add(id, levels);
            }
        }
#endif
    }
}