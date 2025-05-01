//-----------------------------------------------------------------------
// <copyright file="TypeSelectorSettingsAttribute.cs" company="Sirenix ApS">
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
	public class TypeSelectorSettingsAttribute : Attribute
	{
		public const string FILTER_TYPES_FUNCTION_NAMED_VALUE = "type";

		/// <summary> Specifies if the '&lt;none&gt;' item is shown. </summary>
		public bool ShowNoneItem
		{
			get => this.showNoneItem ?? default;
			set => this.showNoneItem = value;
		}

		/// <summary> Specifies if categories are shown. </summary>
		public bool ShowCategories
		{
			get => this.showCategories ?? default;
			set => this.showCategories = value;
		}

		/// <summary>
		/// Specifies if namespaces are preferred over assembly category names for category names.
		/// </summary>
		public bool PreferNamespaces
		{
			get => this.preferNamespaces ?? default;
			set => this.preferNamespaces = value;
		}

		/// <summary>
		/// Function for filtering types displayed in the Type Selector.
		/// </summary>
		/// <example>
		/// <para>
		/// The resolver expects any method that takes a single parameter of <see cref="Type"/>, with the parameter name 'type', and which returns a <see cref="bool"/> indicating whether the <see cref="Type"/> is included or not;
		/// </para>
		/// 
		/// <para>Implementation example: <c>public bool SomeFilterMethod(Type type)</c>.</para>
		/// </example>
		public string FilterTypesFunction = null;

		public bool ShowNoneItemIsSet => this.showNoneItem.HasValue;
		public bool ShowCategoriesIsSet => this.showCategories.HasValue;
		public bool PreferNamespacesIsSet => this.preferNamespaces.HasValue;

		private bool? showNoneItem;
		private bool? showCategories;
		private bool? preferNamespaces;
	}
}