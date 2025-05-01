//-----------------------------------------------------------------------
// <copyright file="OdinDontRegisterAttribute.cs" company="Sirenix ApS">
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
    /// Use this attribute to prevent a type from being included in Odin systems.
    /// The attribute can be applied to Odin drawers, Odin property resolvers and Odin attribute processor types.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OdinDontRegisterAttribute : Attribute
    {
    }
}
#endif