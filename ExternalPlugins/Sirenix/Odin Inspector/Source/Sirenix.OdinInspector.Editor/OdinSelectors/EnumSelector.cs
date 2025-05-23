//-----------------------------------------------------------------------
// <copyright file="EnumSelector.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using System.Linq;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using System.Text;

    /// <summary>
    /// A feature-rich enum selector with support for flag enums.
    /// </summary>
    /// <example>
    /// <code>
    /// KeyCode someEnumValue;
    ///
    /// [OnInspectorGUI]
    /// void OnInspectorGUI()
    /// {
    ///     // Use the selector manually. See the documentation for OdinSelector for more information.
    ///     if (GUILayout.Button("Open Enum Selector"))
    ///     {
    ///         EnumSelector&lt;KeyCode&gt; selector = new EnumSelector&lt;KeyCode&gt;();
    ///         selector.SetSelection(this.someEnumValue);
    ///         selector.SelectionConfirmed += selection =&gt; this.someEnumValue = selection.FirstOrDefault();
    ///         selector.ShowInPopup(); // Returns the Odin Editor Window instance, in case you want to mess around with that as well.
    ///     }
    ///
    ///     // Draw an enum dropdown field which uses the EnumSelector popup:
    ///     this.someEnumValue = EnumSelector&lt;KeyCode&gt;.DrawEnumField(new GUIContent("My Label"), this.someEnumValue);
    /// }
    ///
    /// // All Odin Selectors can be rendered anywhere with Odin. This includes the EnumSelector.
    /// EnumSelector&lt;KeyCode&gt; inlineSelector;
    ///
    /// [ShowInInspector]
    /// EnumSelector&lt;KeyCode&gt; InlineSelector
    /// {
    ///     get { return this.inlineSelector ?? (this.inlineSelector = new EnumSelector&lt;KeyCode&gt;()); }
    ///     set { }
    /// }
    /// </code>
    /// </example>
    /// <seealso cref="OdinSelector{T}"/>
    /// <seealso cref="TypeSelector"/>
    /// <seealso cref="TypeSelectorV2"/>
    /// <seealso cref="GenericSelector{T}"/>
    /// <seealso cref="OdinMenuTree"/>
    /// <seealso cref="OdinEditorWindow"/>
    public class EnumSelector<T> : OdinSelector<T>
    {
        private static readonly StringBuilder SB = new StringBuilder();
        private static readonly StringBuilder tooltipSB = new StringBuilder();
        private static readonly Func<T, T, bool> EqualityComparer = PropertyValueEntry<T>.EqualityComparer;
        private static Color highlightLineColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0, 1f) : new Color(0.015f, 0.68f, 0.015f, 1f);
        private static Color selectedMaskBgColor = EditorGUIUtility.isProSkin ? new Color(0.5f, 1f, 0, 0.1f) : new Color(0.02f, 0.537f, 0, 0.31f);
        private static readonly string title = typeof(T).Name.SplitPascalCase();
        private float maxEnumLabelWidth = 0;
        private ulong curentValue;
        private ulong curentMouseOverValue;

        public static bool DrawSearchToolbar = true;

        /// <summary>
        /// By default, the enum type will be drawn as the title for the selector. No title will be drawn if the string is null or empty.
        /// </summary>
        public override string Title
        {
            get
            {
                if (GeneralDrawerConfig.Instance.DrawEnumTypeTitle)
                {
                    return title;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is flag enum.
        /// </summary>
        public bool IsFlagEnum { get { return EnumTypeUtilities<T>.IsFlagEnum; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumSelector{T}"/> class.
        /// </summary>
        public EnumSelector()
        {
            if (!typeof(T).IsEnum)
            {
                throw new NotSupportedException(typeof(T).GetNiceFullName() + " is not an enum type.");
            }

            if (Event.current != null)
            {
                foreach (var item in Enum.GetNames(typeof(T)))
                {
                    maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(item)).x);
                }

                if (this.Title != null)
                {
                    var titleAndSearch = Title + "                      ";
                    maxEnumLabelWidth = Mathf.Max(maxEnumLabelWidth, SirenixGUIStyles.Label.CalcSize(new GUIContent(titleAndSearch)).x);
                }
            }
        }

        /// <summary>
        /// Populates the tree with all enum values.
        /// </summary>
        protected override void BuildSelectionTree(OdinMenuTree tree)
        {
            tree.Selection.SupportsMultiSelect = IsFlagEnum;
            tree.Config.DrawSearchToolbar = DrawSearchToolbar;
            tree.Config.SelectMenuItemsOnMouseDown = true;
            tree.Config.ConfirmSelectionOnDoubleClick = false;
            
            var enumVals = EnumTypeUtilities<T>.AllEnumMemberInfos;
            foreach (var item in enumVals)
            {
                if (item.Hide) continue;
                tree.Add(item.NiceName, item, item.Icon);
            }

            //tree.AddRange(enumValues, x => Enum.GetName(typeof(T), x).SplitPascalCase());
            
            if (IsFlagEnum)
            {
                tree.DefaultMenuStyle.Offset += 15;
                if (!enumVals.Where(x => x.Value != null).Select(x => Convert.ToInt64(x.Value)).Contains(0))
                {
                    tree.MenuItems.Insert(0, new OdinMenuItem(tree, GetNoneValueString(), new EnumTypeUtilities<T>.EnumMember()
                    {
                        Value = GetZeroValue(),
                        Name = "None",
                        NiceName = "None",
                        IsObsolete = false,
                        Message = ""
                    }));
                }
                tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumFlagItem);
                this.DrawConfirmSelectionButton = false;
            }
            else
            {
                tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumItem);
            }

            tree.EnumerateTree().ForEach(x => x.OnDrawItem += DrawEnumInfo);
        }

        private static T GetZeroValue()
        {
            var backingType = Enum.GetUnderlyingType(typeof(T));

            // Yes, this is insane. Yes, C# makes us do this.
            object backingZero = Convert.ChangeType(0, backingType);
            return (T)backingZero;
        }

        private void DrawEnumInfo(OdinMenuItem obj)
        {
            if (!(obj.Value is EnumTypeUtilities<T>.EnumMember))
            {
                return;
            }

            var member = (EnumTypeUtilities<T>.EnumMember)obj.Value;
            var hasMessage = !string.IsNullOrEmpty(member.Message);

            if (member.IsObsolete)
            {
                var rect = obj.Rect.Padding(5, 3).AlignRight(16).AlignCenterY(16);
                GUI.DrawTexture(rect, EditorIcons.TestInconclusive);
            }
            else if (hasMessage)
            {
                var rect = obj.Rect.Padding(5, 3).AlignRight(16).AlignCenterY(16);
                GUI.DrawTexture(rect, EditorIcons.ConsoleInfoIcon);
            }

            if (hasMessage)
            {
                GUI.Label(obj.Rect, new GUIContent("", member.Message));
            }
        }

        private bool wasMouseDown = false;

        private void DrawEnumItem(OdinMenuItem obj)
        {
            Rect clickableRect = obj.Rect;

            if (obj.ChildMenuItems.Count > 0)
            {
                if (obj.Style.AlignTriangleLeft)
                {
                    clickableRect.xMin += obj.Style.TrianglePadding + obj.Style.TriangleSize;
                }
                else
                {
                    clickableRect.xMax -= obj.Style.TrianglePadding + obj.Style.TriangleSize;
                }
            }
            
            if (Event.current.type == EventType.MouseDown && Event.current.IsMouseOver(clickableRect))
            {
                obj.Select();

                if (obj.ChildMenuItems.Count == 0)
                {
                    // NOTE: the Toggle button won't be called otherwise.
                    Event.current.Use();
                }
                
                wasMouseDown = true;
            }

            if (wasMouseDown)
            {
                GUIHelper.RequestRepaint();
            }

            if (wasMouseDown == true && Event.current.type == EventType.MouseDrag && Event.current.IsMouseOver(clickableRect) && obj.Value != null)
            {
                obj.Select();
            }

            if (Event.current.type == EventType.MouseUp)
            {
                wasMouseDown = false;
                if (obj.IsSelected && Event.current.IsMouseOver(clickableRect) && obj.Value != null)
                {
                    obj.MenuTree.Selection.ConfirmSelection();
                }
            }
        }

        [OnInspectorGUI, PropertyOrder(-1000)]
        private void SpaceToggleEnumFlag()
        {
            if (this.SelectionTree != OdinMenuTree.ActiveMenuTree)
            {
                return;
            }

            if (IsFlagEnum && Event.current.keyCode == KeyCode.Space && Event.current.type == EventType.KeyDown && this.SelectionTree != null)
            {
                foreach (var item in this.SelectionTree.Selection)
                {
                    this.ToggleEnumFlag(item);
                }

                this.TriggerSelectionChanged();

                Event.current.Use();
            }
        }

        /// <summary>
        /// When ShowInPopup is called, without a specified window width, this method gets called.
        /// Here you can calculate and give a good default width for the popup.
        /// The default implementation returns 0, which will let the popup window determine the width itself. This is usually a fixed value.
        /// </summary>
        protected override float DefaultWindowWidth()
        {
            return Mathf.Clamp(maxEnumLabelWidth + 50, 160, 400);
        }

        private void DrawEnumFlagItem(OdinMenuItem obj)
        {
            Rect clickableRect = obj.Rect;

            if (obj.ChildMenuItems.Count > 0)
            {
                if (obj.Style.AlignTriangleLeft)
                {
                    clickableRect.xMin += obj.Style.TrianglePadding + obj.Style.TriangleSize;
                }
                else
                {
                    clickableRect.xMax -= obj.Style.TrianglePadding + obj.Style.TriangleSize;
                }
            }
            
            if ((Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) && Event.current.IsMouseOver(clickableRect) && obj.Value != null)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    ToggleEnumFlag(obj);

                    this.TriggerSelectionChanged();
                }
                Event.current.Use();
            }

            if (Event.current.type == EventType.Repaint && obj.Value != null)
            {
                var val = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
                var isPowerOfTwo = (val & (val - 1)) == 0;

                if (val != 0 && !isPowerOfTwo)
                {
                    var isMouseOver = obj.Rect.Contains(Event.current.mousePosition);
                    if (isMouseOver)
                    {
                        curentMouseOverValue = val;
                    }
                    else if (val == curentMouseOverValue)
                    {
                        curentMouseOverValue = 0;
                    }
                }

                var chked = (val & this.curentValue) == val && !((val == 0 && this.curentValue != 0));
                var highlight = val != 0 && isPowerOfTwo && (val & this.curentMouseOverValue) == val && !((val == 0 && this.curentMouseOverValue != 0));

                if (highlight)
                {
                    EditorGUI.DrawRect(obj.Rect.AlignLeft(6).Padding(2), highlightLineColor);
                }

                if (chked || isPowerOfTwo)
                {
                    var rect = obj.Rect.AlignLeft(30).AlignCenter(EditorIcons.TestPassed.width, EditorIcons.TestPassed.height);
                    if (chked)
                    {
                        if (isPowerOfTwo)
                        {
                            if (!EditorGUIUtility.isProSkin)
                            {
                                var tmp = GUI.color;
                                GUI.color = new Color(1, 0.7f, 1, 1);
                                GUI.DrawTexture(rect, EditorIcons.TestPassed);
                                GUI.color = tmp;
                            }
                            else
                            {
                                GUI.DrawTexture(rect, EditorIcons.TestPassed);
                            }
                        }
                        else
                        {
                            EditorGUI.DrawRect(obj.Rect.AlignTop(obj.Rect.height - (EditorGUIUtility.isProSkin ? 1 : 0)), selectedMaskBgColor);
                        }
                    }
                    else
                    {
                        GUI.DrawTexture(rect, EditorIcons.TestNormal);
                    }
                }
            }
        }

        private void ToggleEnumFlag(OdinMenuItem obj)
        {
            var val = (ulong)Convert.ToInt64(GetMenuItemEnumValue(obj));
            if ((val & this.curentValue) == val)
            {
                this.curentValue = val == 0 ? 0 : (this.curentValue & ~val);
            }
            else
            {
                this.curentValue = this.curentValue | val;
            }

            if (Event.current.clickCount >= 2)
            {
                Event.current.Use();
            }
        }

        /// <summary>
        /// Gets the currently selected enum value.
        /// </summary>
        public override IEnumerable<T> GetCurrentSelection()
        {
            if (IsFlagEnum)
            {
                yield return (T)Enum.ToObject(typeof(T), this.curentValue);
            }
            else
            {
                if (this.SelectionTree.Selection.Count > 0)
                {
                    yield return (T)Enum.ToObject(typeof(T), GetMenuItemEnumValue(this.SelectionTree.Selection.Last()));
                }
            }
        }

        /// <summary>
        /// Selects an enum.
        /// </summary>
        public override void SetSelection(T selected)
        {
            if (IsFlagEnum)
            {
                this.curentValue = (ulong)Convert.ToInt64(selected);
            }
            else
            {
                var selection = this.SelectionTree.EnumerateTree().Where(x => Convert.ToInt64(GetMenuItemEnumValue(x)) == Convert.ToInt64(selected));
                this.SelectionTree.Selection.AddRange(selection);
            }
        }

        private static object GetMenuItemEnumValue(OdinMenuItem item)
        {
            if (item.Value is EnumTypeUtilities<T>.EnumMember)
            {
                var member = (EnumTypeUtilities<T>.EnumMember)item.Value;
                return member.Value;
            }

            return default(T);
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null, SdfIconType valueIcon = SdfIconType.None)
        {
            int id;
            bool hasFocus;
            Rect rect;
            Action<EnumSelector<T>> bindSelector;
            Func<IEnumerable<T>> getResult;

            SirenixEditorGUI.GetFeatureRichControlRect(label, out id, out hasFocus, out rect);

            if (DrawSelectorButton(rect, contentLabel, valueIcon, style ?? EditorStyles.popup, id, true, out bindSelector, out getResult))
            {
                var selector = new EnumSelector<T>();

                if (!EditorGUI.showMixedValue)
                {
                    selector.SetSelection(value);
                }

                var window = selector.ShowInPopup(rect);

                if (EnumTypeUtilities<T>.IsFlagEnum)
                {
                    window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
                }

                bindSelector(selector);

                if ((int)Application.platform == 16) // LinuxEditor
                {
                    GUIHelper.ExitGUI(true);
                }
            }

            if (getResult != null)
            {
                value = getResult().FirstOrDefault();
            }

            return value;
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(GUIContent label, T value, GUIStyle style = null)
        {
            string display;
            SdfIconType icon = SdfIconType.None;
            string tooltip = "";

            if (EditorGUI.showMixedValue)
            {
                display = SirenixEditorGUI.MixedValueDashChar;
            }
            else
            {
                display = GetValueString(value, out icon, out tooltip);
            }

            return DrawEnumField(label, new GUIContent(display, tooltip), value, style, icon);
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(Rect rect, GUIContent label, GUIContent contentLabel, T value, GUIStyle style = null, SdfIconType valueIcon = SdfIconType.None)
        {
            int id;
            bool hasFocus;
            Action<EnumSelector<T>> bindSelector;
            Func<IEnumerable<T>> getResult;

            rect = SirenixEditorGUI.GetFeatureRichControl(rect, label, out id, out hasFocus);

            if (DrawSelectorButton(rect, contentLabel, valueIcon, style ?? EditorStyles.popup, id, true, out bindSelector, out getResult))
            {
                var selector = new EnumSelector<T>();

                if (!EditorGUI.showMixedValue)
                {
                    selector.SetSelection(value);
                }

                var window = selector.ShowInPopup(rect);

                if (EnumTypeUtilities<T>.IsFlagEnum)
                {
                    window.OnClose += selector.SelectionTree.Selection.ConfirmSelection;
                }

                bindSelector(selector);

                if ((int)Application.platform == 16) // LinuxEditor
                {
                    GUIHelper.ExitGUI(true);
                }
            }

            if (getResult != null)
            {
                value = getResult().FirstOrDefault();
            }

            return value;
        }

        /// <summary>
        /// Draws an enum selector field using the enum selector.
        /// </summary>
        public static T DrawEnumField(Rect rect, GUIContent label, T value, GUIStyle style = null)
        {
            var display = (EnumTypeUtilities<T>.IsFlagEnum && Convert.ToInt64(value) == 0) ? GetNoneValueString() : (EditorGUI.showMixedValue ? SirenixEditorGUI.MixedValueDashChar : value.ToString().SplitPascalCase());
            return DrawEnumField(rect, label, new GUIContent(display), value, style);
        }

        private static string GetNoneValueString()
        {
            var name = Enum.GetName(typeof(T), GetZeroValue());
            if (name != null) return name.SplitPascalCase();
            return "None";
        }

        private static string GetValueString(T value, out SdfIconType sdfIcon, out string tooltip)
        {
            var enumVals = EnumTypeUtilities<T>.AllEnumMemberInfos;
            sdfIcon = SdfIconType.None;
            tooltip = "";

            for (int i = 0; i < enumVals.Length; i++)
            {
                var val = enumVals[i];

                if (EqualityComparer(val.Value, value))
                {
                    sdfIcon = val.Icon;
                    tooltip = val.Tooltip;
                    return val.NiceName;
                }
            }

            if (EnumTypeUtilities<T>.IsFlagEnum)
            {
                var val64 = Convert.ToInt64(value);

                if (val64 == 0)
                {
                    return GetNoneValueString();
                }

                SB.Length = 0;
                tooltipSB.Length = 0;

                for (int i = 0; i < enumVals.Length; i++)
                {
                    var val = enumVals[i];
                    var flags = Convert.ToInt64(val.Value);
                    if (flags == 0) continue;
                    if ((val64 & flags) == flags)
                    {
                        tooltipSB.Append($"{val.NiceName}: {val.Tooltip}\n");
                        if (SB.Length > 0) SB.Append(", ");
                        SB.Append(val.NiceName);
                    }
                }

                tooltip = tooltipSB.ToString();
                return SB.ToString();
            }

            //var display = (isFlagEnum && Convert.ToInt64(value) == 0) ? GetNoneValueString() : (EditorGUI.showMixedValue ? SirenixEditorGUI.MixedValueDashChar : GetValueString(value));

            return value.ToString().SplitPascalCase();
        }
    }
}
#endif