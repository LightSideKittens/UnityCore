//-----------------------------------------------------------------------
// <copyright file="PropertyType.cs" company="Sirenix ApS">
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

    using System;

    /// <summary>
    /// Enumeration describing the different types of properties that exist.
    /// </summary>
    public enum PropertyType
    {
        /// <summary>
        /// Property represents a value.
        /// </summary>
        Value = 0,

        /// <summary>
        /// Property represents a method.
        /// </summary>
        Method = 1,

        /// <summary>
        /// Property represents a named group of properties.
        /// </summary>
        Group = 2
    }
}
#endif