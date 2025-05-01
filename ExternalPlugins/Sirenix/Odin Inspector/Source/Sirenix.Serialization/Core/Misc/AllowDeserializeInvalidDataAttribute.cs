//-----------------------------------------------------------------------
// <copyright file="AllowDeserializeInvalidDataAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// <para>
    /// Applying this attribute to a type indicates that in the case where, when expecting to deserialize an instance of the type
    /// or any of its derived types, but encountering an incompatible, uncastable type in the data being read, the serializer
    /// should attempt to deserialize an instance of the expected type using the stored, possibly invalid data.
    /// </para>
    /// <para>
    /// This is equivalent to the <see cref="SerializationConfig.AllowDeserializeInvalidData"/> option, expect type-specific instead
    /// of global.
    /// </para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public class AllowDeserializeInvalidDataAttribute : Attribute
    {
    }
}