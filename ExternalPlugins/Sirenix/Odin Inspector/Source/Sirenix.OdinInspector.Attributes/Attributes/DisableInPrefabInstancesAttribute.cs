//-----------------------------------------------------------------------
// <copyright file="DisableInPrefabInstancesAttribute.cs" company="Sirenix ApS">
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

    using System.ComponentModel;
    using System;

    /// <summary>
    /// Disables a property if it is drawn from a prefab instance.
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use [DisableIn(PrefabKind.PrefabInstance)] instead.", false)]
    public class DisableInPrefabInstancesAttribute : Attribute
    {
        
    }
}