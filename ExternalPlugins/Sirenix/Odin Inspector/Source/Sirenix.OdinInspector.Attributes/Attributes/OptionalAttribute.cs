//-----------------------------------------------------------------------
// <copyright file="OptionalAttribute.cs" company="Sirenix ApS">
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
    /// Overrides the 'Reference Required by Default' rule to allow for null values.
    /// Has no effect if the rule is disabled.
    /// 
    /// This attribute does not do anything unless you have Odin Validator and the 'Reference Required by Default' rule is enabled.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public class OptionalAttribute : Attribute
    {
    }

    // As implementing this attribute below was holding back the release of 3.1, it has been removed for now and will be added in a later patch

    ///// <summary>
    ///// Overrides the 'Reference Required by Default' rule to allow for null values.
    ///// Has no effect if the rule is disabled.
    ///// 
    ///// This attribute does not do anything unless you have Odin Validator and the 'Reference Required by Default' rule is enabled.
    ///// </summary>
    //[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    //[System.Diagnostics.Conditional("UNITY_EDITOR")]
    //public class OptionalInAttribute : Attribute
    //{
    //    public PrefabKind PrefabKind;

    //    public OptionalInAttribute(PrefabKind prefabKind)
    //    {
    //        this.PrefabKind = prefabKind;
    //    }
    //}
}