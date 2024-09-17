using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    public class LSToggleGroup : MonoBehaviour
    {
        [Serializable]
        public abstract class Handler : ISerializationCallbackReceiver
        {
            [SerializeField] protected List<LSToggle> toggles;
            protected List<Action<bool>> eventHandlers = new();
            
            public void AddRange(IEnumerable<LSToggle> collection)
            {
                foreach (var toggle in collection)
                {
                    Add(toggle);
                }
            }
            
            public void Add(LSToggle toggle)
            {
                var index = toggles.Count;
                Action<bool> eventHandler = value => OnValueChanged(index, value);
                
                toggle.CallAndSub(eventHandler);
                eventHandlers.Add(eventHandler);
                toggles.Add(toggle);
            }

            public bool Remove(LSToggle toggle)
            {
                var index = toggles.IndexOf(toggle);
                if (index < 0) return false;
                RemoveAt(index);
                return true;
            }

            public void RemoveAt(int index)
            {
                toggles[index].ValueChanged -= eventHandlers[index];
                eventHandlers.RemoveAt(index);
                toggles.RemoveAt(index);
            }

            public void Clear()
            {
                for (int i = 0; i < toggles.Count; i++)
                {
                    toggles[i].ValueChanged -= eventHandlers[i];
                }
                
                eventHandlers.Clear();
                toggles.Clear();
            }
            
            public abstract void OnValueChanged(int index, bool value);
            
            void ISerializationCallbackReceiver.OnBeforeSerialize() {}

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                var list = new List<LSToggle>(toggles);
                Clear();
                AddRange(list);
            }
        }

        [Serializable]
        public class OnlyOne : Handler
        {
            [MinMaxSlider(0, "MaxRange")] public Vector2Int countCanBeActive;
            
            private int CurrentActive => toggles.Count(x => x.IsOn);
            private int lastActive = -1;
            private int MaxRange => toggles.Count;
            
            public override void OnValueChanged(int index, bool value)
            {
                if (value)
                {
                    if (CurrentActive > countCanBeActive.y)
                    {
                        if (lastActive > 0 && lastActive < toggles.Count)
                        {
                            toggles[lastActive].IsOn = false;
                        }
                    }
                }
            }
        }
        
        [SerializeReference] private Handler handler;
        
        public void Add(LSToggle toggle) => handler.Add(toggle);
        public void Remove(LSToggle toggle) => handler.Remove(toggle);
        public void RemoveAt(int index) => handler.RemoveAt(index);
        public void Clear() => handler.Clear();
    }
}