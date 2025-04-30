using System;
using System.Collections;
using System.Collections.Generic;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using System.Reflection;
using Sirenix.OdinInspector.Editor;
#endif

namespace LSCore.DataStructs
{
#if UNITY_EDITOR
    [ResolverPriority(100)]
    public class UniDictResolver : ProcessedMemberPropertyResolver<IUniDict>
    {
        public override bool IsCollection => false;

        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            bool includeSpeciallySerializedMembers = !Property.ValueEntry.SerializationBackend.IsUnity;
            var type = Property.ValueEntry.WeakSmartValue.GetType();
            List<InspectorPropertyInfo> memberProperties = InspectorPropertyInfoUtility.CreateMemberProperties(Property, type, includeSpeciallySerializedMembers);
            return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(Property, type, memberProperties, includeSpeciallySerializedMembers);
        }
    }
#endif
    
    public interface IUniDict{}
    
    [Serializable]
    [Unwrap]
    public class UniDict<TKey, TValue> : ISerializationCallbackReceiver, IDictionary<TKey, TValue>, IUniDict
    {
        [Serializable]
        public class Data
        {
            [InfoBox("$keyError", InfoMessageType.Error, "$KeyErrorExist")]
            public TKey key;
            [InfoBox("$valueError", InfoMessageType.Error, "$ValueErrorExist")]
            public TValue value;

#if UNITY_EDITOR
            internal string keyError;
            internal bool KeyErrorExist => !string.IsNullOrEmpty(keyError);
            internal string valueError;
            internal bool ValueErrorExist => !string.IsNullOrEmpty(valueError);
#endif
        }
        
        [OnValueChanged("OnValueChanged", true)]
        [SerializeField] private List<Data> data;
        
        private Dictionary<TKey, TValue> dict = new();
        private ICollection<KeyValuePair<TKey, TValue>> DictCollection => dict;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            dict = new Dictionary<TKey, TValue>();

            for (int i = 0; i < data.Count; i++)
            {
                var d = data[i];
                dict.TryAdd(d.key, d.value);
            }
#if UNITY_EDITOR
            OnValueChanged();
#endif
        } 

#if UNITY_EDITOR

        public void Editor_ApplyToData()
        {
            data ??= new List<Data>();
            data.Clear();
            
            foreach (var (key, value) in dict)
            {
                data.Add(new Data() { key = key, value = value });
            }
        }
        
        private void OnValueChanged()
        {
            var set = new HashSet<TKey>();

            for (var i = 0; i < data.Count; i++)
            {
                var d = data[i];
                d.keyError = string.Empty;
                d.valueError = string.Empty;
                
                if (!set.Add(d.key))
                {
                    d.keyError = "Key already exists";
                }
                
                if (d.key == null || d.key.Equals(null))
                {
                    d.keyError = "Key is null";
                }

                if (d.value == null || d.value.Equals(null))
                {
                    d.valueError = "Value is null";
                }

                data[i] = d;
            }
        }
        
        protected static UniDict<TKey, TValue> currentInspected;

        [OnInspectorInit] private void OnInit() => OnGui();
        [OnInspectorGUI] private void OnGui() => currentInspected = this;

        private void ProcessChildMemberAttributes(MemberInfo member, List<Attribute> attributes)
        {
            switch (member.Name)
            {
                case "key":
                    OnKeyProcessAttributes(attributes);
                    break;
                case "value":
                    OnValueProcessAttributes(attributes);
                    break;
            }
        }

        protected virtual void OnKeyProcessAttributes(List<Attribute> attributes) { }
        
        protected virtual void OnValueProcessAttributes(List<Attribute> attributes) { }
        
        private class EntryAttributeProcessor : OdinAttributeProcessor<Data>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
            {
                currentInspected.ProcessChildMemberAttributes(member, attributes);
            }
        }
#endif
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => dict.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => DictCollection.Add(item);

        public void Clear() => dict.Clear();

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => DictCollection.Contains(item);

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => DictCollection.CopyTo(array, arrayIndex);

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => DictCollection.Remove(item);

        public int Count => dict.Count;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => DictCollection.IsReadOnly;
        
        public void Add(TKey key, TValue value) => dict.Add(key, value);

        public bool ContainsKey(TKey key) => dict.ContainsKey(key);

        public bool Remove(TKey key) => dict.Remove(key);

        public bool TryGetValue(TKey key, out TValue value) => dict.TryGetValue(key, out value);

        public TValue this[TKey key]
        {
            get => dict[key];
            set => dict[key] = value;
        }

        public ICollection<TKey> Keys => dict.Keys;
        public ICollection<TValue> Values => dict.Values;
    }
}