//-----------------------------------------------------------------------
// <copyright file="PrefabKind.cs" company="Sirenix ApS">
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
    /// The prefab kind returned by <see cref="Sirenix.OdinInspector.Editor.OdinPrefabUtility.GetPrefabKind"/> 
    /// </summary>
    [Flags]
    public enum PrefabKind
    {
        /// <summary>
        /// None. 
        /// </summary>
        None = 0,

        /// <summary>
        /// Instances of prefabs in scenes.
        /// </summary>
        InstanceInScene = 1 << 0,

        /// <summary>
        /// Instances of prefabs nested inside other prefabs.
        /// </summary>
        InstanceInPrefab = 1 << 1,

        /// <summary>
        /// Regular prefab assets.
        /// </summary>
        Regular = 1 << 2,

        /// <summary>
        /// Prefab variant assets.
        /// </summary>
        Variant = 1 << 3,

        /// <summary>
        /// Non-prefab component or gameobject instances in scenes.
        /// </summary>
        NonPrefabInstance = 1 << 4, 

        /// <summary>
        /// Instances of regular prefabs, and prefab variants in scenes or nested in other prefabs.
        /// </summary>
        PrefabInstance = InstanceInScene | InstanceInPrefab,

        /// <summary>
        /// Prefab assets and prefab variant assets.
        /// </summary>
        PrefabAsset = Regular | Variant,

        /// <summary>
        /// Prefab Instances, as well as non-prefab instances.
        /// </summary>
        PrefabInstanceAndNonPrefabInstance = PrefabInstance | NonPrefabInstance,

        /// <summary>
        /// All kinds
        /// </summary>
        All = InstanceInScene | InstanceInPrefab | Regular | Variant | NonPrefabInstance,
    }
}