//-----------------------------------------------------------------------
// <copyright file="UInt16Drawer.cs" company="Sirenix ApS">
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
    /// Ushort property drawer.
    /// </summary>
    public sealed class UInt16Drawer : OdinValueDrawer<ushort>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            int value;
            if (GeneralDrawerConfig.Instance.EnableSmartNumberFields)
            {
                value = SirenixEditorFields.SmartIntField(this.Property.ToFieldExpressionContext(), label, this.ValueEntry.SmartValue);
            }
            else
            {
                value = SirenixEditorFields.IntField(label, this.ValueEntry.SmartValue);
            }

            if (value < ushort.MinValue)
            {
                value = ushort.MinValue;
            }
            else if (value > ushort.MaxValue)
            {
                value = ushort.MaxValue;
            }

            this.ValueEntry.SmartValue = (ushort)value;
        }
    }
}
#endif