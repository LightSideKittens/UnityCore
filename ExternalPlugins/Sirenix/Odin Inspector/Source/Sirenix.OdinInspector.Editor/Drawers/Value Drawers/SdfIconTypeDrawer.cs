//-----------------------------------------------------------------------
// <copyright file="SdfIconTypeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.Internal;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    [DrawerPriority(0, 0, 1)]
    public class SdfIconTypeDrawer : OdinValueDrawer<SdfIconType>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.ValueEntry.SmartValue = SdfIconSelector.DrawIconSelectorDropdownField(label, this.ValueEntry.SmartValue);
        }
    }
}
#endif