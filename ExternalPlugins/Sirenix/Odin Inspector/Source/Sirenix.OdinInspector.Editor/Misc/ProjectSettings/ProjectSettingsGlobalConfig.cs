//-----------------------------------------------------------------------
// <copyright file="ProjectSettingsGlobalConfig.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using UnityEngine;

    public abstract class ProjectSettingsGlobalConfig<T> : GlobalConfig<T>, ISerializationCallbackReceiver where T : ProjectSettingsGlobalConfig<T>, new()
    {
        [NonSerialized]
        private bool initialized;

        [OnInspectorInit, PropertyOrder(float.MinValue)]
        private void EnsureInitialized()
        {
            if (!this.initialized)
            {
                this.initialized = true;
                ProjectSettingsUtility.InitAllProjectSettingFieldsFromAttributes(this);
            }
        }

        protected virtual void Awake()
        {
            this.EnsureInitialized();
        }

        protected virtual void OnEnable()
        {
            this.EnsureInitialized();
        }

        public virtual void OnAfterDeserialize()
        {
            this.EnsureInitialized();
        }

        public virtual void OnBeforeSerialize() { }

        protected override void OnConfigInstanceFirstAccessed()
        {
            this.EnsureInitialized();
        }
    }
}
#endif