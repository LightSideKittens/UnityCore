//-----------------------------------------------------------------------
// <copyright file="PolymorphicDrawerSettingsAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using System.Runtime.Serialization;

namespace Sirenix.OdinInspector
{
#pragma warning disable

	/// <summary>
	/// Specifies how non-default constructors are handled.
	/// </summary>
	public enum NonDefaultConstructorPreference
	{
		/// <summary>
		/// Excludes types with non default constructors from the Selector.
		/// </summary>
		Exclude,
		
		/// <summary>
		/// Attempts to find the most straightforward constructor to call, prioritizing default values.
		/// </summary>
		ConstructIdeal,

		/// <summary>
		/// Uses <see cref="FormatterServices.GetUninitializedObject"/> if no default constructor is found.
		/// </summary>
		PreferUninitialized,

		/// <summary>
		/// Logs a warning instead of constructing the object, indicating that an attempt was made to construct an object without a default constructor.
		/// </summary>
		LogWarning,
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class PolymorphicDrawerSettingsAttribute : Attribute
	{
		/// <summary>
		/// Determines whether the base type should be displayed in the drawer.
		/// </summary>
		public bool ShowBaseType
		{
			get => this.showBaseType ?? default;
			set => this.showBaseType = value;
		}

		/// <summary>
		/// Indicates if the drawer should be read-only once a value is assigned.
		/// </summary>
		public bool ReadOnlyIfNotNullReference = false;

		/// <summary>
		/// Specifies how non-default constructors are handled.
		/// </summary>
		public NonDefaultConstructorPreference NonDefaultConstructorPreference
		{
			get => this.nonDefaultConstructorPreference ?? NonDefaultConstructorPreference.ConstructIdeal;
			set => this.nonDefaultConstructorPreference = value;
		}

		/// <summary>
		/// Specifies a custom function for creating an instance of the selected <see cref="Type"/>.
		/// </summary>
		/// <remarks>Does not get called for <see cref="UnityEngine.Object">UnityEngine.Object</see> types.</remarks>
		/// <example>
		/// <para>
		/// The resolver expects any method that takes a single parameter of <see cref="Type"/>, where the parameter is named 'type', and which returns an <see cref="object"/>.
		/// </para>
		/// 
		/// <para>Implementation example: <c>public object Method(Type type)</c>.</para>
		/// </example>
		public string CreateInstanceFunction = null;

		[Obsolete("Use " + nameof(OnValueChangedAttribute) + " instead.",
#if SIRENIX_INTERNAL
					 true
#else
					 false
#endif
					)]
		public string OnInstanceAssigned = null;
		
		public bool ShowBaseTypeIsSet => this.showBaseType.HasValue;
		public bool NonDefaultConstructorPreferenceIsSet => this.nonDefaultConstructorPreference.HasValue;

		private bool? showBaseType;
		private NonDefaultConstructorPreference? nonDefaultConstructorPreference;
	}
}