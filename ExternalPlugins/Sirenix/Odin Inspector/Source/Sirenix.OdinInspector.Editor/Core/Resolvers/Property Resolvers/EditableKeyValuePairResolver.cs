//-----------------------------------------------------------------------
// <copyright file="EditableKeyValuePairResolver.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.Serialization;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Collections.Generic;

    [ResolverPriority(-1)]
    public class EditableKeyValuePairResolver<TKey, TValue> : OdinPropertyResolver<EditableKeyValuePair<TKey, TValue>>, IHasSpecialPropertyPaths, IMaySupportPrefabModifications
    {
        private static Dictionary<SerializationBackend, InspectorPropertyInfo[]> ChildInfos = new Dictionary<SerializationBackend, InspectorPropertyInfo[]>();
        private SerializationBackend backend;

        public bool MaySupportPrefabModifications { get { return DictionaryKeyUtility.KeyTypeSupportsPersistentPaths(typeof(TKey)); } }

        public string GetSpecialChildPath(int childIndex)
        {
            if (this.Property.Parent != null && this.Property.Parent.ChildResolver is IKeyValueMapResolver)
            {
                var key = this.ValueEntry.SmartValue.Key;
                var keyStr = DictionaryKeyUtility.GetDictionaryKeyString(key);

                if (childIndex == 0) // Key
                {
                    return this.Property.Parent.Path + "." + keyStr + "#key";
                }
                else if (childIndex == 1) // Value
                {
                    return this.Property.Parent.Path + "." + keyStr;
                }
            }
            else
            {
                if (childIndex == 0)
                {
                    return this.Property.Path + ".Key";
                }
                else if (childIndex == 1)
                {
                    return this.Property.Path + ".Value";
                }
            }

            throw new ArgumentOutOfRangeException();
        }

        protected override void Initialize()
        {
            this.backend = this.Property.ValueEntry.SerializationBackend;

            if (!ChildInfos.ContainsKey(backend))
            {
                ChildInfos[backend] = InspectorPropertyInfoUtility.GetDefaultPropertiesForType(this.Property, typeof(EditableKeyValuePair<TKey, TValue>), false);
            }
        }

        public override InspectorPropertyInfo GetChildInfo(int childIndex)
        {
            return ChildInfos[this.backend][childIndex];
        }

        protected override int GetChildCount(EditableKeyValuePair<TKey, TValue> value)
        {
            return ChildInfos[this.backend].Length;
        }

        public override int ChildNameToIndex(string name)
        {
            StringSlice slice = name;
            return this.ChildNameToIndex(ref slice);
        }

        public override int ChildNameToIndex(ref StringSlice name)
        {
            if (name == "Key") return 0;
            if (name == "Value") return 1;
            if (name == "#Value") return 1;
            return -1;
        }
    }
}
#endif