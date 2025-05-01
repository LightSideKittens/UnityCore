//-----------------------------------------------------------------------
// <copyright file="ByteDrawer.cs" company="Sirenix ApS">
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
    /// Byte property drawer.
    /// </summary>
    public sealed class ByteDrawer : OdinValueDrawer<byte>
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

            if (value < byte.MinValue)
            {
                value = byte.MinValue;
            }
            else if (value > byte.MaxValue)
            {
                value = byte.MaxValue;
            }

            this.ValueEntry.SmartValue = (byte)value;
        }
    }
}
#endif