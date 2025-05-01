//-----------------------------------------------------------------------
// <copyright file="PlaceholderLabelAttributeDrawer.cs" company="Sirenix ApS">
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

    //using UnityEngine;
    //using Sirenix.OdinInspector.Editor;
    //using Sirenix.Utilities.Editor;
    //using Sirenix.Utilities;
    //using Sirenix.OdinInspector.Editor.ValueResolvers;
    //using UnityEditor;

//#if !ODIN_BETA && !SIRENIX_INTERNAL
//#error TODO, add attribute examples for PlaceholderLabelAttribute. Also polish it up a bit more.
//#endif

//    public class PlaceholderLabelAttributeDrawer : OdinAttributeDrawer<PlaceholderLabelAttribute, string>
//    {
//        private GUIStyle style;
//        private ValueResolver<string> labelResolver;

//        protected override void Initialize()
//        {
//            this.style = new GUIStyle(EditorStyles.textField);

//            if (this.Attribute.LabelText != null)
//            {
//                this.labelResolver = ValueResolver.GetForString(this.Property, this.Attribute.LabelText);
//            }
//        }

//        protected override void DrawPropertyLayout(GUIContent label)
//        {
//            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, EditorStyles.textField);
//            var indent = 0f;
//            var iconSize = 14;
//            var iconPadding = 5;

//            if (this.Attribute.Icon != SdfIconType.None)
//            {
//                indent += iconSize;
//                indent += iconPadding;
//            }

//            this.style.padding.left = EditorStyles.textField.padding.left + (int)indent;

//            this.ValueEntry.SmartValue = EditorGUI.TextField(rect, this.ValueEntry.SmartValue, this.style);

//            if (Event.current.type == EventType.Repaint)
//            {
//                if (this.Attribute.Icon != SdfIconType.None)
//                {
//                    var iconRect = rect;

//                    iconRect.x += EditorStyles.textField.padding.left;
//                    iconRect.width = iconSize;
//                    iconRect = iconRect.AlignCenterY(iconSize);
//                    iconRect.y += 1;

//                    var activeColor = SirenixGUIStyles.Label.normal.textColor;
//                    SdfIcons.DrawIcon(iconRect, this.Attribute.Icon, activeColor);
//                }

//                if (string.IsNullOrEmpty(this.ValueEntry.SmartValue))
//                {
//                    string labelText = null;

//                    if (this.labelResolver != null)
//                    {
//                        labelText = this.labelResolver.GetValue();
//                    }
//                    else if (!string.IsNullOrEmpty(label.text))
//                    {
//                        labelText = label.text;
//                    }

//                    if (labelText != null)
//                    {
//                        rect.xMin += indent;
//                        GUI.Label(rect, labelText, SirenixGUIStyles.LeftAlignedGreyLabel);
//                    }
//                }
//            }
//        }
//    }
}
#endif