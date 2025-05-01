//-----------------------------------------------------------------------
// <copyright file="CharDrawer.cs" company="Sirenix ApS">
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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Char property drawer.
    /// </summary>
    public sealed class CharDrawer : OdinValueDrawer<char>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            EditorGUI.BeginChangeCheck();
            string s = new string(entry.SmartValue, 1);
            s = SirenixEditorFields.TextField(label, s);

            if (EditorGUI.EndChangeCheck() && s.Length > 0)
            {
                entry.SmartValue = s[0];
            }
        }
    }
}
#endif