//-----------------------------------------------------------------------
// <copyright file="TypeSelector.cs" company="Sirenix ApS">
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
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    public class TypeSelector : OdinSelector<Type>
    {
        private static Dictionary<AssemblyCategory, List<OdinMenuItem>> cachedAllTypesMenuItems = new Dictionary<AssemblyCategory, List<OdinMenuItem>>();

        private IEnumerable<Type> types;
        private AssemblyCategory assemblyCategories;
        private bool supportsMultiSelect;

        [HideInInspector]
        public bool HideNamespaces = false;

        [HideInInspector]
        public bool FlattenTree = false;

        public override string Title
        {
            get { return null; /* "Select Type";*/ }
        }

        [Obsolete(AssemblyUtilities.AssemblyTypeFlagsObsoleteMessage, Consts.IsSirenixInternal)]
        public TypeSelector(AssemblyTypeFlags assemblyFlags, bool supportsMultiSelect)
        {
            this.types = null;
            this.supportsMultiSelect = supportsMultiSelect;
            this.assemblyCategories = AssemblyUtilities.LossyBadConvertAssemblyTypeFlagsToCategories(assemblyFlags);
        }

        public TypeSelector(AssemblyCategory assemblyCategories, bool supportsMultiSelect)
        {
            this.types = null;
            this.supportsMultiSelect = supportsMultiSelect;
            this.assemblyCategories = assemblyCategories;
        }

        public TypeSelector(IEnumerable<Type> types, bool supportsMultiSelect)
        {
            this.types = types != null ? OrderTypes(types) : types;
            this.supportsMultiSelect = supportsMultiSelect;
        }

        private static IEnumerable<Type> OrderTypes(IEnumerable<Type> types)
        {
            return types.OrderByDescending(x => x.Namespace.IsNullOrWhitespace())
                        .ThenBy(x => x.Namespace)
                        .ThenBy(x => x.Name);
        }

        public override bool IsValidSelection(IEnumerable<Type> collection)
        {
            return collection.Any();
        }

        /// <summary>
        /// Builds the selection tree.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Config.UseCachedExpandedStates = false;
            tree.DefaultMenuStyle.NotSelectedIconAlpha = 1f;
            tree.Config.SelectMenuItemsOnMouseDown = true;
            if (types == null)
            {
                List<OdinMenuItem> items;
                if (cachedAllTypesMenuItems.TryGetValue(this.assemblyCategories, out items))
                {
                    AddRecursive(tree, items, tree.MenuItems);
                }
                else
                {
                    var assemblyTypes = OrderTypes(AssemblyUtilities.GetTypes(this.assemblyCategories).Where(x =>
                    {
                        if (x.Name == null) return false;
                        var trimmedName = x.Name.TrimStart();
                        if (trimmedName.Length == 0) return false;
                        return char.IsLetter(trimmedName[0]);
                    }));

                    foreach (var t in assemblyTypes)
                    {
                        var niceName = t.GetNiceName();
                        string path = this.GetTypeNamePath(t, niceName);
                        var last = tree.AddObjectAtPath(path, t).AddThumbnailIcons().Last();
                        last.SearchString = niceName == path ? path : niceName + "|" + path;
                    }

                    cachedAllTypesMenuItems[this.assemblyCategories] = tree.MenuItems;
                }
            }
            else
            {
                foreach (var t in this.types)
                {
                    var niceName = t.GetNiceName();
                    string path = this.GetTypeNamePath(t, niceName);
                    var last = tree.AddObjectAtPath(path, t).Last();
                    last.SearchString = niceName == path ? path : niceName + "|" + path;

                    if (this.FlattenTree && t.Namespace != null && !this.HideNamespaces)
                    {
                        last.OnDrawItem += x => GUI.Label(x.Rect.Padding(10, 0).AlignCenterY(16), t.Namespace, SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    }
                }

                tree.EnumerateTree(x => x.Value != null, false).AddThumbnailIcons();
            }

            //tree.EnumerateTree().ForEach(i =>
            //{
            //    var t = i.Value as Type;
            //    if (t != null) { i.SearchString = t.GetNiceFullName(); }
            //});

            tree.Selection.SupportsMultiSelect = this.supportsMultiSelect;
            tree.Selection.SelectionChanged += (t) =>
            {
                lastType = this.SelectionTree.Selection.Select(x => x.Value).OfType<Type>().LastOrDefault() ?? lastType;
            };
        }

        private string GetTypeNamePath(Type t, string niceName)
        {
            var name = niceName;

            if (!this.FlattenTree)
            {
                if (!string.IsNullOrEmpty(t.Namespace) && !this.HideNamespaces)
                {
                    var separator = this.FlattenTree ? '.' : '/';
                    name = t.Namespace + separator + name;
                }
            }

            return name;
        }

        private static void AddRecursive(OdinMenuTree tree, List<OdinMenuItem> source, List<OdinMenuItem> destination)
        {
            destination.Capacity = source.Count;

            for (int i = 0; i < source.Count; i++)
            {
                var item = source[i];
                var clone = new OdinMenuItem(tree, item.Name, item.Value)
                    .AddThumbnailIcon(false);

                clone.SearchString = item.SearchString;

                destination.Add(clone);

                if (item.ChildMenuItems.Count > 0)
                {
                    AddRecursive(tree, item.ChildMenuItems, clone.ChildMenuItems);
                }
            }
        }

        private Type lastType;

        /// <summary>
        /// 450
        /// </summary>
        protected override float DefaultWindowWidth()
        {
            return 450;
        }

        [OnInspectorGUI, PropertyOrder(10)]
        private void ShowTypeInfo()
        {
            var fullTypeName = "";
            var assembly = "";
            var baseType = "";
            var labelHeight = 16;
            var rect = GUILayoutUtility.GetRect(0, labelHeight * 3 + 8).Padding(10, 4).AlignTop(labelHeight);
            var labelWidth = 75;

            if (lastType != null)
            {
                fullTypeName = lastType.GetNiceFullName();
                assembly = lastType.Assembly.GetName().Name;
                baseType = lastType.BaseType == null ? "" : lastType.BaseType.GetNiceFullName();
            }

            var style = SirenixGUIStyles.LeftAlignedGreyMiniLabel;
            GUI.Label(rect.AlignLeft(labelWidth), "Type Name", style);
            GUI.Label(rect.AlignRight(rect.width - labelWidth), fullTypeName, style);
            rect.y += labelHeight;
            GUI.Label(rect.AlignLeft(labelWidth), "Base Type", style);
            GUI.Label(rect.AlignRight(rect.width - labelWidth), baseType, style);
            rect.y += labelHeight;
            GUI.Label(rect.AlignLeft(labelWidth), "Assembly", style);
            GUI.Label(rect.AlignRight(rect.width - labelWidth), assembly, style);
        }

        /// <summary>
        /// Sets the selected types.
        /// </summary>
        public override void SetSelection(Type selected)
        {
            base.SetSelection(selected);

            // Expand so selected is visisble.
            this.SelectionTree.Selection.SelectMany(x => x.GetParentMenuItemsRecursive(false))
                .ForEach(x => x.Toggled = true);
        }
    }
}
#endif