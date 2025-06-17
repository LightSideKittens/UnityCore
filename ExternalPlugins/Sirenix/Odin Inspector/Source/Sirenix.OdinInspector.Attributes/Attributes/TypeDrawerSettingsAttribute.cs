//-----------------------------------------------------------------------
// <copyright file="TypeDrawerSettingsAttribute.cs" company="Sirenix ApS">
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

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class TypeDrawerSettingsAttribute : Attribute
	{
		/// <summary>
		/// Specifies whether a base type should be used instead of all types.
		/// </summary>
		public Type BaseType = null;
		public string FilterFunc;
		public Func<Type, bool> FilterMethod;

		/// <summary>
		/// Filters the result.
		/// </summary>
		public TypeInclusionFilter Filter = TypeInclusionFilter.IncludeAll;
	}
}