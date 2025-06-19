﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace LSCore
{
    public abstract class ValueByKey<TKey, TValue> : SerializedScriptableObject
    {
        [Serializable]
        protected class Entry
        {
            [HideInInspector] 
            public TKey key;
            public TValue value;
            
#if UNITY_EDITOR
            public Entry()
            {
                var type = typeof(TValue);
                if (!typeof(Object).IsAssignableFrom(type))
                {
                    value = Activator.CreateInstance<TValue>();
                }
            }
#endif
            
            public override bool Equals(object obj)
            {
                if (obj is Entry drawer)
                {
                    return Equals(drawer);
                }
                
                return false;
            }
        
            public bool Equals(Entry other) => key.Equals(other.key);

            public override int GetHashCode() => key.GetHashCode();
        }
        
        [ValueDropdown("DataSelector", IsUniqueList = true)]
        [OdinSerialize]
        private HashSet<Entry> byKey = new();

        private Dictionary<TKey, TValue> byKeyDict = new();

        public Dictionary<TKey, TValue> ByKey
        {
            get
            {
                Init();
                return byKeyDict;
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                Init();
                return byKeyDict.Keys;
            }
        }
        
        public IEnumerable<TValue> Values
        {
            get
            {
                Init();
                return byKeyDict.Values;
            }
        }
        
        public TValue this[TKey key]
        {
            get
            {
                Init();
                return byKeyDict[key];
            }
        }
        
        [NonSerialized] private bool isInited;

        private void OnDestroy()
        {
            World.Created -= OnCreated;
            World.Destroyed -= OnCreated;
        }
        
        private void OnCreated()
        {
            World.Created -= OnCreated;
            World.Destroyed -= OnCreated;
            isInited = false;
        }

        public void Init()
        {
            if (isInited) return;
            World.Created += OnCreated;
            World.Destroyed += OnCreated;
            
            byKeyDict.Clear();

            foreach (var entry in byKey)
            {
                byKeyDict.Add(entry.key, entry.value);
            }

            isInited = true;
        }

#if UNITY_EDITOR
        
        private ValueDropdownList<Entry> list;
        private IList<ValueDropdownItem<Entry>> DataSelector
        {
            get
            {
                list ??= new ValueDropdownList<Entry>();
                list.Clear();
                SetupDataSelector(list);
                return list;
            }
        }

        protected abstract void SetupDataSelector(ValueDropdownList<Entry> list);

        protected static ValueByKey<TKey, TValue> currentInspected;

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
        
        private class EntryAttributeProcessor : OdinAttributeProcessor<Entry>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
            {
                currentInspected.ProcessChildMemberAttributes(member, attributes);
            }
        }
#endif
    }
}