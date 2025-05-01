//-----------------------------------------------------------------------
// <copyright file="PersistentAssemblyAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities
{
#pragma warning disable

	using System;
    
	/// <summary>
    /// Indicates a persistent assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PersistentAssemblyAttribute : Attribute
    {
    }
}