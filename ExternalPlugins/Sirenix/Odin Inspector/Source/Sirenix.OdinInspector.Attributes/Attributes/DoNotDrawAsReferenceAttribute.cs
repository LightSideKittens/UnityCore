//-----------------------------------------------------------------------
// <copyright file="DoNotDrawAsReferenceAttribute.cs" company="Sirenix ApS">
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
    /// Indicates that the member should not be drawn as a value reference, if it becomes a reference to another value in the tree. Beware, and use with care! This may lead to infinite draw loops!
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class DoNotDrawAsReferenceAttribute : Attribute
    {
    }
}