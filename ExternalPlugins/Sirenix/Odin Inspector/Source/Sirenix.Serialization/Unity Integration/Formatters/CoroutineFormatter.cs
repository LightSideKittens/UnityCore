//-----------------------------------------------------------------------
// <copyright file="CoroutineFormatter.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Serialization;

[assembly: RegisterFormatter(typeof(CoroutineFormatter))]

namespace Sirenix.Serialization
{
#pragma warning disable

    using System;
    using UnityEngine;

    /// <summary>
    /// <para>
    /// Custom formatter for the <see cref="Coroutine"/> type.
    /// This serializes nothing and always deserializes null,
    /// and only exists to ensure that no coroutine instances
    /// are ever created by the serialization system, since they
    /// will in almost all cases be invalid instances.
    /// </para>
    /// <para>
    /// Invalid coroutine instances crash Unity instantly when
    /// they are garbage collected.
    /// </para>
    /// </summary>
    public sealed class CoroutineFormatter : IFormatter<Coroutine>
    {
        /// <summary>
        /// Gets the type that the formatter can serialize.
        /// </summary>
        /// <value>
        /// The type that the formatter can serialize.
        /// </value>
        public Type SerializedType { get { return typeof(Coroutine); } }

        /// <summary>
        /// Returns null.
        /// </summary>
        object IFormatter.Deserialize(IDataReader reader)
        {
            return null;
        }

        /// <summary>
        /// Returns null.
        /// </summary>
        public Coroutine Deserialize(IDataReader reader)
        {
            return null;
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Serialize(object value, IDataWriter writer)
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        public void Serialize(Coroutine value, IDataWriter writer)
        {
        }
    }
}