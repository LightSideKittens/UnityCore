//-----------------------------------------------------------------------
// <copyright file="Int64Drawer.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Long property drawer.
    /// </summary>
    public sealed class Int64Drawer : OdinValueDrawer<long>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (GeneralDrawerConfig.Instance.EnableSmartNumberFields)
            {
                this.ValueEntry.SmartValue = SirenixEditorFields.SmartLongField(this.Property.ToFieldExpressionContext(), label, this.ValueEntry.SmartValue);
            }
            else
            {
                this.ValueEntry.SmartValue = SirenixEditorFields.LongField(label, this.ValueEntry.SmartValue);
            }
        }
    }
}
#endif