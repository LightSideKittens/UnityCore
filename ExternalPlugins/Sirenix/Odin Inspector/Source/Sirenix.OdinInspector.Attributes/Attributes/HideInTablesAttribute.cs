//-----------------------------------------------------------------------
// <copyright file="HideInTablesAttribute.cs" company="Sirenix ApS">
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
    /// The HideInTables attribute is used to prevent members from showing up as columns in tables drawn using the <see cref="TableListAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class HideInTablesAttribute : Attribute
    {
    }
}