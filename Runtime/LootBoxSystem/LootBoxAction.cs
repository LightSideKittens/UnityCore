using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LSCore
{
    [Serializable]
    public class LootBoxAction : LSAction
    {
        [GenerateGuid] public string id;
        [SerializeReference] public List<LSAction> actions;
        
        public int guaranteedAt = -1;
        [CustomValueDrawer("ChanceDrawer")] 
        public float chance;
        public float NormalizedChange => chance / 100;
        
        internal float minValue;
        internal float maxValue = 100;
        

        private float ChanceDrawer(float value, GUIContent label)
        {
            value = Mathf.Clamp(EditorGUILayout.Slider(label, value, 0, 100), minValue, maxValue);
            return value;
        }

        public override void Invoke()
        {
            actions.Invoke();
        }
    }
    
    
    [Serializable]
    public class LootBox : LSAction
    {
        [GenerateGuid] public string id;
        [SerializeReference]
        public List<LootBoxAction> actions;
        
        [Button]
        public override void Invoke()
        {
            var totalOpenings = LootBoxConfig.OnOpen(id);

            var guaranteed = actions.FirstOrDefault(x => x.guaranteedAt > 1 && totalOpenings % x.guaranteedAt == 0);
            
            if (guaranteed != null)
            {
                guaranteed.Invoke();
                LootBoxConfig.OnActionPick(id, guaranteed.id);
                return;
            }
            
            var filteredActions = actions.Where(x =>
            {
                LootBoxConfig.TryGetActionPickingsCount(id, x.id, out var count);
                var ratio = count / ((float)totalOpenings);
                return x.NormalizedChange >= ratio;
            });

            var picked = GetRandomItem(filteredActions.ToList());
            picked.Invoke();
            LootBoxConfig.OnActionPick(id, picked.id);
        }
        
        public LootBoxAction GetRandomItem(List<LootBoxAction> picked)
        {
            var randomValue = Random.Range(0f, 100f);
            var cumulativeProbability = 0f;
            
            foreach (var action in picked)
            {
                cumulativeProbability += action.chance;
                if (randomValue <= cumulativeProbability)
                {
                    return action;
                }
            }
            
            return picked[^1];
        }

        private bool isInited;
        
        [OnInspectorGUI]
        private void OnInspectorGui()
        {
            if(World.IsPlaying) return;
            actions ??= new();
            
            if (!isInited)
            {
                float maxValue = 100;
                foreach (var action in actions)
                {
                    action.minValue = 0;
                    action.maxValue = maxValue;
                    maxValue -= action.chance;
                }
            }
            
            OnChanceChanged();
            OnGuaranteedAtChanged();
        }
        
        public void OnChanceChanged()
        {
            float maxValue = 100;
            int i = 0;
            int count = actions.Count - 1;
            
            foreach (var action in actions)
            {
                action.minValue = 0;
                
                if (i > 0 && action.maxValue > 0 && i < count)
                {
                    action.chance *= maxValue / action.maxValue;
                }
                else if(i == count)
                {
                    action.minValue = maxValue;
                    action.maxValue = maxValue;
                    action.chance = maxValue;
                    break;
                }

                i++;   
                action.maxValue = maxValue;
                maxValue -= action.chance;
            }
        }

        private HashSet<int> guaranteedSet = new();
        private HashSet<string> idSet = new();
        
        public void OnGuaranteedAtChanged()
        {
            guaranteedSet.Clear();
            idSet.Clear();
            
            foreach (var action in actions)
            {
                if (guaranteedSet.Contains(action.guaranteedAt))
                {
                    action.guaranteedAt++;
                }

                if (action.guaranteedAt > 1)
                {
                    guaranteedSet.Add(action.guaranteedAt);
                }
                
                if (idSet.Contains(action.id))
                {
                    action.id = Guid.NewGuid().ToString("N");
                }
                
                idSet.Add(action.id);
            }
        }
    }
}