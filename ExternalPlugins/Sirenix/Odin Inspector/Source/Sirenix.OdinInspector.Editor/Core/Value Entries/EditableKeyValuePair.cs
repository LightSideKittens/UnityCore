//-----------------------------------------------------------------------
// <copyright file="EditableKeyValuePair.cs" company="Sirenix ApS">
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
    using System;
    using UnityEngine;

    public struct EditableKeyValuePair<TKey, TValue> : IEquatable<EditableKeyValuePair<TKey, TValue>>
    {
        [Space(2)]
        [OdinSerialize, Delayed, DisableContextMenu, ShowInInspector, SuppressInvalidAttributeError, DoesNotSupportPrefabModifications]
        public TKey Key;

        [OdinSerialize, ShowInInspector, OmitFromPrefabModificationPaths]
        public TValue Value;

        [NonSerialized]
        public bool IsTempKey;

        [NonSerialized]
        public bool IsInvalidKey;

        public EditableKeyValuePair(TKey key, TValue value, bool isInvalidKey, bool isTempKey)
        {
            this.Key = key;
            this.Value = value;
            this.IsInvalidKey = isInvalidKey;
            this.IsTempKey = isTempKey;
        }

        public bool Equals(EditableKeyValuePair<TKey, TValue> other)
        {
            // We consider these to be equal if only the key is equal
            return PropertyValueEntry<TKey>.EqualityComparer(this.Key, other.Key);
        }
    }
}
#endif