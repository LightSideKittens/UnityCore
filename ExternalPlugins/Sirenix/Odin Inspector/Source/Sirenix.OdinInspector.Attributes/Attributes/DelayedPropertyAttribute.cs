//-----------------------------------------------------------------------
// <copyright file="DelayedPropertyAttribute.cs" company="Sirenix ApS">
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
    /// Delays applying changes to properties while they still being edited in the inspector.
    /// Similar to Unity's built-in Delayed attribute, but this attribute can also be applied to properties.
    /// </summary>
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
	public class DelayedPropertyAttribute : Attribute
	{ }
}