//-----------------------------------------------------------------------
// <copyright file="UInt32Drawer.cs" company="Sirenix ApS">
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
    /// Uint property drawer.
    /// </summary>
    public sealed class UInt32Drawer : OdinValueDrawer<uint>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            long value;
            if (GeneralDrawerConfig.Instance.EnableSmartNumberFields)
            {
                value = SirenixEditorFields.SmartLongField(this.Property.ToFieldExpressionContext(), label, this.ValueEntry.SmartValue);
            }
            else
            {
                value = SirenixEditorFields.LongField(label, this.ValueEntry.SmartValue);
            }

            if (value > uint.MaxValue)
            {
                value = uint.MaxValue;
            }
            else if (value < 0)
            {
                value = 0;
            }

            this.ValueEntry.SmartValue = (uint)value;
        }
    }
}
#endif