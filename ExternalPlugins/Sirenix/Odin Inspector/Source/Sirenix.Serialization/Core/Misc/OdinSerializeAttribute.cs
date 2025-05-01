//-----------------------------------------------------------------------
// <copyright file="OdinSerializeAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Indicates that an instance field or auto-property should be serialized by Odin.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [JetBrains.Annotations.MeansImplicitUse]
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class OdinSerializeAttribute : Attribute
    {
    }
}