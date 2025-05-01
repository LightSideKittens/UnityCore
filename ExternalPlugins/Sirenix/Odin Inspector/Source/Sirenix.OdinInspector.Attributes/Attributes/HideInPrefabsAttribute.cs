//-----------------------------------------------------------------------
// <copyright file="HideInPrefabsAttribute.cs" company="Sirenix ApS">
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
    using System.ComponentModel;

    /// <summary>
    /// Hides a property if it is drawn from a prefab instance or a prefab asset.
    /// </summary>
    [Obsolete("Use [HideIn(PrefabKind.PrefabAsset | PrefabKind.PrefabInstance)] instead.", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class HideInPrefabsAttribute : Attribute
    {
    }
}