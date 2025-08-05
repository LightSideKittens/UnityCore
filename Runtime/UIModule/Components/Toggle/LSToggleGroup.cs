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
            protected List<bool> toggleStates = new();
            protected int CurrentActive => toggleStates.Count(value => value);
            
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
                toggleStates.Add(false);
                
                Action<bool> eventHandler = value =>
                {
                    toggleStates[index] = value;
                    OnValueChanged(index, value);
                };
                
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
                toggleStates.RemoveAt(index);
            }

            public void Clear()
            {
                if (eventHandlers.Count == 0)
                {
                    goto ret;
                }
                
                for (int i = 0; i < toggles.Count; i++)
                {
                    toggles[i].ValueChanged -= eventHandlers[i];
                }

                ret:
                eventHandlers.Clear();
                toggles.Clear();
                toggleStates.Clear();
            }
            
            public abstract void OnValueChanged(int index, bool value);
            
            void ISerializationCallbackReceiver.OnBeforeSerialize() {}

            void ISerializationCallbackReceiver.OnAfterDeserialize()
            {
                if(World.IsEditMode) return;
                World.Updated += OnUpdated;

                void OnUpdated()
                {
                    World.Updated -= OnUpdated;
                    var list = new List<LSToggle>(toggles);
                    Clear();
                    AddRange(list);
                }
            }
        }

        [Serializable]
        public class OnlyOne : Handler
        {
            [PropertyRange(0, "MaxRange")] public int countCanBeActive;
            private int lastActive = -1;
            private int MaxRange => toggles.Count;
            
            public override void OnValueChanged(int index, bool value)
            {
                if (value)
                {
                    if (CurrentActive > countCanBeActive)
                    {
                        if (lastActive >= 0 && lastActive < toggles.Count)
                        {
                            toggles[lastActive].IsOn = false;
                        }
                    }

                    lastActive = index;
                }
                else
                {
                    if (CurrentActive == 0)
                    {
                        lastActive = -1;
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