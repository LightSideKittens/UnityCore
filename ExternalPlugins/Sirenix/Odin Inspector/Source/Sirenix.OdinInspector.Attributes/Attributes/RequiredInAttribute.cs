//-----------------------------------------------------------------------
// <copyright file="RequiredInAttribute.cs" company="Sirenix ApS">
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
    /// Makes a member required based on which type of a prefab and instance it is in. 
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class RequiredInAttribute : System.Attribute
    {
        public string ErrorMessage;
        public PrefabKind PrefabKind;

        public RequiredInAttribute(PrefabKind kind)
        {
            this.PrefabKind = kind;
        }
    }
}