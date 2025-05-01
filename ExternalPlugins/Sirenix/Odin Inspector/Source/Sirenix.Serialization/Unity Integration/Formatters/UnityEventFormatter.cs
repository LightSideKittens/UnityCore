//-----------------------------------------------------------------------
// <copyright file="UnityEventFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(UnityEventFormatter<>), weakFallback: typeof(WeakUnityEventFormatter))] 

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;
    using UnityEngine.Events;

    /// <summary>
    /// Custom generic formatter for the <see cref="UnityEvent{T0}"/>, <see cref="UnityEvent{T0, T1}"/>, <see cref="UnityEvent{T0, T1, T2}"/> and <see cref="UnityEvent{T0, T1, T2, T3}"/> types.
    /// </summary>
    /// <typeparam name="T">The type of UnityEvent that this formatter can serialize and deserialize.</typeparam>
    /// <seealso cref="ReflectionFormatter{UnityEngine.Events.UnityEvent}" />
    public class UnityEventFormatter<T> : ReflectionFormatter<T> where T : UnityEventBase, new()
    {
        /// <summary>
        /// Get an uninitialized object of type <see cref="T" />.
        /// </summary>
        /// <returns>
        /// An uninitialized object of type <see cref="T" />.
        /// </returns>
        protected override T GetUninitializedObject()
        {
            return new T();
        }
    }

    public class WeakUnityEventFormatter : WeakReflectionFormatter
    {
        public WeakUnityEventFormatter(Type serializedType) : base(serializedType)
        {
        }

        protected override object GetUninitializedObject()
        {
            return Activator.CreateInstance(this.SerializedType);
        }
    }
}