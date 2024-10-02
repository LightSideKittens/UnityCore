using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class LootBoxAction : LSAction
    {
        [GenerateGuid] public string id;
        [SerializeReference] public List<LSAction> actions;
        
        [OnValueChanged("OnGuaranteedAtChanged")]
        public int guaranteedAt = -1;
        
        [OnValueChanged("OnChanceChanged")]
        [CustomValueDrawer("ChanceDrawer")] 
        public float chance;
        public float NormalizedChange => chance / 100;
        
        internal LootBox box;
        internal float maxValue;
        private void OnChanceChanged()
        {
            box.OnChanceChanged();
        }
        
        private void OnGuaranteedAtChanged()
        {
            box.OnGuaranteedAtChanged();
        }

        private float ChanceDrawer(float value, GUIContent label)
        {
            value = Mathf.Clamp(EditorGUILayout.Slider(label, value, 0, 100), 0, maxValue);
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
        [OnCollectionChanged("OnInspectorInit")]
        public List<LootBoxAction> actions;
        
        public override void Invoke()
        {
            var totalOpenings = LootBoxConfig.OnOpen(id);

            var guaranteed = actions.FirstOrDefault(x => x.guaranteedAt > 1 && x.guaranteedAt == totalOpenings);
            
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

            var picked = filteredActions.RandomElement();
            picked.Invoke();
            LootBoxConfig.OnActionPick(id, picked.id);
        }

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            actions ??= new();
            guaranteedSet.Clear();
            
            float maxValue = 100;
            foreach (var action in actions)
            {
                action.box = this;
                action.maxValue = maxValue;
                maxValue -= action.chance;
                if (action.guaranteedAt > 1)
                {
                    guaranteedSet.Add(action.guaranteedAt);
                }
            }
        }

        public void OnChanceChanged()
        {
            float maxValue = 100;
            int c = 0;
            
            foreach (var action in actions)
            {
                if (c > 0 && action.maxValue > 0)
                {
                    action.chance *= maxValue / action.maxValue;
                }

                c++;   
                action.maxValue = maxValue;
                maxValue -= action.chance;
            }
        }

        private HashSet<int> guaranteedSet = new();
        
        public void OnGuaranteedAtChanged()
        {
            foreach (var action in actions)
            {
                if (guaranteedSet.Contains(action.guaranteedAt))
                {
                    action.guaranteedAt++;
                }
            }
        }
    }
}