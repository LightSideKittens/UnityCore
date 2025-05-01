//-----------------------------------------------------------------------
// <copyright file="ExcludeDataFromInspectorAttribute.cs" company="Sirenix ApS">
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
    /// Causes Odin's inspector to completely ignore a given member, preventing it from even being included in an Odin PropertyTree,
    /// and such will not cause any performance hits in the inspector.
    /// </para>
    /// <para>Note that Odin can still serialize an excluded member - it is merely ignored in the inspector itself.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [Obsolete("Use [HideInInspector] instead - it now also excludes the member completely from becoming a property in the property tree.", false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class ExcludeDataFromInspectorAttribute : Attribute
    {
    }
}