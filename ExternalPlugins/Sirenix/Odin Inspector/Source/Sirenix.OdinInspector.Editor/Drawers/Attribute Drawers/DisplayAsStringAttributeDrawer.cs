//-----------------------------------------------------------------------
// <copyright file="DisplayAsStringAttributeDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#if UNITY_EDITOR
#define ODIN_PREVIEW
namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

#pragma warning disable

    using Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;
    using System.Text.RegularExpressions;
    using System;

    /// <summary>
    /// Draws properties marked with <see cref="DisplayAsStringAttribute"/>.
    /// Calls the properties ToString method to get the string to draw.
    /// </summary>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="DetailedInfoBoxAttribute"/>
    /// <seealso cref="MultiLinePropertyAttribute"/>
    /// <seealso cref="MultilineAttribute"/>
    [DrawerPriority(0, 0, 999.9995)] // Prioritize over other default Odin attribute drawers, which are 999.999
    public sealed class DisplayAsStringAttributeDrawer<T> : OdinAttributeDrawer<DisplayAsStringAttribute, T>, IDefinesGenericMenuItems
    {
        private GUIStyle labelStyle;
        private readonly Regex richTextTags = new Regex(@"<\/?\s*(b|i|size|color|material|quad)(\s*[^>]*)?>");

        public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
        {
            genericMenu.AddItem(new GUIContent("Copy to clipboard"), false, () =>
            {
                var str = this.ValueEntry.SmartValue == null ? "Null" : this.ValueEntry.SmartValue.ToString();
                str = this.richTextTags.Replace(str, string.Empty);
                GUIUtility.systemCopyBuffer = str;
            });

            genericMenu.AddItem(new GUIContent("Copy to clipboard (include rich text tags)"), false, () =>
            {
                var str = this.ValueEntry.SmartValue == null ? "Null" : this.ValueEntry.SmartValue.ToString();
                GUIUtility.systemCopyBuffer = str;
            });
        }

        protected override void Initialize()
        {
            TextAnchor alignment;

            switch (this.Attribute.Alignment)
            {
                case TextAlignment.Right:
                    alignment = TextAnchor.MiddleRight;
                    break;
                case TextAlignment.Center:
                    alignment = TextAnchor.MiddleCenter;
                    break;
                case TextAlignment.Left:
                default:
                    alignment = TextAnchor.MiddleLeft;
                    break;
            }

            this.labelStyle = new GUIStyle(EditorStyles.label)
            {
                alignment = alignment,
                richText = this.Attribute.EnableRichText,
                stretchWidth = !this.Attribute.Overflow,
                wordWrap = !this.Attribute.Overflow,
                fontSize = this.Attribute.FontSize,
            };
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;
            var attribute = this.Attribute;

            if (entry.Property.ChildResolver is ICollectionResolver)
            {
                this.CallNextDrawer(label);
                return;
            }

            string str;

            if (string.IsNullOrEmpty(this.Attribute.Format) == false && entry.SmartValue is IFormattable f)
            {
                str = f.ToString(this.Attribute.Format, null);
            }
            else
            {
                str = entry.SmartValue == null ? "Null" : entry.SmartValue.ToString();
            }

            if (label == null)
            {
                EditorGUILayout.LabelField(str, this.labelStyle, GUILayoutOptions.MinWidth(0));
            }
            else if (!attribute.Overflow)
            {
                var stringLabel = GUIHelper.TempContent(str);
                var position = EditorGUILayout.GetControlRect(false, this.labelStyle.CalcHeight(stringLabel, entry.Property.LastDrawnValueRect.width - GUIHelper.BetterLabelWidth), GUILayoutOptions.MinWidth(0));
                var rect = EditorGUI.PrefixLabel(position, label);
                GUI.Label(rect, stringLabel, this.labelStyle);
            }
            else
            {
                int id;
                bool keyboard;
                Rect rect;
                SirenixEditorGUI.GetFeatureRichControlRect(label, out id, out keyboard, out rect);
                GUI.Label(rect, str, this.labelStyle);
            }
        }
    }
}
#endif
#endif