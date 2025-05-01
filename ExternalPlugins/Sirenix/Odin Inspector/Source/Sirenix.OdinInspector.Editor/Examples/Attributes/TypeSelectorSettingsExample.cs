//-----------------------------------------------------------------------
// <copyright file="TypeSelectorSettingsExample.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

	[AttributeExample(typeof(TypeSelectorSettingsAttribute))]
	internal class TypeSelectorSettingsExample
	{
		[ShowInInspector]
		public Type Default;

		[Title("Show Categories"), ShowInInspector, LabelText("On")]
		[TypeSelectorSettings(ShowCategories = true)]
		public Type ShowCategories_On;

		[ShowInInspector, LabelText("Off")]
		[TypeSelectorSettings(ShowCategories = false)]
		public Type ShowCategories_Off;

		[Title("Prefer Namespaces"), ShowInInspector, LabelText("On")]
		[TypeSelectorSettings(PreferNamespaces = true, ShowCategories = true)]
		public Type PreferNamespaces_On;

		[ShowInInspector, LabelText("Off")]
		[TypeSelectorSettings(PreferNamespaces = false, ShowCategories = true)]
		public Type PreferNamespaces_Off;

		[Title("Show None Item"), ShowInInspector, LabelText("On")]
		[TypeSelectorSettings(ShowNoneItem = true)]
		public Type ShowNoneItem_On;

		[ShowInInspector, LabelText("Off")]
		[TypeSelectorSettings(ShowNoneItem = false)]
		public Type ShowNoneItem_Off;

		[Title("Custom Type Filter"), ShowInInspector]
		[TypeSelectorSettings(FilterTypesFunction = nameof(TypeFilter), ShowCategories = false)]
		public Type CustomTypeFilterExample;

		private bool TypeFilter(Type type)
		{
			return type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
		}
	}
}
#endif