using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore.ConditionModule
{
    [Serializable]
    public class Conditions<T> : Condition, ISerializationCallbackReceiver, IList<T> where T : Condition
    {
        [SerializeReference] public List<T> conditions;
        [NonSerialized] public ConditionBuilder conditionBuilder;

        protected internal override bool Check() => conditionBuilder;

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            if(conditions == null) return;
            if (conditions.Count == 0) return;
            
            conditionBuilder = ConditionBuilder.If(conditions[0]);
            
            for (int i = 0; i < conditions.Count; i++)
            {
                if (conditions[i] != null)
                {
                    conditionBuilder.Add(conditions[i]);
                }
            }
        }

        public IEnumerator<T> GetEnumerator() => conditions.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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