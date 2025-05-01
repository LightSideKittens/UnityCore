//-----------------------------------------------------------------------
// <copyright file="DisableInNonPrefabsAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System.ComponentModel;

namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Disables a property if it is drawn from a non-prefab asset or instance.
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [Obsolete("Use [DisableIn(PrefabKind.NonPrefabInstance)] instead.", false)]
    public class DisableInNonPrefabsAttribute : Attribute
    {
    }
}