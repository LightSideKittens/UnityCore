//-----------------------------------------------------------------------
// <copyright file="HideInAttribute.cs" company="Sirenix ApS">
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
    /// Hides a member based on which type of a prefab and instance it is in. 
    /// </summary>
    [AttributeUsage(AttributeTargets.All)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class HideInAttribute : Attribute
    {
        public PrefabKind PrefabKind;

        public HideInAttribute(PrefabKind prefabKind)
        {
            this.PrefabKind = prefabKind;
        }
    }
}