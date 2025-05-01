//-----------------------------------------------------------------------
// <copyright file="StringDrawer.cs" company="Sirenix ApS">
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

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// String property drawer.
    /// </summary>
    public sealed class StringDrawer : OdinValueDrawer<string>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            entry.SmartValue = label == null ?
                EditorGUILayout.TextField(entry.SmartValue, EditorStyles.textField) :
                EditorGUILayout.TextField(label, entry.SmartValue, EditorStyles.textField);
        }
    }
}
#endif