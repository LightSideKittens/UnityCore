using System;
using System.Collections.Generic;
using LSCore;
using UnityEngine;

namespace LSCore.SDKManagement
{
    [Serializable]
    public partial class SDKInitializer : ISerializationCallbackReceiver
    {
        public event Action Completed;
        [SerializeReference] private List<Base> initializers = new();
        private Dictionary<Type, Base> initializersByType = new();
        private int initializedCount;

        public void Init()
        {
            for (int i = 0; i < initializers.Count; i++)
            {
                var initializer = initializers[i];

                if (initializer != null)
                {
                    initializers[i].TryInit(OnComplete);
                }
                else
                {
                    OnComplete();
                }
            }
        }

        public void Add(Base initializer)
        {
            initializers.Add(initializer);
            initializersByType.Add(initializer.GetType(), initializer);
        }

        private void OnComplete()
        {
            initializedCount++;

            if (initializedCount >= initializers.Count)
            {
                Completed?.Invoke();
            }
        }

        public bool TryGetInitializer<T>(out T initializer) where T : Base
        {
            var exist = initializersByType.TryGetValue(typeof(T), out var baseInitializer);
            initializer = (T) baseInitializer;
            return exist;
        } 

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if (World.IsPlaying)
            {
                for (int i = 0; i < initializers.Count; i++)
                {
                    var initializer = initializers[i];
                    initializersByType.Add(initializer.GetType(), initializer);
                }
            }
        }
    }
}