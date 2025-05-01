//-----------------------------------------------------------------------
// <copyright file="SerializedComponent.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using Utilities;
    using UnityEngine;
    using Sirenix.Serialization;

    /// <summary>
    /// A Unity Component which is serialized by the Sirenix serialization system.
    /// </summary>
    [Sirenix.OdinInspector.ShowOdinSerializedPropertiesInInspector]
    public abstract class SerializedComponent : Component, ISerializationCallbackReceiver, ISupportsPrefabSerialization
    {
        [SerializeField, HideInInspector]
        private SerializationData serializationData;

        SerializationData ISupportsPrefabSerialization.SerializationData { get { return this.serializationData; } set { this.serializationData = value; } }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (this.SafeIsUnityNull()) return;
            UnitySerializationUtility.DeserializeUnityObject(this, ref this.serializationData);
            this.OnAfterDeserialize();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (this.SafeIsUnityNull()) return;
            this.OnBeforeSerialize();
            UnitySerializationUtility.SerializeUnityObject(this, ref this.serializationData);
        }

        /// <summary>
        /// Invoked after deserialization has taken place.
        /// </summary>
        protected virtual void OnAfterDeserialize()
        {
        }

        /// <summary>
        /// Invoked before serialization has taken place.
        /// </summary>
        protected virtual void OnBeforeSerialize()
        {
        }

#if UNITY_EDITOR

        [HideInTables]
        [OnInspectorGUI, PropertyOrder(int.MinValue)]
        private void InternalOnInspectorGUI()
        {
            EditorOnlyModeConfigUtility.InternalOnInspectorGUI(this);
        }

#endif
    }
}