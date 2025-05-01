//-----------------------------------------------------------------------
// <copyright file="SdfIconOverviewWindow.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#pragma warning disable

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using System;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using Sirenix.Reflection.Editor;

    public class SdfIconOverviewWindow : EditorWindow
    {
        [NonSerialized] private SdfIcon[] icons;
        private string prevSearch = "123";
        private string searchFilter;
        private GUIStyle padding;
        private int size = 50;
        private float scrollPos;
        private float scrollMax;


        private Color backColor = new Color(0.1f, 0.1f, 0.1f, 1);
        private Color iconColor = new Color(1, 1, 1, 1);
        private float f;
        internal Action<SdfIconType> onSelect;
        internal SdfIconType? selected;

        public SdfIconType? MouseOverIcon { get; private set; }

        private void OnEnable()
        {
            this.titleContent = new GUIContent("Sdf Icon Overview");
        }

        public static void ShowWindow() => GetWindow<SdfIconOverviewWindow>();

        void OnGUI()
        {
            // Update
            if (Event.current.type == EventType.Layout)
            {
                if (prevSearch != searchFilter || (icons == null || icons.Length == 0))
                {
                    if (string.IsNullOrEmpty(searchFilter))
                    {
                        icons = SdfIcons.AllIcons;

                    }
                    else
                    {
                        icons = SdfIcons.AllIcons.Where(x => FuzzySearch.Contains(searchFilter, x.Name)).ToArray();
                    }

                    prevSearch = searchFilter;
                    padding = padding ?? new GUIStyle() { padding = new RectOffset(20, 20, 10, 10), };
                }
            }

            // Draw filters
            GUILayout.BeginHorizontal(padding);
            {
                GUILayout.BeginVertical();
                searchFilter = EditorGUILayout.TextField("Search", searchFilter);
                size = (int)EditorGUILayout.Slider("Size", size, 10, 128);
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
                backColor = EditorGUILayout.ColorField("Preview back color", backColor);
                iconColor = EditorGUILayout.ColorField("Preview icon color", iconColor);
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            this.scrollPos = EditorGUILayout.BeginScrollView(new Vector2(0, this.scrollPos)).y;
            {
                var area = GUILayoutUtility.GetRect(0, this.scrollMax, GUIStyle.none, GUILayoutOptions.ExpandHeight());
                EditorGUI.DrawRect(area, backColor);

                // Draw Icons
                var draw = Event.current.type == EventType.Repaint;
                if (draw || Event.current.type == EventType.MouseDown)
                {
                    var selectedIndex = selected.HasValue ? (int)selected.Value : -1;
                    float yMax = 0f;
                    var padding = 10f;
                    var s = size + padding;
                    int num = (int)(area.width / s);
                    var remain = (area.width % s) / num;
                    s += remain;
                    area.width += 1;
                    var mp = Event.current.mousePosition;
                    int mouseOver = -1;
                    Rect mouseOverRect = default;

                    for (int i = 0; i < icons.Length; i++)
                    {
                        var cell = area.SplitGrid(s, s, i);
                        yMax = Math.Max(yMax, cell.bottom);

                        if (cell.Contains(mp))
                        {
                            mouseOver = i;
                            cell = cell.AlignCenter(size + padding * 2, size + padding * 2);
                            mouseOverRect = cell;

                        }
                        else
                        {
                            cell = cell.AlignCenter(size, size);
                        }

                        if (selectedIndex == i)
                        {
                            var selectedBgColor = iconColor;
                            selectedBgColor.a *= 0.4f;
                            GUI.DrawTexture(cell.Expand(5, 5), Texture2D.whiteTexture, ScaleMode.StretchToFill, true, 0, selectedBgColor, 0, 5);
                        }

                        SdfIcons.DrawIcon(cell, icons[i], iconColor, backColor);
                    }

                    this.scrollMax = yMax;
                    this.MouseOverIcon = null;
                    if (mouseOver >= 0)
                    {
                        var style = new GUIStyle(SirenixGUIStyles.WhiteLabel);
                        style.fontStyle = FontStyle.Bold;

                        var name = icons[mouseOver].Name;
                        this.MouseOverIcon = (SdfIconType)mouseOver;
                        var size = style.CalcSize(new GUIContent(name));
                        var pos = Event.current.mousePosition + new Vector2(30, 5);
                        var rect = new Rect(pos, size);

                        var push = rect.xMax - area.xMax + 20;
                        if (push > 0)
                        {
                            rect.x -= push;
                        }

                        EditorGUI.DrawRect(rect.Expand(10, 5), Color.black);
                        GUI.Label(rect, name, style);

                        if (mouseOverRect.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown)
                        {
                            if (onSelect != null)
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    onSelect((SdfIconType)icons[mouseOver].Index);
                                };
                            }
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();

            this.Repaint();

            if (Event.current.type == EventType.MouseDown)
            {
                GUIHelper.RemoveFocusControl();
            }
        }

        public static Rect Split(Rect rect, int index, int length)
        {
            if (length == 1)
                return rect;

            var count = Math.Max(1, (int)(Math.Sqrt(length - 1))) + 1;

            var x = index % count;
            var y = index / count;

            rect.width = rect.width / count;
            rect.height = rect.height / count;
            rect.x = rect.x + x * rect.width;
            rect.y = rect.y + y * rect.height;
            return rect;
        }
    }

    public static class SdfIconSelector
    {
        private static int selectorControlId = -1;
        private static SdfIconOverviewWindow overview;
        private static SdfIconType? selectorIcon = null;
        private static EditorWindow window;

        private static SdfIconType? MouseOverIcon => overview ? overview.MouseOverIcon : null;

        public static SdfIconType SelectIcon(SdfIconType selected, int controlId, bool show)
        {
            if (show)
            {
                window = GUIHelper.CurrentWindow;
                selectorControlId = controlId;
                overview = EditorWindow.CreateInstance<SdfIconOverviewWindow>();
                overview.ShowAuxWindow();
                overview.selected = selected;
                overview.onSelect = (x) =>
                {
                    if (window)
                    {
                        window.Repaint();
                        selectorIcon = x;
                    }
                    overview.Close();
                    overview = null;
                };
            }

            if (selectorIcon.HasValue && controlId == selectorControlId)
            {
                EditorGUIUtility.hotControl = controlId;
                window = null;
                GUI.changed = true;
                var val = selectorIcon.Value;
                selectorIcon = null;
                selectorControlId = -1;
                overview = null;
                return val;
            }

            return selected;
        }

        public static SdfIconType DrawIconSelectorDropdownField(GUIContent label, SdfIconType selected)
        {
            Rect position = EditorGUILayout.GetControlRect();

            return DrawIconSelectorDropdownField(position, label, selected);
        }

        public static SdfIconType DrawIconSelectorDropdownField(Rect rect, GUIContent label, SdfIconType selected)
        {
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            GUIContent valLabel = GUIHelper.TempContent(selected.ToString(), EditorIcons.Transparent.Active);

            bool pressed = GUI_Internals.Button(rect, FocusType.Keyboard, valLabel, EditorStyles.miniPullDown, out int id);

            SdfIcons.DrawIcon(rect.Padding(4).AlignLeft(rect.height), selected);

            selected = SdfIconSelector.SelectIcon(selected, id, pressed);
            return selected;
        }

        public static SdfIconType DrawIconSelectorDropdownField(Rect rect, GUIContent label, SdfIconType selected, out SdfIconType? mouseOverIcon)
        {
            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            GUIContent valLabel = GUIHelper.TempContent(selected.ToString(), EditorIcons.Transparent.Active);

            bool pressed = GUI_Internals.Button(rect, FocusType.Keyboard, valLabel, EditorStyles.miniPullDown, out int id);

            SdfIcons.DrawIcon(rect.Padding(4).AlignLeft(rect.height), selected);

            selected = SdfIconSelector.SelectIcon(selected, id, pressed);

            if (SdfIconSelector.selectorControlId == id)
            {
                GUIHelper.RequestRepaint();
                mouseOverIcon = MouseOverIcon;
            }
            else
            {
                mouseOverIcon = null;
            }

            return selected;
        }
    }
}
#endif