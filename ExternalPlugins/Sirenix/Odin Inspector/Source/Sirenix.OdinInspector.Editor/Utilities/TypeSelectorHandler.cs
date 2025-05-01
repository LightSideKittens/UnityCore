//-----------------------------------------------------------------------
// <copyright file="TypeSelectorHandler.cs" company="Sirenix ApS">
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
using Sirenix.Utilities;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	/// <summary>
	/// Handles instantiating different versions of the Type Selector depending on the context.
	/// </summary>
	/// <remarks>This handler only handles shared constructors between the two versions, for obsolete or unique constructors use the desired selector.</remarks>
	public static class TypeSelectorHandler_WILL_BE_DEPRECATED
	{
		// NOTE: the parameters on the first line of each method are shared between selectors, the rest are exclusively to the new selector.
		public static OdinSelector<Type> InstantiateSelector(AssemblyCategory category, bool supportsMultiSelect = false,
																			  Type selectedType = null, bool? showCategories = null, bool showHidden = false,
																			  bool? preferNamespaces = null,
																			  bool? showNoneItem = null,
																			  InspectorProperty property = null)
		{
			return InstantiateSelector(GeneralDrawerConfig.Instance.useOldTypeSelector,
									   category, supportsMultiSelect,
										selectedType, showCategories, showHidden, preferNamespaces, showNoneItem,
										property);
		}

		public static OdinSelector<Type> InstantiateSelector(IEnumerable<Type> types, bool supportsMultiSelect = false,
																			  Type selectedType = null, bool? showCategories = null, bool showHidden = false,
																			  bool? preferNamespaces = null,
																			  bool? showNoneItem = null,
																			  InspectorProperty property = null)
		{
			return InstantiateSelector(GeneralDrawerConfig.Instance.useOldTypeSelector,
									   types, supportsMultiSelect,
										selectedType, showCategories, showHidden, preferNamespaces, showNoneItem,
										property);
		}

		internal static OdinSelector<Type> InstantiateSelector(bool useOldSelector,
															   AssemblyCategory category, bool supportsMultiSelect = false,
																Type selectedType = null, bool? showCategories = null, bool showHidden = false,
																bool? preferNamespaces = null,
																bool? showNoneItem = null,
																InspectorProperty property = null)
		{
			OdinSelector<Type> selector = useOldSelector
														? (OdinSelector<Type>) new TypeSelector(category, supportsMultiSelect)
														: new TypeSelectorV2(category, supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces,
																					showNoneItem,
																					property);

			return selector;
		}
		
		internal static OdinSelector<Type> InstantiateSelector(bool useOldSelector,
															   IEnumerable<Type> types, bool supportsMultiSelect = false,
																Type selectedType = null, bool? showCategories = null, bool showHidden = false,
																bool? preferNamespaces = null,
																bool? showNoneItem = null,
																InspectorProperty property = null)
		{
			OdinSelector<Type> selector = useOldSelector
														? (OdinSelector<Type>) new TypeSelector(types, supportsMultiSelect)
														: new TypeSelectorV2(types, supportsMultiSelect, selectedType, showCategories, showHidden, preferNamespaces,
																					showNoneItem,
																					property); 
			return selector;
		}
	}
}
#endif