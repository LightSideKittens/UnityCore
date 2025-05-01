//-----------------------------------------------------------------------
// <copyright file="IncludeMyAttributesAttribute.cs" company="Sirenix ApS">
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
    /// When this attribute is added is added to another attribute, then attributes from that attribute
    /// will also be added to the property in the attribute processing step.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class IncludeMyAttributesAttribute : Attribute
    {
    }
}