//-----------------------------------------------------------------------
// <copyright file="AlwaysFormatsSelfAttribute.cs" company="Sirenix ApS">
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
    /// Use this attribute to specify that a type that implements the <see cref="ISelfFormatter"/>
    /// interface should *always* format itself regardless of other formatters being specified.
    /// <para />
    /// This means that the interface will be used to format all types derived from the type that
    /// is decorated with this attribute, regardless of custom formatters for the derived types.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
    public sealed class AlwaysFormatsSelfAttribute : Attribute
    {
    }
}