//-----------------------------------------------------------------------
// <copyright file="EnumDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

    using System;
    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Enum property drawer.
    /// </summary>
    public sealed class EnumDrawer<T> : OdinValueDrawer<T>
    {
        /// <summary>
        /// Returns <c>true</c> if the drawer can draw the type.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.IsEnum;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            if (GeneralDrawerConfig.Instance.UseImprovedEnumDropDown)
            {
                entry.SmartValue = EnumSelector<T>.DrawEnumField(label, entry.SmartValue);
            }
            else
            {
                entry.WeakSmartValue = SirenixEditorFields.EnumDropdown(label, (Enum)entry.WeakSmartValue);
            }
        }
    }
}
#endif