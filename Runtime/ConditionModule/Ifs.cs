using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.ConditionModule
{
    [Serializable]
    public class Ifs : Ifs<If> { }

    [Serializable]
    public class Ifs<T> : If, ISerializationCallbackReceiver where T : If
    {
        [OnValueChanged("OnAfterDeserialize", true)]
        [SerializeReference] public List<T> conditions;
        [NonSerialized] public IfBuilder ifBuilder;

        protected internal override bool Check() => ifBuilder;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            ifBuilder = IfBuilder.Default;
            
            if(conditions == null) return;
            if (conditions.Count == 0) return;
            
            ifBuilder = IfBuilder.If(conditions[0]);
            
            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i] != null)
                {
                    ifBuilder.Add(conditions[i]);
                }
            }
        }

        public IEnumerator<T> GetEnumerator() => conditions.GetEnumerator();

        public void Add(T item) => conditions.Add(item);
        public void Clear() => conditions.Clear();

        public bool Contains(T item) => conditions.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => conditions.CopyTo(array, arrayIndex);
        public bool Remove(T item) => conditions.Remove(item);
        public int Count => conditions.Count;
        public bool IsReadOnly => false;
        public int IndexOf(T item) => conditions.IndexOf(item);
        public void Insert(int index, T item) => conditions.Insert(index, item);

        public void RemoveAt(int index) => conditions.RemoveAt(index);

        public T this[int index]
        {
            get => conditions[index];
            set => conditions[index] = value;
        }
    }
}