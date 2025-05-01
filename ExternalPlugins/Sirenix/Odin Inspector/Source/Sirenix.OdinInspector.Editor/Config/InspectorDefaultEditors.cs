//-----------------------------------------------------------------------
// <copyright file="InspectorDefaultEditors.cs" company="Sirenix ApS">
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
    /// InspectorDefaultEditors is a bitmask used to tell <see cref="InspectorConfig"/> which types should have an Odin Editor generated.
    /// </summary>
    /// <seealso cref="InspectorConfig"/>
    [Flags]
    public enum InspectorDefaultEditors
    {
        /// <summary>
        /// Excludes all types.
        /// </summary>
        None = 0,

        /// <summary>
        /// UserTypes includes all custom user scripts that are not located in an editor or plugin folder.
        /// </summary>
        UserTypes = 1 << 0,

        /// <summary>
        /// PluginTypes includes all types located in the plugins folder and are not located in an editor folder.
        /// </summary>
        PluginTypes = 1 << 1,

        /// <summary>
        /// UnityTypes includes all types depended on UnityEngine and from UnityEngine, except editor, plugin and user types.
        /// </summary>
        UnityTypes = 1 << 2,

        /// <summary>
        /// OtherTypes include all other types that are not depended on UnityEngine or UnityEditor.
        /// </summary>
        OtherTypes = 1 << 3
    }
}
#endif