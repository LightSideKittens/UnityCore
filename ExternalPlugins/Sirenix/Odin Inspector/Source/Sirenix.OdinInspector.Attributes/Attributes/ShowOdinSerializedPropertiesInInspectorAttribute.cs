//-----------------------------------------------------------------------
// <copyright file="ShowOdinSerializedPropertiesInInspectorAttribute.cs" company="Sirenix ApS">
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

    using System;

    /// <summary>
    /// Marks a type as being specially serialized. Odin uses this attribute to check whether it should include non-Unity-serialized members in the inspector.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class ShowOdinSerializedPropertiesInInspectorAttribute : Attribute
    {
    }
}