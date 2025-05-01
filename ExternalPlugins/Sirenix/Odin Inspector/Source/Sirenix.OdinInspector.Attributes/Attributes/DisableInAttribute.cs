//-----------------------------------------------------------------------
// <copyright file="DisableInAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;

namespace Sirenix.OdinInspector
{
#pragma warning disable

    /// <summary>
    /// Disables a member based on which type of a prefab and instance it is in. 
    /// </summary>
    [DontApplyToListElements]
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class DisableInAttribute : Attribute
    {
        public PrefabKind PrefabKind;

        public DisableInAttribute(PrefabKind prefabKind)
        {
            this.PrefabKind = prefabKind;
        }
    }
}