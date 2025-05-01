//-----------------------------------------------------------------------
// <copyright file="EmittedFormatterAttribute.cs" company="Sirenix ApS">
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
    /// Indicates that this formatter type has been emitted. Never put this on a type!
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EmittedFormatterAttribute : Attribute
    {
    }
}