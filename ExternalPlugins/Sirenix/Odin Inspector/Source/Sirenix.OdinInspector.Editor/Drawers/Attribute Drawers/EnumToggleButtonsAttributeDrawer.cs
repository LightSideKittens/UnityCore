//-----------------------------------------------------------------------
// <copyright file="EnumToggleButtonsAttributeDrawer.cs" company="Sirenix ApS">
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
    using System.Linq;
    using UnityEngine;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using StringExtensions = Sirenix.Utilities.StringExtensions;

    /// <summary>
    /// Draws an enum in a horizontal button group instead of a dropdown.
    /// </summary>
    public class EnumToggleButtonsAttributeDrawer<T> : OdinAttributeDrawer<EnumToggleButtonsAttribute, T>
    {
        private static Color ActiveColor = EditorGUIUtility.isProSkin ? Color.white : new Color(0.802f, 0.802f, 0.802f, 1f);
        private static Color InactiveColor = EditorGUIUtility.isProSkin ? new Color(0.75f, 0.75f, 0.75f, 1f) : Color.white;

        private (GUIContent name, ulong value, SdfIconType icon, string tooltip)[] Members;
        //private ulong[] Values;
        private float[] NameSizes;
        private bool IsFlagsEnum;
        private List<int> ColumnCounts;
        private float PreviousControlRectWidth;

        /// <summary>
        /// Returns <c>true</c> if the drawer can draw the type.
        /// </summary>
        public override bool CanDrawTypeFilter(Type type)
        {
            return type.IsEnum;
        }

        protected override void Initialize()
        {
            var enumType = this.ValueEntry.TypeOfValue;
            var enumNames = Enum.GetNames(enumType);

            this.Members = EnumTypeUtilities<T>.VisibleEnumMemberInfos.Select(x => (
                    new GUIContent(x.NiceName),
                    TypeExtensions.GetEnumBitmask(Enum.Parse(enumType, x.Name), enumType),
                    x.Icon,
                    x.Tooltip
                )).ToArray();

            this.IsFlagsEnum = enumType.IsDefined<FlagsAttribute>();
            this.NameSizes = this.Members.Select(x => SirenixGUIStyles.MiniButtonMid.CalcSize(x.name).x).ToArray();
            this.ColumnCounts = new List<int>() { this.NameSizes.Length };
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var entry = this.ValueEntry;

            var t = entry.WeakValues[0].GetType();
            int i = 1;
            for (; i < entry.WeakValues.Count; i++)
            {
                if (t != entry.WeakValues[i].GetType())
                {
                    SirenixEditorGUI.ErrorMessageBox("ToggleEnum does not support multiple different enum types.");
                    return;
                }
            }


            ulong value = TypeExtensions.GetEnumBitmask(entry.SmartValue, typeof(T));

            Rect controlRect = new Rect();

            i = 0;
            for (int j = 0; j < this.ColumnCounts.Count; j++)
            {
                int id;
                bool hasFocus;
                Rect rect;
                SirenixEditorGUI.GetFeatureRichControlRect(j == 0 ? label : GUIContent.none, out id, out hasFocus, out rect);

                if (j == 0)
                {
                    controlRect = rect;
                }
                else
                {
                    rect.xMin = controlRect.xMin;
                }

                var xMax = rect.xMax;
                rect.width /= this.ColumnCounts[j];
                rect.width = (int)rect.width;
                int from = i;
                int to = i + this.ColumnCounts[j];
                for (; i < to; i++)
                {
                    bool selected;
                    var member = this.Members[i];

                    if (this.IsFlagsEnum)
                    {
                        var mask = TypeExtensions.GetEnumBitmask(member.value, typeof(T));

                        if (value == 0)
                            selected = mask == 0;
                        else if (mask != 0)
                            selected = (mask & value) == mask;
                        else
                            selected = false;
                    }
                    else
                    {
                        selected = member.value == value;
                    }


                    GUIStyle style;
                    Rect btnRect = rect;
                    if (i == from && i == to - 1)
                    {
                        style = SirenixGUIStyles.MiniButton;
                        btnRect.x -= 1;
                        btnRect.xMax = xMax + 1;
                    }
                    else if (i == from)
                        style = SirenixGUIStyles.MiniButtonLeft;
                    else if (i == to - 1)
                    {
                        style = SirenixGUIStyles.MiniButtonRight;
                        btnRect.xMax = xMax;
                    }
                    else
                        style = SirenixGUIStyles.MiniButtonMid;

                    member.name.tooltip = member.tooltip;
                    if (SirenixEditorGUI.SDFIconButton(btnRect, member.name, member.icon, IconAlignment.LeftOfText, style, selected))
                    {
                        GUIHelper.RemoveFocusControl();

                        if (!this.IsFlagsEnum || Event.current.button == 1 || Event.current.modifiers == EventModifiers.Control)
                        {
                            entry.WeakSmartValue = Enum.ToObject(typeof(T), member.value);
                        }
                        else
                        {
                            if (member.value == 0)
                                value = 0;
                            else if (selected)
                                value &= ~member.value;
                            else
                                value |= member.value;

                            entry.WeakSmartValue = Enum.ToObject(typeof(T), value);
                        }

                        GUIHelper.RequestRepaint();
                    }

                    rect.x += rect.width;
                }
            }

            if (Event.current.type == EventType.Repaint && this.PreviousControlRectWidth != controlRect.width)
            {
                this.PreviousControlRectWidth = controlRect.width;

                float maxBtnWidth = 0;
                int row = 0;
                this.ColumnCounts.Clear();
                this.ColumnCounts.Add(0);
                i = 0;
                for (; i < this.NameSizes.Length; i++)
                {
                    float btnWidth = this.NameSizes[i] + 3;
                    int columnCount = ++this.ColumnCounts[row];
                    float columnWidth = controlRect.width / columnCount;

                    maxBtnWidth = Mathf.Max(btnWidth, maxBtnWidth);

                    if (maxBtnWidth > columnWidth && columnCount > 1)
                    {
                        this.ColumnCounts[row]--;
                        this.ColumnCounts.Add(1);
                        row++;
                        maxBtnWidth = btnWidth;
                    }
                }
            }
        }
    }
}
#endif