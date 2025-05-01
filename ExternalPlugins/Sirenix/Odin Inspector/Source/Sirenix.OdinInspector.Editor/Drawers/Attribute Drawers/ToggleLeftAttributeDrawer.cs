//-----------------------------------------------------------------------
// <copyright file="ToggleLeftAttributeDrawer.cs" company="Sirenix ApS">
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
    /// Draws properties marked with <see cref="ToggleLeftAttribute"/>.
    /// </summary>
    /// <seealso cref="ToggleLeftAttribute"/>
    public sealed class ToggleLeftAttributeDrawer : OdinAttributeDrawer<ToggleLeftAttribute, bool>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            
            EditorGUI.BeginChangeCheck();

            var value = label == null ?
               EditorGUILayout.ToggleLeft(GUIContent.none, entry.SmartValue) :
               EditorGUILayout.ToggleLeft(label, entry.SmartValue);

            if (EditorGUI.EndChangeCheck())
            {
                entry.SmartValue = value;
            }
        }
    }
}
#endif