//-----------------------------------------------------------------------
// <copyright file="ColorPaletteDrawer.cs" company="Sirenix ApS">
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

    using System.Collections.Generic;
    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Color palette property drawer.
    /// </summary>
    internal sealed class ColorPaletteDrawer : OdinValueDrawer<ColorPalette>
    {
        private bool isEditing;

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            entry.SmartValue.Name = entry.SmartValue.Name ?? "Palette Name";

            SirenixEditorGUI.BeginBox();
            {
                SirenixEditorGUI.BeginToolbarBoxHeader();
                {
                    GUILayout.Label(entry.SmartValue.Name);
                    GUILayout.FlexibleSpace();
                    if (SirenixEditorGUI.IconButton(EditorIcons.Pen))
                    {
                        isEditing = !isEditing;
                    }
                }
                SirenixEditorGUI.EndToolbarBoxHeader();

                if (entry.SmartValue.Colors == null)
                {
                    entry.SmartValue.Colors = new List<Color>();
                }

                if (SirenixEditorGUI.BeginFadeGroup(entry.SmartValue, entry, isEditing))
                {
                    this.CallNextDrawer(null);
                }
                SirenixEditorGUI.EndFadeGroup();

                if (SirenixEditorGUI.BeginFadeGroup(entry.SmartValue, entry.SmartValue, isEditing == false))
                {
                    Color col = default(Color);

                    var stretch = ColorPaletteManager.Instance.StretchPalette;
                    var size = ColorPaletteManager.Instance.SwatchSize;
                    var margin = ColorPaletteManager.Instance.SwatchSpacing;
                    ColorPaletteAttributeDrawer.DrawColorPaletteColorPicker(entry, entry.SmartValue, ref col, entry.SmartValue.ShowAlpha, stretch, size, 20, margin);
                }
                SirenixEditorGUI.EndFadeGroup();
            }
            SirenixEditorGUI.EndToolbarBox();
        }
    }
}
#endif