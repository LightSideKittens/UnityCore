//-----------------------------------------------------------------------
// <copyright file="PrefabModificationType.cs" company="Sirenix ApS">
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

    /// <summary>
    /// Types of prefab modification that can be applied.
    /// </summary>
    public enum PrefabModificationType
    {
        /// <summary>
        /// A value has been changed at a given path.
        /// </summary>
        Value,

        /// <summary>
        /// A list length has been changed at a given path.
        /// </summary>
        ListLength,

        /// <summary>
        /// A dictionary has been changed at a given path.
        /// </summary>
        Dictionary
    }
}