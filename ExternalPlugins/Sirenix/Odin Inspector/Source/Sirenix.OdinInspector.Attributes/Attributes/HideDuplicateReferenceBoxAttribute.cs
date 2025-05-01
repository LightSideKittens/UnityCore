//-----------------------------------------------------------------------
// <copyright file="HideDuplicateReferenceBoxAttribute.cs" company="Sirenix ApS">
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
    /// Indicates that Odin should hide the reference box, if this property would otherwise be drawn as a reference to another property, due to duplicate reference values being encountered.
    /// Note that if the value is referencing itself recursively, then the reference box will be drawn regardless of this attribute in all recursive draw calls.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class HideDuplicateReferenceBoxAttribute : Attribute
    {
    }
}