using System;
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif

namespace LSCore
{
    public abstract class ValuesByKeys<TKey, TValue> : SerializedScriptableObject
    {
        [Serializable]
        protected class Data
        {
            [HideInInspector] 
            public TKey key;
            public TValue value;
            
            public override bool Equals(object obj)
            {
                if (obj is Data drawer)
                {
                    return Equals(drawer);
                }
                
                return false;
            }
        
            public bool Equals(Data other) => key.Equals(other.key);

            public override int GetHashCode() => key.GetHashCode();
        }

        [HideReferenceObjectPicker]
        [ValueDropdown("DataSelector", IsUniqueList = true)]
        [OdinSerialize]
        private HashSet<Data> byKey = new();

        public Dictionary<TKey, TValue> ByKey { get; } = new();


        protected override void OnAfterDeserialize()
        {
            ByKey.Clear();

            foreach (var data in byKey)
            {
                ByKey.Add(data.key, data.value);
            }
        }

#if UNITY_EDITOR
        private ValueDropdownList<Data> list;
        private IList<ValueDropdownItem<Data>> DataSelector
        {
            get
            {
                list ??= new ValueDropdownList<Data>();
                list.Clear();
                SetupDataSelector(list);
                return list;
            }
        }

        protected abstract void SetupDataSelector(ValueDropdownList<Data> list);

        protected static ValuesByKeys<TKey, TValue> currentInspected;

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
        
        private class DataAttributeProcessor : OdinAttributeProcessor<Data>
        {
            public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
            {
                currentInspected.ProcessChildMemberAttributes(member, attributes);
            }
        }
#endif
    }
}