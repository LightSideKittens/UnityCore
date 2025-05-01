//-----------------------------------------------------------------------
// <copyright file="AssetSelectorExamples.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using UnityEngine;
    using System.Collections.Generic;
    using Sirenix.OdinInspector.Editor.Examples.Internal;

    [AttributeExample(typeof(AssetSelectorAttribute), "The AssetSelector attribute prepends a small button next to the object field that will present the user with a dropdown of assets to select from which can be customized from the attribute.")]
	[ExampleAsComponentData(Namespaces = new string[] { "System.Collections.Generic" })]
    internal class AssetSelectorExamples
    {
        [AssetSelector]
        public Material AnyAllMaterials;

        [AssetSelector]
        public Material[] ListOfAllMaterials;

        [AssetSelector(FlattenTreeView = true)]
        public Material NoTreeView;

        [AssetSelector(Paths = "Assets/MyScriptableObjects")]
        public ScriptableObject ScriptableObjectsFromFolder;

        [AssetSelector(Paths = "Assets/MyScriptableObjects|Assets/Other/MyScriptableObjects")]
        public Material ScriptableObjectsFromMultipleFolders;

        [AssetSelector(Filter = "name t:type l:label")]
        public UnityEngine.Object AssetDatabaseSearchFilters;

        [Title("Other Minor Features")]
        [AssetSelector(DisableListAddButtonBehaviour = true)]
        public List<GameObject> DisableListAddButtonBehaviour;

        [AssetSelector(DrawDropdownForListElements = false)]
        public List<GameObject> DisableListElementBehaviour;

        [AssetSelector(ExcludeExistingValuesInList = false)]
        public List<GameObject> ExcludeExistingValuesInList;

        [AssetSelector(IsUniqueList = false)]
        public List<GameObject> DisableUniqueListBehaviour;

        [AssetSelector(ExpandAllMenuItems = true)]
        public List<GameObject> ExpandAllMenuItems;

        [AssetSelector(DropdownTitle = "Custom Dropdown Title")]
        public List<GameObject> CustomDropdownTitle;
    }
}
#endif