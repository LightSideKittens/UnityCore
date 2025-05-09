//-----------------------------------------------------------------------
// <copyright file="TextAreaAttributeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Reflection;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// TextArea attribute drawer.
    /// </summary>
    public class TextAreaAttributeDrawer : OdinAttributeDrawer<TextAreaAttribute, string>
    {
        private delegate string ScrollableTextAreaInternalDelegate(Rect position, string text, ref Vector2 scrollPosition, GUIStyle style);

        private static readonly ScrollableTextAreaInternalDelegate EditorGUI_ScrollableTextAreaInternal;
        private static readonly FieldInfo EditorGUI_s_TextAreaHash_Field;
        private static readonly int EditorGUI_s_TextAreaHash;

        static TextAreaAttributeDrawer()
        {
            var method = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", Flags.StaticAnyVisibility);

            if (method != null)
            {
                EditorGUI_ScrollableTextAreaInternal = (ScrollableTextAreaInternalDelegate)Delegate.CreateDelegate(typeof(ScrollableTextAreaInternalDelegate), method);
            }

            EditorGUI_s_TextAreaHash_Field = typeof(EditorGUI).GetField("s_TextAreaHash", Flags.StaticAnyVisibility);

            if (EditorGUI_s_TextAreaHash_Field != null)
            {
                EditorGUI_s_TextAreaHash = (int)EditorGUI_s_TextAreaHash_Field.GetValue(null);
            }
        }

        private Vector2 scrollPosition;

        /// <summary>
        /// Draws the property in the Rect provided. This method does not support the GUILayout, and is only called by DrawPropertyImplementation if the GUICallType is set to Rect, which is not the default.
        /// If the GUICallType is set to Rect, both GetRectHeight and DrawPropertyRect needs to be implemented.
        /// If the GUICallType is set to GUILayout, implementing DrawPropertyLayout will suffice.
        /// </summary>
        /// <param name="label">The label. This can be null, so make sure your drawer supports that.</param>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            var stringValue = entry.SmartValue;
            var num = EditorStyles.textArea.CalcHeight(GUIHelper.TempContent(stringValue), GUIHelper.ContextWidth);
            var num2 = Mathf.Clamp(Mathf.CeilToInt(num / 13f), attribute.minLines, attribute.maxLines);
            var height = 32f + (float)((num2 - 1) * 13);
            var position = EditorGUILayout.GetControlRect(label != null, height);

            if (EditorGUI_ScrollableTextAreaInternal == null || EditorGUI_s_TextAreaHash_Field == null)
            {
                EditorGUI.LabelField(position, label, GUIHelper.TempContent("Cannot draw TextArea because Unity's internal API has changed."));
                return;
            }

            if (label != null)
            {
                Rect labelPosition = position;
                labelPosition.height = 16f;
                position.yMin += labelPosition.height;
                GUIHelper.IndentRect(ref labelPosition);
                EditorGUI.HandlePrefixLabel(position, labelPosition, label);
            }

            // Yes, this looks bad, but we're compensating for Unity's own EditorGUI.ScrollableTextAreaInternal not claiming any control ID during layout.
            if (Event.current.type == EventType.Layout)
            {
                GUIUtility.GetControlID(EditorGUI_s_TextAreaHash, FocusType.Keyboard, position);
            }

            entry.SmartValue = EditorGUI_ScrollableTextAreaInternal(position, entry.SmartValue, ref this.scrollPosition, EditorStyles.textArea);
        }
    }
}
#endif