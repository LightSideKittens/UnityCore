using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    public class LSToggleGroup : MonoBehaviour
    {
        [Serializable]
        public abstract class Handler
        {
            public abstract void OnValueChanged(bool value);
        }
        
        [SerializeField] private List<LSToggle> toggles;
        [SerializeReference] private Handler handler;

        public void Add(LSToggle toggle)
        {
            toggles.Add(toggle);
        }

        public void Remove(LSToggle toggle)
        {
            toggles.Remove(toggle);
        }

        public void RemoveAt(int index)
        {
            toggles.RemoveAt(index);
        }

        public void Clear()
        {
            toggles.Clear();
        }
    }
}