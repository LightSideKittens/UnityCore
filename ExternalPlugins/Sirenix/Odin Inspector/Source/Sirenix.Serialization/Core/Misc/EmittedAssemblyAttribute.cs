//-----------------------------------------------------------------------
// <copyright file="EmittedAssemblyAttribute.cs" company="Sirenix ApS">
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

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public sealed class EmittedAssemblyAttribute : Attribute
    {
        [Obsolete("This attribute cannot be used in code, and is only meant to be applied to dynamically emitted assemblies.", true)]
        public EmittedAssemblyAttribute() { }
    }
}