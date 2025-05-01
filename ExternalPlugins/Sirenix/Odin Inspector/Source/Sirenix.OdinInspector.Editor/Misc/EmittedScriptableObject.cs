//-----------------------------------------------------------------------
// <copyright file="EmittedScriptableObject.cs" company="Sirenix ApS">
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

    using System.Reflection;
    using UnityEngine;

    /// <summary>
    /// Base class for emitted ScriptableObject-derived types that have been created by the <see cref="UnityPropertyEmitter"/>.
    /// </summary>
    public abstract class EmittedScriptableObject : ScriptableObject
    {
        /// <summary>
        /// The field that backs the value of this scriptable object.
        /// </summary>
        public abstract FieldInfo BackingFieldInfo { get; }

        /// <summary>
        /// Sets the value contained in this scriptable object.
        /// </summary>
        public abstract void SetWeakValue(object value);

        /// <summary>
        /// Gets the value contained in this scriptable object.
        /// </summary>
        public abstract object GetWeakValue();
    }

    /// <summary>
    /// Strongly typed base class for emitted scriptable object types that have been created by the <see cref="UnityPropertyEmitter"/>.
    /// </summary>
    public abstract class EmittedScriptableObject<T> : EmittedScriptableObject
    {
        /// <summary>
        /// Sets the value contained in this scriptable object.
        /// </summary>
        public override void SetWeakValue(object value)
        {
            this.SetValue((T)value);
        }

        /// <summary>
        /// Gets the value contained in this scriptable object.
        /// </summary>
        public override object GetWeakValue()
        {
            return this.GetValue();
        }

        /// <summary>
        /// Sets the value contained in this scriptable object.
        /// </summary>
        public abstract void SetValue(T value);

        /// <summary>
        /// Gets the value contained in this scriptable object.
        /// </summary>
        public abstract T GetValue();
    }
}
#endif