//-----------------------------------------------------------------------
// <copyright file="SirenixEditorGUI.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System.Globalization;
    using System;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;
    using Utilities;
    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Reflection.Editor;
    using Sirenix.Utilities.Editor.Expressions.Internal;

    /// <summary>
    /// Collection of various editor GUI functions.
    /// </summary>
    [InitializeOnLoad]
    public static class SirenixEditorGUI
    {
        private static readonly Dictionary<Type, Delegate> DynamicFieldDrawers = new Dictionary<Type, Delegate>()
        {
            { typeof(char),     (Func<GUIContent, char, GUILayoutOption[], char>)       ((label, value, options) => {
                string str = value.ToString();
                str = SirenixEditorFields.TextField(label, str, options);
                if (str.Length == 0) return default(char);
                else return str[0];
            }) },
            { typeof(string),   (Func<GUIContent, string, GUILayoutOption[], string>)   ((label, value, options) => SirenixEditorFields.TextField(label, value, options)) },
            { typeof(sbyte),    (Func<GUIContent, sbyte, GUILayoutOption[], sbyte>)     ((label, value, options) => (sbyte)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), sbyte.MinValue, sbyte.MaxValue)) },
            { typeof(byte),     (Func<GUIContent, byte, GUILayoutOption[], byte>)       ((label, value, options) => (byte)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), byte.MinValue, byte.MaxValue)) },
            { typeof(short),    (Func<GUIContent, short, GUILayoutOption[], short>)     ((label, value, options) => (short)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), short.MinValue, short.MaxValue)) },
            { typeof(ushort),   (Func<GUIContent, ushort, GUILayoutOption[], ushort>)   ((label, value, options) => (ushort)Mathf.Clamp(SirenixEditorFields.IntField(label, value, options), ushort.MinValue, ushort.MaxValue)) },
            { typeof(int),      (Func<GUIContent, int, GUILayoutOption[], int>)         ((label, value, options) => SirenixEditorFields.IntField(label, value, options)) },
            { typeof(uint),     (Func<GUIContent, uint, GUILayoutOption[], uint>)       ((label, value, options) => (uint)Mathf.Clamp(EditorGUILayout.LongField(label, value, options), uint.MinValue, uint.MaxValue)) },
            { typeof(long),     (Func<GUIContent, long, GUILayoutOption[], long>)       ((label, value, options) => EditorGUILayout.LongField(label, value, options)) },
            { typeof(ulong),    (Func<GUIContent, ulong, GUILayoutOption[], ulong>)     ((label, value, options) => {
                string str = value.ToString(CultureInfo.InvariantCulture);
                str = EditorGUILayout.DelayedTextField(label, str, EditorStyles.textField);

                ulong newValue;

                if (GUI.changed && ulong.TryParse(str, NumberStyles.Any, null, out newValue))
                {
                    value = newValue;
                }

                return value;
            }) },
            { typeof(float),    (Func<GUIContent, float, GUILayoutOption[], float>)     ((label, value, options) => SirenixEditorFields.FloatField(label, value, options)) },
            { typeof(double),   (Func<GUIContent, double, GUILayoutOption[], double>)   ((label, value, options) => EditorGUILayout.DoubleField(label, value, options)) },
            { typeof(decimal),  (Func<GUIContent, decimal, GUILayoutOption[], decimal>) ((label, value, options) => {
                string str = value.ToString(CultureInfo.InvariantCulture);
                str = EditorGUILayout.DelayedTextField(label, str, EditorStyles.textField);
                decimal newValue;

                if (GUI.changed && decimal.TryParse(str, NumberStyles.Any, null, out newValue))
                {
                    value = newValue;
                }

                return value;
            }) },
            { typeof(Guid),   (Func<GUIContent, Guid, GUILayoutOption[], Guid>)   ((label, value, options) => SirenixEditorFields.GuidField(label, value, options)) },
        };

        private static readonly object fadeGroupKey = new object();
        private static readonly object shakeableGroupKey = new object();
        private static readonly object menuListKey = new object();
        private static readonly object animatedTabGroupKey = new object();
        private static readonly GUIScopeStack<Rect> verticalListBorderRects = new GUIScopeStack<Rect>();
        private static readonly List<int> currentListItemIndecies = new List<int>();
        private static int animatingFadeGroupIndex = -1;
        private static int currentFadeGroupIndex = 0;
        private static int currentScope = 0;
        public static float currentDrawingToolbarHeight;
        private static float slideRectSensitivity = 0f;
        private static float? defaultFadeGroupDuration;
        private static float? tabPageSlideAnimationDuration;
        private static float? shakingAnimationDuration;
        private static bool? expandFoldoutByDefault;
        private static bool? showButtonResultsByDefault;
        private static object drawColorPaletteColorPickerKey = new object();
        private static BeginAutoScrollBoxInfo currentBeginAutoScrollBoxInfo;
        private static Color darkerLinerColor = EditorGUIUtility.isProSkin ? SirenixGUIStyles.BorderColor : new Color(0, 0, 0, 0.2f);
        private static Color lighterLineColor = EditorGUIUtility.isProSkin ? new Color(1.000f, 1.000f, 1.000f, 0.103f) : new Color(1, 1, 1, 1);
        private static Color darkerLinerColorThick = EditorGUIUtility.isProSkin ? new Color(0.11f, 0.11f, 0.11f, 0.294f) : new Color(0, 0, 0, 0.2f);
        private static Color lighterLineColorThick = EditorGUIUtility.isProSkin ? new Color(1.000f, 1.000f, 1.000f, 0.103f) : new Color(1, 1, 1, 1);
        private static Color sdfIconButtonSeparatorColor = EditorGUIUtility.isProSkin ? new Color(0.188f, 0.188f, 0.188f, 0.5f) : new Color(0.698f, 0.698f, 0.698f, 0.5f);
        private static float slideDeltaBuffer;
        private static VerticalMenuListInfo currentVerticalMenuListInfo;
        private static bool popMenuItemLabelColor = false;
        private static GUIStyle fadeGroupFix;
        private static GUIContent tempSDFIconTooltipContent;
        private static GUIStyle _sdfIconButtonLabelStyle;
        private static readonly GUIScopeStack<int> shakingGroupControlIdStack = new GUIScopeStack<int>();
        private static int currentShakingGroupControlId = -1;
        private static double shakingGroupStartTime = -1;

        private static GUIStyle SdfIconButtonLabelStyle
        {
            get
            {
                if (_sdfIconButtonLabelStyle == null)
                {
                    _sdfIconButtonLabelStyle = new GUIStyle()
                    {
                        alignment = TextAnchor.MiddleCenter,
                        padding = new RectOffset(0, 0, 0, 0),
                        clipping = TextClipping.Clip,
                    };
                }
                return _sdfIconButtonLabelStyle;
            }
        }

        private static GUIStyle legendBoxPadding = new GUIStyle()
        {
            padding = new RectOffset(2, 2, 13, 3),
            margin = new RectOffset(0, 0, 1, 1),
        };

        private static GUIStyle legendBoxNoLabelPadding = new GUIStyle()
        {
            padding = new RectOffset(2, 2, 3, 3),
            margin = new RectOffset(0, 0, 0, 1),
        };

        /// <summary>
        /// The mixed value dash character, to show when something has mixed values;
        /// </summary>
        public const string MixedValueDashChar = "—";
        private const string EXPAND_FOLDOUT_BY_DEFAULT = "SirenixEditorGUI.ExpandFoldoutByDefault";
        private const string SHOW_BUTTON_RESULTS_BY_DEFAULT = "SirenixEditorGUI.ShowButtonResultsByDefault";
        private const string DEFAULT_FADE_GROUP_DURATION = "SirenixEditorGUI.DefaultFadeGroupDuration";
        private const string TAB_PAGE_SLIDE_ANIMATION_DURATION = "SirenixEditorGUI.TabPageSlideAnimationDuration";
        private const string SHAKING_ANIMATION_DURATION = "SirenixEditorGUI.ShakingAnimationDuration";

        /// <summary>
        /// Default fade group animation duration.
        /// </summary>
        public static float DefaultFadeGroupDuration
        {
            get
            {
                if (defaultFadeGroupDuration == null)
                {
                    defaultFadeGroupDuration = EditorPrefs.GetFloat(DEFAULT_FADE_GROUP_DURATION, 0.13f);
                }

                return defaultFadeGroupDuration.Value;
            }
            set
            {
                defaultFadeGroupDuration = value;
                EditorPrefs.SetFloat(DEFAULT_FADE_GROUP_DURATION, value);
            }
        }

        /// <summary>
        /// Tab page slide animation duration.
        /// </summary>
        public static float TabPageSlideAnimationDuration
        {
            get
            {
                if (tabPageSlideAnimationDuration == null)
                {
                    tabPageSlideAnimationDuration = EditorPrefs.GetFloat(TAB_PAGE_SLIDE_ANIMATION_DURATION, 0.13f);
                }

                return tabPageSlideAnimationDuration.Value;
            }
            set
            {
                tabPageSlideAnimationDuration = value;
                EditorPrefs.SetFloat(TAB_PAGE_SLIDE_ANIMATION_DURATION, value);
            }
        }

        /// <summary>
        /// Shaking animation duration.
        /// </summary>
        public static float ShakingAnimationDuration
        {
            get
            {
                if (shakingAnimationDuration == null)
                {
                    shakingAnimationDuration = EditorPrefs.GetFloat(SHAKING_ANIMATION_DURATION, 0.5f);
                }

                return shakingAnimationDuration.Value;
            }
            set
            {
                shakingAnimationDuration = value;
                EditorPrefs.SetFloat(SHAKING_ANIMATION_DURATION, value);
            }
        }


        private static GUIStyle propertyLayoutFix;

        public static Rect BeginHorizontalPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            return BeginHorizontalPropertyLayout(label, out labelRect);
        }

        public static Rect BeginHorizontalPropertyLayout(GUIContent label, out Rect labelRect)
        {
            var lblWidth = GUIHelper.BetterLabelWidth;
            labelRect = new Rect();
            GUILayout.BeginHorizontal();

            if (label == null)
            {
                GUILayout.Space(GUIHelper.CurrentIndentAmount);
            }
            else
            {
                EditorGUILayout.PrefixLabel(label);
                labelRect = GUILayoutUtility.GetLastRect();
                GUILayout.Space(2);
            }

            var rect = EditorGUILayout.BeginHorizontal();
            GUIHelper.PushLabelWidth(lblWidth - GUIHelper.CurrentIndentAmount);
            GUIHelper.PushIndentLevel(0);
            return rect;
        }

        public static void EndHorizontalPropertyLayout()
        {
            GUIHelper.PopLabelWidth();
            GUIHelper.PopIndentLevel();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
        }

        public static Rect BeginVerticalPropertyLayout(GUIContent label)
        {
            Rect labelRect;
            return BeginVerticalPropertyLayout(label, out labelRect);
        }

        public static Rect BeginVerticalPropertyLayout(GUIContent label, out Rect labelRect)
        {
            var lblWidth = GUIHelper.BetterLabelWidth;
            labelRect = new Rect();
            GUILayout.BeginHorizontal();
            if (label == null)
            {
                GUILayout.Space(GUIHelper.CurrentIndentAmount);
            }
            else
            {
                EditorGUILayout.PrefixLabel(label);
                labelRect = GUILayoutUtility.GetLastRect();
                GUILayout.Space(2);
            }

            var rect = EditorGUILayout.BeginVertical();
            GUIHelper.PushLabelWidth(lblWidth - GUIHelper.CurrentIndentAmount);
            GUIHelper.PushIndentLevel(0);
            return rect;
        }

        public static void EndVerticalPropertyLayout()
        {
            GUIHelper.PopLabelWidth();
            GUIHelper.PopIndentLevel();
            GUILayout.Space(-2);
            EditorGUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        /// <summary>
        /// Expand foldouts by default.
        /// </summary>
        public static bool ExpandFoldoutByDefault
        {
            get
            {
                if (expandFoldoutByDefault == null)
                {
                    expandFoldoutByDefault = EditorPrefs.GetBool(EXPAND_FOLDOUT_BY_DEFAULT, false);
                }

                return expandFoldoutByDefault.Value;
            }
            set
            {
                expandFoldoutByDefault = value;
                EditorPrefs.SetBool(EXPAND_FOLDOUT_BY_DEFAULT, value);
            }
        }

        /// <summary>
        /// Show buttons results by default.
        /// </summary>
        public static bool ShowButtonResultsByDefault
        {
            get
            {
                if (showButtonResultsByDefault == null)
                {
                    showButtonResultsByDefault = EditorPrefs.GetBool(SHOW_BUTTON_RESULTS_BY_DEFAULT, true);
                }

                return showButtonResultsByDefault.Value;
            }
            set
            {
                showButtonResultsByDefault = value;
                EditorPrefs.SetBool(SHOW_BUTTON_RESULTS_BY_DEFAULT, value);
            }
        }

        /// <summary>
        /// Draws a GUI field for objects.
        /// </summary>
        /// <param name="rect">The rect to draw the field in.</param>
        /// <param name="label">The label of the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="objectType">The object type for the field.</param>
        /// <param name="allowSceneObjects">If set to <c>true</c> then allow scene objects to be assigned to the field.</param>
        /// <param name="isReadOnly">If set to <c>true</c> the field is readonly.</param>
        /// <returns>The object assigned to the field.</returns>
        [Obsolete("Use SirenixEditorFields.UnityObjectField instead")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static UnityEngine.Object ObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool isReadOnly = false)
        {
            bool removeFocus = false;
            if (isReadOnly)
            {
                if (Event.current.type == EventType.MouseDown)
                {
                    var objectFieldRect = new Rect(rect.x + 20, rect.y, rect.width - 20, 16);

                    if (new Rect(rect.xMax - 20, rect.y, 20, rect.height).Contains(Event.current.mousePosition))
                    {
                        Event.current.Use();
                    }
                    else if (objectFieldRect.Contains(Event.current.mousePosition))
                    {
                        removeFocus = true;
                    }
                }
            }

            // Sometimes a Unity value is *so null* that Unity can't even handle it
            // In these cases, we just pass in a straight null instead, to make Unity
            // feel better about itself.
            bool isNull = value == null;// || value.SafeIsUnityNull();

            var penRect = rect;
            penRect.x += penRect.width - 38;
            penRect.width = 20;

            BeginDrawOpenInspector(penRect, value, IndentLabelRect(rect, label != null));

            //rect.xMin += 4;
            value = label == null ?
                EditorGUI.ObjectField(rect, isNull ? null : value, objectType, allowSceneObjects) :
                EditorGUI.ObjectField(rect, label, isNull ? null : value, objectType, allowSceneObjects);

            EndDrawOpenInspector(penRect, value);

            if (removeFocus)
            {
                GUIHelper.RemoveFocusControl();
            }

            return value;
        }

        internal static Rect IndentLabelRect(Rect totalPosition, bool hasLabel)
        {
            if (!hasLabel)
            {
                return EditorGUI.IndentedRect(totalPosition);
            }
            else
            {
                return new Rect(totalPosition.x + GUIHelper.BetterLabelWidth, totalPosition.y, totalPosition.width - GUIHelper.BetterLabelWidth, totalPosition.height);
            }
        }

        /// <summary>
        /// Draws an GUI field for objects.
        /// </summary>
        /// <param name="label">The label for the field.</param>
        /// <param name="value">The value of the field.</param>
        /// <param name="objectType">The object type for the field.</param>
        /// <param name="allowSceneObjects">If set to <c>true</c> then allow scene objects to be assigned to the field.</param>
        /// <param name="isReadOnly">If set to <c>true</c> the field is readonly.</param>
        /// <returns>The object assigned to the field.</returns>
        [Obsolete("Use SirenixEditorFields.UnityObjectField instead")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static UnityEngine.Object ObjectField(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool isReadOnly = false)
        {
            var rect = EditorGUILayout.GetControlRect(label != null);
            return ObjectField(rect, label, value, objectType, allowSceneObjects, isReadOnly);
        }

        /// <summary>
        /// Draws a GUI field for objects.
        /// </summary>
        /// <param name="key">The key for the field.</param>
        /// <param name="type">The type.</param>
        /// <param name="label">The label for the field.</param>
        /// <param name="value">The current value for the field.</param>
        /// <param name="allowSceneObjects">If set to <c>true</c> then allow scene objects to be assigned to the field.</param>
        /// <returns>
        /// The object assigned to the field.
        /// </returns>
        [Obsolete("Use SirenixEditorFields.PolymorphicObjectField instead")]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public static object ObjectField(object key, Type type, GUIContent label, object value, bool allowSceneObjects = true)
        {
            var dropZone = DragAndDropManager.BeginDropZone(key, type, false);

            var rect = EditorGUILayout.GetControlRect(label != null);

            if (label != null && label.text != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }
            else
            {
                rect = EditorGUI.IndentedRect(rect);
            }

            GUIContent title;

            if (EditorGUI.showMixedValue)
            {
                title = new GUIContent("   " + "— Conflict (" + type.GetNiceName() + ")");
            }
            else if (value == null)
            {
                title = new GUIContent("   " + "Null (" + type.GetNiceName() + ")");
            }
            else
            {
                string baseType = value.GetType() == type ? "" : " : " + type.GetNiceName();
                title = new GUIContent("   " + value.GetType().GetNiceName() + baseType);
            }

            GUI.Label(rect, title, EditorStyles.objectField);
            EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height));

            var eventType = Event.current.type;

            if (dropZone.IsReadyToClaim)
            {
                object droppedObject = dropZone.ClaimObject();
                GUI.changed = true;
                return droppedObject;
            }

            // Handle Unity dragging manually for now
            else if ((eventType == EventType.DragUpdated || eventType == EventType.DragPerform) && dropZone.Rect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length == 1)
            {
                UnityEngine.Object obj = DragAndDrop.objectReferences[0];

                bool accept = false;

                if (type.IsAssignableFrom(obj.GetType()))
                {
                    accept = true;
                }
                else if (obj is GameObject && (type.InheritsFrom(typeof(Component)) || type.IsInterface))
                {
                    obj = (obj as GameObject).GetComponent(type);

                    if (obj != null)
                    {
                        accept = true;
                    }
                }

                if (accept)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    Event.current.Use();

                    if (eventType == EventType.dragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        GUI.changed = true;
                        return obj;
                    }
                }
            }

            DragAndDropManager.EndDropZone();

            var objectPicker = ObjectPicker.GetObjectPicker(key, type);

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                Event.current.Use();
                objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
            }

            if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
            {
                GUI.changed = true;
                return objectPicker.ClaimObject();
            }

            return value;
        }

        /// <summary>
        /// Draws a nicely formatted title with an optinal sub-title and horizontal ruler.
        /// </summary>
        public static void Title(string title, string subtitle, TextAlignment textAlignment, bool horizontalLine, bool boldLabel = true)
        {
            GUIStyle titleStyle = null;
            GUIStyle subtitleStyle = null;

            switch (textAlignment)
            {
                case TextAlignment.Left:
                titleStyle = boldLabel ? SirenixGUIStyles.BoldTitle : SirenixGUIStyles.Title;
                subtitleStyle = SirenixGUIStyles.Subtitle;
                break;

                case TextAlignment.Center:
                titleStyle = boldLabel ? SirenixGUIStyles.BoldTitleCentered : SirenixGUIStyles.TitleCentered;
                subtitleStyle = SirenixGUIStyles.SubtitleCentered;
                break;

                case TextAlignment.Right:
                titleStyle = boldLabel ? SirenixGUIStyles.BoldTitleRight : SirenixGUIStyles.TitleRight;
                subtitleStyle = SirenixGUIStyles.SubtitleRight;
                break;

                default:
                // Hidden feature by calling: Title("title", "subTitle", (TextAlignment)3, true, true);
                // This hidden feature is added because the TitleAlignment enum located in the Sirenix.OdinInspector.Attribute assembly have an extra split option.
                // But we don't have access to the assembly from here.
                titleStyle = boldLabel ? SirenixGUIStyles.BoldTitle : SirenixGUIStyles.Title;
                subtitleStyle = SirenixGUIStyles.SubtitleRight;
                break;
            }

            if ((int)textAlignment > (int)TextAlignment.Right)
            {
                var rect = GUILayoutUtility.GetRect(0, 18, titleStyle, GUILayoutOptions.ExpandWidth());

                GUI.Label(rect, title, titleStyle);
                rect.y += 3;
                GUI.Label(rect, subtitle, subtitleStyle);

                if (horizontalLine)
                {
                    HorizontalLineSeparator(SirenixGUIStyles.LightBorderColor, 1);
                    GUILayout.Space(3f);
                }
            }
            else
            {
                Rect rect = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect(false));
                GUI.Label(rect, title, titleStyle);

                if (subtitle != null && !subtitle.IsNullOrWhitespace())
                {
                    rect = EditorGUI.IndentedRect(GUILayoutUtility.GetRect(GUIHelper.TempContent(subtitle), subtitleStyle));
                    GUI.Label(rect, subtitle, subtitleStyle);
                }

                if (horizontalLine)
                {
                    DrawSolidRect(rect.AlignBottom(1), SirenixGUIStyles.LightBorderColor);
                    GUILayout.Space(3f);
                }
            }
        }

        /// <summary>
        /// Draws a GUI color field.
        /// </summary>
        /// <param name="rect">The rect to draw the field in.</param>
        /// <param name="color">The color of the field.</param>
        /// <param name="useAlphaInPreview">If set to <c>true</c> then use alpha in the preview.</param>
        /// <param name="showAlphaBar">If set to <c>true</c> then show alpha bar in the preview.</param>
        /// <returns>The color assigned to the field.</returns>
        public static Color DrawColorField(Rect rect, Color color, bool useAlphaInPreview = true, bool showAlphaBar = false)
        {
            const int margin = 3;

            EditorGUI.LabelField(rect, GUIContent.none, SirenixGUIStyles.ColorFieldBackground);

            rect.x += margin;
            rect.y += margin;
            rect.width -= margin * 2;
            rect.height -= margin * 2;

#pragma warning disable 0618 // Type or member is obsolete
            color = EditorGUI.ColorField(rect, GUIContent.none, color, false, true, false, null);
#pragma warning restore 0618 // Type or member is obsolete

            DrawSolidRect(rect, useAlphaInPreview ? color : new Color(color.r, color.g, color.b, 1f));

            if (showAlphaBar)
            {
                rect.y += rect.height - 7;
                rect.height = 7;
                DrawSolidRect(rect, Color.black);
                rect.width *= color.a;
                DrawSolidRect(rect, Color.white);
            }

            return color;
        }

        /// <summary>
        ///	Draws a warning message box.
        /// </summary>
        /// <remarks>
        /// Also triggers a warning during validation checks done by <see cref="OdinInspectorValidationChecker"/>
        /// </remarks>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void WarningMessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.Warning, wide);

            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy warning message"), false, () => { Clipboard.Copy(message); });
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        /// <summary>
        /// Draws a thick horizontal seperator.
        /// </summary>
        public static void DrawThickHorizontalSeperator(Rect rect)
        {
            EditorGUI.DrawRect(rect, darkerLinerColorThick);
            EditorGUI.DrawRect(rect.AlignTop(1), darkerLinerColorThick);
            EditorGUI.DrawRect(rect.AlignBottom(1), lighterLineColorThick);
        }

        /// <summary>
        /// Draws a thick horizontal seperator.
        /// </summary>
        public static void DrawThickHorizontalSeparator()
        {
            DrawThickHorizontalSeperator(5f, 0, 0);
        }

        /// <summary>
        /// Draws a thick horizontal seperator.
        /// </summary>
        public static void DrawThickHorizontalSeparator(float topPadding, float bottomPadding)
        {
            DrawThickHorizontalSeperator(5f, topPadding, bottomPadding);
        }

        /// <summary>
        /// Draws a thick horizontal seperator.
        /// </summary>
        public static void DrawThickHorizontalSeperator(float height, float topPadding, float bottomPadding)
        {
            var rect = GUILayoutUtility.GetRect(0, height + topPadding + bottomPadding);
            rect.y += topPadding;
            rect.height = height;
            DrawThickHorizontalSeperator(rect);
        }

        /// <summary>
        /// Draws a horizontal line seperator.
        /// </summary>
        public static void DrawHorizontalLineSeperator(float x, float y, float width, float alpha = 0.5f)
        {
            var dColor = darkerLinerColor;
            var lColor = lighterLineColor;
            dColor.a *= alpha;
            lColor.a *= alpha;
            Rect rect = new Rect(x, y - 1, width, 1);
            EditorGUI.DrawRect(rect, dColor);
            rect.y += 1;
            EditorGUI.DrawRect(rect, lColor);
        }

        /// <summary>
        /// Draws a vertical line seperator.
        /// </summary>
        public static void DrawVerticalLineSeperator(float x, float y, float height, float alpha = 0.5f)
        {
            var dColor = darkerLinerColor;
            var lColor = lighterLineColor;
            dColor.a *= alpha;
            lColor.a *= alpha;
            Rect rect = new Rect(x - 1, y, 1, height);
            EditorGUI.DrawRect(rect, dColor);
            rect.x += 1;
            EditorGUI.DrawRect(rect, lColor);
        }

        /// <summary>
        ///	Draws an error message box.
        /// </summary>
        /// <remarks>
        /// Also triggers an error during validation checks done by <see cref="OdinInspectorValidationChecker"/>
        /// </remarks>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void ErrorMessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.Error, wide);
        }

        /// <summary>
        /// Draws a info message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void InfoMessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.Info, wide);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void MessageBox(string message, bool wide = true)
        {
            MessageBox(message, MessageType.None, wide);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void MessageBox(string message, MessageType messageType, bool wide = true)
        {
            PrivateMessageBox(message, messageType, SirenixGUIStyles.MessageBox, wide);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="style">The style.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void MessageBox(string message, MessageType messageType, GUIStyle style, bool wide = true)
        {
            PrivateMessageBox(message, messageType, style, wide);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="style">The style.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        public static void MessageBox(string message, MessageType messageType, GUIStyle style, bool wide = true, Action<GenericMenu> onContextClick = null)
        {
            PrivateMessageBox(message, messageType, style, wide, onContextClick);
        }

        /// <summary>
        /// Draws a message box.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        /// <param name="messageBoxStyle">The style of the message box.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        private static Rect PrivateMessageBox(string message, MessageType messageType, GUIStyle messageBoxStyle, bool wide = true, Action<GenericMenu> onContextClick = null)
        {
            // TODO: @Tor Implement wide properly or make it obsolete as we don't really use it anyway.
            Texture icon = null;
            switch (messageType)
            {
                case MessageType.Info:
                icon = EditorIcons.UnityInfoIcon;
                break;

                case MessageType.Warning:
                icon = EditorIcons.UnityWarningIcon;
                break;

                case MessageType.Error:
                icon = EditorIcons.UnityErrorIcon;
                break;

                default:
                icon = null;
                break;
            }

            var rect = GUILayoutUtility.GetRect(GUIHelper.TempContent(message, icon), messageBoxStyle);
            rect = EditorGUI.IndentedRect(rect); // TODO: That's a bug, and will result in text overflowing vertically. Indent has to happen during or before calculating size based on text.

            GUI.Label(rect, GUIHelper.TempContent(message, icon), messageBoxStyle);

            if (Event.current.button == 1 && Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy message"), false, () => { Clipboard.Copy(message); });
                if (onContextClick != null)
                {
                    onContextClick(menu);
                }
                menu.ShowAsContext();
                Event.current.Use();
            }

            return rect;
        }

        /// <summary>
        /// Draws a message box that can be expanded to show more details.
        /// </summary>
        /// <param name="message">The message of the message box.</param>
        /// <param name="detailedMessage">The detailed message of the message box.</param>
        /// <param name="messageType">Type of the message box.</param>
        /// <param name="hideDetailedMessage">If set to <c>true</c> the detailed message is hidden.</param>
        /// <param name="wide">If set to <c>true</c> the message box will be wide.</param>
        /// <returns>State of isFolded.</returns>
        public static bool DetailedMessageBox(string message, string detailedMessage, MessageType messageType, bool hideDetailedMessage, bool wide = true)
        {
            var rect = PrivateMessageBox(hideDetailedMessage ? message : detailedMessage, messageType, SirenixGUIStyles.DetailedMessageBox, wide);

            var e = Event.current.rawType;

            if (e == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                hideDetailedMessage = !hideDetailedMessage;
                // We can only eat events while GUI.enabled is true.
                GUIHelper.PushGUIEnabled(true);
                Event.current.Use();
                GUIHelper.PopGUIEnabled();
            }

            if (e == EventType.Repaint)
            {
                rect.y += rect.height * 0.5f - 9;
                rect.x += rect.width - 9 - 10;
                rect.height = rect.width = 18;

                var arrow = hideDetailedMessage ? EditorIcons.TriangleDown : EditorIcons.TriangleUp;
                GUI.Label(rect, GUIHelper.TempContent(arrow.Active, hideDetailedMessage ? "Show details" : "Hide details"));
            }

            return hideDetailedMessage;
        }

        /// <summary>
        /// Draws a message box with the specified icon.
        /// </summary>
        /// <param name="message">The message to be displayed.</param>
        /// <param name="iconType">The icon to be displayed.</param>
        /// <param name="color">The color of the icon.</param>
        /// <param name="messageBoxStyle">The style of the message box.</param>
        /// <param name="onContextClick">The action to be invoked if the message box is right-clicked.</param>
        public static void IconMessageBox(string message, SdfIconType iconType, Color? color = null, GUIStyle messageBoxStyle = null, Action<GenericMenu> onContextClick = null)
        {
            color = color ?? EditorStyles.label.normal.textColor;
            messageBoxStyle = messageBoxStyle ?? SirenixGUIStyles.MessageBox;

            var messageStyle = new GUIStyle("label")
            {
                alignment = TextAnchor.MiddleLeft,
                wordWrap = messageBoxStyle.wordWrap,
                clipping = messageBoxStyle.clipping,
                fontSize = messageBoxStyle.fontSize,
                fontStyle = messageBoxStyle.fontStyle,
                normal = { textColor = messageBoxStyle.normal.textColor },
            };

            var rect = EditorGUILayout.BeginHorizontal(messageBoxStyle);
            var iconRect = GUILayoutUtility.GetRect(14, 18, GUILayout.ExpandWidth(false));
            var messageRect = GUILayoutUtility.GetRect(GUIHelper.TempContent(message), messageBoxStyle);
            EditorGUILayout.EndHorizontal();

            SdfIcons.DrawIcon(rect.AlignLeft(18).AddX(3).Padding(2), iconType, color.Value);
            GUI.Label(messageRect, message, messageStyle);

            if (Event.current.OnMouseDown(1) && rect.Contains(Event.current.mousePosition))
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Copy message"), false, () => { Clipboard.Copy(message); });
                onContextClick?.Invoke(menu);
                menu.ShowAsContext();
                Event.current.Use();
            }
        }

        /// <summary>
        /// Draws a horizontal line separator.
        /// </summary>
        /// <param name="lineWidth">Width of the line.</param>
        public static void HorizontalLineSeparator(int lineWidth = 1)
        {
            HorizontalLineSeparator(SirenixGUIStyles.BorderColor, lineWidth);
        }

        /// <summary>
        /// Draws a horizontal line separator.
        /// </summary>
        /// <param name="color">The color of the line.</param>
        /// <param name="lineWidth">The size of the line.</param>
        public static void HorizontalLineSeparator(Color color, int lineWidth = 1)
        {
            Rect rect = GUILayoutUtility.GetRect(lineWidth, lineWidth, GUILayoutOptions.ExpandWidth(true));
            DrawSolidRect(rect, color, true);
        }

        /// <summary>
        /// Draws a vertical line separator.
        /// </summary>
        /// <param name="lineWidth">Width of the line.</param>
        public static void VerticalLineSeparator(int lineWidth = 1)
        {
            VerticalLineSeparator(SirenixGUIStyles.BorderColor, lineWidth);
        }

        /// <summary>
        /// Draws a vertical line separator.
        /// </summary>
        /// <param name="color">The color of the line.</param>
        /// <param name="lineWidth">Width of the line.</param>
        public static void VerticalLineSeparator(Color color, int lineWidth = 1)
        {
            Rect rect = GUILayoutUtility.GetRect(lineWidth, lineWidth, GUILayoutOptions.ExpandHeight(true).Width(lineWidth));
            DrawSolidRect(rect, color, true);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(EditorIcon icon, int width = 18, int height = 18, string tooltip = "")
        {
            return IconButton(icon, null, width, height, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="style">The GUI style for the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(EditorIcon icon, GUIStyle style, int width = 18, int height = 18, string tooltip = "")
        {
            style = style ?? SirenixGUIStyles.IconButton;
            Rect rect = GUILayoutUtility.GetRect(icon.HighlightedGUIContent, style, GUILayoutOptions.ExpandWidth(false).Width(width).Height(height));
            return IconButton(rect, icon, style, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The editor icon for the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(Rect rect, EditorIcon icon)
        {
            return IconButton(rect, icon, null, "");
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(Rect rect, EditorIcon icon, string tooltip)
        {
            return IconButton(rect, icon, null, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The editor icon for the button.</param>
        /// <param name="style">The GUI style for the button.</param>
        /// <param name="tooltip">The tooltip of the button.</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool IconButton(Rect rect, EditorIcon icon, GUIStyle style, string tooltip)
        {
            style = style ?? SirenixGUIStyles.IconButton;
            if (GUI.Button(rect, GUIHelper.TempContent(null, null, tooltip), style))
            {
                GUIHelper.RemoveFocusControl();
                return true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                float size = Mathf.Min(rect.height, rect.width);
                icon.Draw(rect, size);
            }

            return false;
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The icon texture.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Rect rect, Texture icon, string tooltip)
        {
            return IconButton(rect, icon, null, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="rect">The rect to draw the button in.</param>
        /// <param name="icon">The icon texture.</param>
        /// <param name="style">Style for the button.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Rect rect, Texture icon, GUIStyle style, string tooltip)
        {
            style = style ?? SirenixGUIStyles.IconButton;

            if (GUI.Button(rect, GUIHelper.TempContent(icon, tooltip), style))
            {
                GUIHelper.RemoveFocusControl();
                return true;
            }

            if (Event.current.isMouse)
            {
                GUIHelper.RequestRepaint();
            }

            return false;
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The icon texture.</param>
        /// <param name="width">Width of the button in pixels.</param>
        /// <param name="height">Height of the button in pixels.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Texture icon, int width = 18, int height = 18, string tooltip = "")
        {
            return IconButton(icon, SirenixGUIStyles.IconButton, width, height, tooltip);
        }

        /// <summary>
        /// Draws a GUI button with an icon.
        /// </summary>
        /// <param name="icon">The icon texture.</param>
        /// <param name="style">Style for the button.</param>
        /// <param name="width">Width of the button in pixels.</param>
        /// <param name="height">Height of the button in pixels.</param>
        /// <param name="tooltip">The tooltip for the button.</param>
        /// <returns><c>true</c> when the button is pressed.</returns>
        public static bool IconButton(Texture icon, GUIStyle style, int width = 18, int height = 18, string tooltip = "")
        {
            style = style ?? SirenixGUIStyles.IconButton;
            Rect rect = GUILayoutUtility.GetRect(GUIHelper.TempContent(icon), style, GUILayoutOptions.ExpandWidth(false).Width(width).Height(height));
            return IconButton(rect, icon, style, tooltip);
        }

        /// <summary>
        /// Draws a repeating icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <returns><c>true</c> while the button is active. Otherwise <c>false</c>.</returns>
        public static bool IconRepeatButton(EditorIcon icon)
        {
            if (IconRepeatButton(icon, 18))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a repeating icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <param name="size">The size.</param>
        /// <returns><c>true</c> while the button is active. Otherwise <c>false</c>.</returns>
        public static bool IconRepeatButton(EditorIcon icon, int size)
        {
            if (IconRepeatButton(icon, size, size))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a repeating icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <param name="width">The width of the button.</param>
        /// <param name="height">The height of the button.</param>
        /// <returns><c>true</c> while the button is active. Otherwise <c>false</c>.</returns>
        public static bool IconRepeatButton(EditorIcon icon, int width, int height)
        {
            var rect = GUILayoutUtility.GetRect(width, height);

            if (GUI.RepeatButton(rect, icon.ActiveGUIContent, SirenixGUIStyles.None))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="label">The button's label.</param>
        /// <param name="height">The button's height.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(string label, float height, SdfIconType icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(height), GUILayout.ExpandHeight(true));
            return SDFIconButton(rect, new GUIContent(label), icon, iconAlignment, style);
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="label">The button's label.</param>
        /// <param name="height">The button's height.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(GUIContent label, float height, SdfIconType icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(height), GUILayout.ExpandHeight(true));
            return SDFIconButton(rect, label, icon, iconAlignment, style);
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="label">The button's label.</param>
        /// <param name="height">The button's height.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(string label, float height, Texture icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(height), GUILayout.ExpandHeight(true));
            return SDFIconButton(rect, new GUIContent(label), icon, iconAlignment, style);
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="label">The button's label.</param>
        /// <param name="height">The button's height.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(GUIContent label, float height, Texture icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            var rect = EditorGUILayout.GetControlRect(false, GUILayout.Height(height), GUILayout.ExpandHeight(true));
            return SDFIconButton(rect, label, icon, iconAlignment, style);
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="label">The button's label.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, string label, SdfIconType icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            return SDFIconButton(rect, new GUIContent(label), icon, iconAlignment, style);
        }


        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, SdfIconType icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            return SDFIconButton(rect, (GUIContent)null, icon, iconAlignment, style);
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, SdfIconType icon, GUIStyle style = null)
        {
            return SDFIconButton(rect, (GUIContent)null, icon, default, style);
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="label">The button's label.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, string label, Texture icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            return SDFIconButton(rect, new GUIContent(label), icon, iconAlignment, style);
        }

        private static void CalculateSDFIconButtonWidthAndPadding(bool hasIcon, float height, out float iconWidth, out float singlePadding)
        {
            singlePadding = (int)(height / 5) + 1;

            if (!hasIcon)
            {
                iconWidth = 0;
            }
            else
            {
                iconWidth = (int)(Mathf.Min(height, Mathf.Max(35f, height)) * 0.8f);
            }
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="label">The button's label.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, GUIContent label, SdfIconType icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null, bool selected = false)
        {
            return SDFIconButton(rect, label, (null, icon), iconAlignment, style, selected);
        }

        /// <summary>
        /// Draws a SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="label">The button's label.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, GUIContent label, Texture icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            return SDFIconButton(rect, label, (icon, SdfIconType.None), iconAlignment, style);
        }
        
        /// <summary>
        /// Draws a colored SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="label">The button's label.</param>
        /// <param name="buttonColor">The button's background color.</param>
        /// <param name="textColor">The button's text color.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, GUIContent label, Color buttonColor, Color textColor, SdfIconType icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            return SDFIconButton(rect, label, (null, icon), iconAlignment, style, false, buttonColor, textColor);
        }
        
        /// <summary>
        /// Draws a colored SDF icon button.
        /// </summary>
        /// <param name="rect">The button's rect.</param>
        /// <param name="label">The button's label.</param>
        /// <param name="buttonColor">The button's background color.</param>
        /// <param name="textColor">The button's text color.</param>
        /// <param name="icon">The button's icon.</param>
        /// <param name="iconAlignment">The button's icon alignment. ButtonIconAlignment.LeftOfText by default.</param>
        /// <param name="style">The button's style.</param>
        /// <returns></returns>
        public static bool SDFIconButton(Rect rect, GUIContent label, Color buttonColor, Color textColor, Texture icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null)
        {
            return SDFIconButton(rect, label, (icon, SdfIconType.None), iconAlignment, style, false, buttonColor, textColor);
        }

        private static bool SDFIconButton(Rect rect, GUIContent label, (Texture icon1, SdfIconType icon2) icon, IconAlignment iconAlignment = IconAlignment.LeftOfText, GUIStyle style = null, bool selected = false, Color? buttonColor = null, Color? textColor = null)
        {
            style = style ?? SirenixGUIStyles.Button;

            var hasIcon = icon.icon1 || icon.icon2 != SdfIconType.None;
            CalculateMinimumSDFIconButtonWidth(label?.text, SdfIconButtonLabelStyle, hasIcon, rect.height, out var labelWidth, out var iconWidth, out var singlePadding, out var minWidth);

            GUIContent tooltip;

            if (label != null)
            {
                if (tempSDFIconTooltipContent == null)
                {
                    tempSDFIconTooltipContent = new GUIContent("");
                }

                tooltip = tempSDFIconTooltipContent;

                if (rect.width < minWidth)
                {
                    if (!string.IsNullOrEmpty(label.tooltip))
                    {
                        tooltip.tooltip = label.text + "\n\n" + label.tooltip;
                    }
                    else
                    {
                        tooltip.tooltip = label.text;
                    }
                }
                else
                {

                    tooltip.tooltip = label.tooltip;
                }
            }
            else
            {
                tooltip = GUIContent.none;
            }

            var pressed = GUI.Button(rect, tooltip, style);

            if (Event.current.type == EventType.Repaint)
            {
                var hover = rect.Contains(Event.current.mousePosition);
                var btnColor = buttonColor.GetValueOrDefault();

                if (btnColor != Color.clear) { GUIHelper.PushColor(btnColor); }
                style.Draw(rect, tooltip, hover, false, selected, false);
                if (btnColor != Color.clear) { GUIHelper.PopColor(); }

                label = label ?? GUIContent.none;

                if (rect.Contains(Event.current.mousePosition))
                {
                    SdfIconButtonLabelStyle.normal.textColor = style.hover.textColor;
                }
                else
                {
                    SdfIconButtonLabelStyle.normal.textColor = style.normal.textColor;
                }
                
                var txtColor = textColor.GetValueOrDefault(SdfIconButtonLabelStyle.normal.textColor);
                var hasLabel = !string.IsNullOrEmpty(label.text) && !hasIcon || (!string.IsNullOrEmpty(label.text) && (minWidth <= rect.width + 1));

                var contentRect = rect.HorizontalPadding(singlePadding);

                if (hasIcon)
                {
                    Rect iconRect;

                    if (hasLabel)
                    {
                        if (iconAlignment == IconAlignment.LeftOfText)
                        {
                            contentRect = contentRect.AlignCenter(Math.Min(minWidth - singlePadding * 2, contentRect.width));
                            iconRect = contentRect.TakeFromLeft(iconWidth);
                            contentRect = contentRect.TakeFromRight(labelWidth);
                        }
                        else if (iconAlignment == IconAlignment.RightOfText)
                        {
                            contentRect = contentRect.AlignCenter(Math.Min(minWidth - singlePadding * 2, contentRect.width));
                            iconRect = contentRect.TakeFromRight(iconWidth);
                            contentRect = contentRect.TakeFromLeft(labelWidth);
                        }
                        else if (iconAlignment == IconAlignment.RightEdge)
                        {
                            iconRect = contentRect.TakeFromRight(iconWidth);
                            contentRect.TakeFromRight(singlePadding);
                        }
                        else if (iconAlignment == IconAlignment.LeftEdge)
                        {
                            iconRect = contentRect.TakeFromLeft(iconWidth);
                            contentRect.TakeFromLeft(singlePadding);
                        }
                        else
                        {
                            throw new NotImplementedException();
                        }


                        if (labelWidth > contentRect.width)
                            SdfIconButtonLabelStyle.alignment = TextAnchor.MiddleLeft;
                        else
                            SdfIconButtonLabelStyle.alignment = TextAnchor.MiddleCenter;

                        GUIHelper.PushContentColor(txtColor);
                        GUI.Label(contentRect, label, SdfIconButtonLabelStyle);
                        GUIHelper.PopContentColor();
                    }
                    else
                    {
                        iconRect = contentRect.AlignCenterX(iconWidth);
                    }

                    iconRect = iconRect.VerticalPadding(iconRect.height * 0.15f);

                    if (icon.icon1)
                        GUI.DrawTexture(iconRect, icon.icon1, ScaleMode.ScaleToFit, false, 1f, txtColor, 0f, 0f);
                    else if (icon.icon2 != SdfIconType.None)
                        SdfIcons.DrawIcon(iconRect, icon.icon2, txtColor);
                }
                else
                {
                    if (hasLabel)
                    {
                        if (labelWidth > contentRect.width)
                            SdfIconButtonLabelStyle.alignment = TextAnchor.MiddleLeft;
                        else
                            SdfIconButtonLabelStyle.alignment = TextAnchor.MiddleCenter;

                        GUIHelper.PushContentColor(txtColor);
                        GUI.Label(contentRect, label, SdfIconButtonLabelStyle);
                        GUIHelper.PopContentColor();
                    }
                    else
                    {

                    }
                }
                return pressed;
            }

            return pressed;
        }

        /// <summary>
        /// Calculates the minimum needed space for a SDF icon button where the label is still visible.
        /// </summary>
        /// <param name="label">The label of the SDF icon button.</param>
        /// <param name="buttonHeight">The height of the SDF icon button.</param>
        /// <returns>The minimum width of the SDF icon button.</returns>
        // TODO: Provide stub for the public one with Obsolete attribute. Or.. .
        public static void CalculateMinimumSDFIconButtonWidth(string btnText, GUIStyle labelStyle, bool hasIcon, float buttonHeight, out float labelWidth, out float iconWidth, out float singlePadding, out float totalWidth)
        {
            labelStyle = null;

            labelStyle = labelStyle ?? SdfIconButtonLabelStyle;
            var hasLabel = !string.IsNullOrEmpty(btnText);
            labelWidth = !string.IsNullOrEmpty(btnText) ? (int)labelStyle.CalcSize(GUIHelper.TempContent(btnText)).x : 0f;

            CalculateSDFIconButtonWidthAndPadding(hasIcon, buttonHeight, out iconWidth, out singlePadding);

            if (hasIcon)
            {
                if (hasLabel)
                {
                    totalWidth = labelWidth + iconWidth + singlePadding * 3;
                }
                else
                {
                    totalWidth = iconWidth + singlePadding * 2;
                }
            }
            else
            {
                if (hasLabel)
                {
                    totalWidth = labelWidth + singlePadding * 2;
                }
                else
                {
                    totalWidth = singlePadding * 2; // no icon, no label
                }
            }
        }

        /// <summary>
        /// Draws a toolbar icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <param name="ignoreGUIEnabled">If true, the button clickable while GUI.enabled == false.</param>
        /// <returns>
        ///   <c>true</c> if the button was pressed. Otherwise <c>false</c>.
        /// </returns>
        public static bool ToolbarButton(EditorIcon icon, bool ignoreGUIEnabled = false)
        {
            var rect = GUILayoutUtility.GetRect(currentDrawingToolbarHeight, 0, GUILayoutOptions.ExpandWidth(false).ExpandHeight());
            if (GUI.Button(rect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                icon.Draw(rect, 16);
            }

            if (ignoreGUIEnabled)
            {
                if (Event.current.button == 0 && Event.current.rawType == EventType.MouseDown)
                {
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        GUIHelper.RemoveFocusControl();
                        GUIHelper.RequestRepaint();
                        GUIHelper.PushGUIEnabled(true);
                        Event.current.Use();
                        GUIHelper.PopGUIEnabled();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Draws a toolbar icon button.
        /// </summary>
        /// <param name="icon">The icon for the button.</param>
        /// <param name="ignoreGUIEnabled">If true, the button clickable while GUI.enabled == false.</param>
        /// <returns>
        ///   <c>true</c> if the button was pressed. Otherwise <c>false</c>.
        /// </returns>
        public static bool ToolbarButton(SdfIconType icon, bool ignoreGUIEnabled = false)
        {
            var rect = GUILayoutUtility.GetRect(currentDrawingToolbarHeight, 0, GUILayoutOptions.ExpandWidth(false).ExpandHeight());
            if (GUI.Button(rect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }

            if (Event.current.type == EventType.Repaint)
            {
                var isMouseOver = rect.Contains(Event.current.mousePosition);
                var col = !isMouseOver ? SirenixGUIStyles.IconButton.normal.textColor
                    : SirenixGUIStyles.IconButton.hover.textColor;

                SdfIcons.DrawIcon(rect.AlignCenter(currentDrawingToolbarHeight - 8), icon, col);
            }

            if (ignoreGUIEnabled)
            {
                if (Event.current.button == 0 && Event.current.rawType == EventType.MouseDown)
                {
                    if (GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition))
                    {
                        GUIHelper.RemoveFocusControl();
                        GUIHelper.RequestRepaint();
                        GUIHelper.PushGUIEnabled(true);
                        Event.current.Use();
                        GUIHelper.PopGUIEnabled();
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Draws a toolbar icon button.
        /// </summary>
        /// <param name="content">The GUI content for the button.</param>
        /// <param name="selected">Whether the button state is selected or not</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool ToolbarButton(GUIContent content, bool selected = false)
        {
            if (GUILayout.Button(content, selected ? SirenixGUIStyles.ToolbarButtonSelected : SirenixGUIStyles.ToolbarButton, GUILayoutOptions.Height(currentDrawingToolbarHeight).ExpandWidth(false)))
            {
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draws a toolbar icon button.
        /// </summary>
        /// <param name="label">The label for the button.</param>
        /// <param name="selected">Whether the button state is selected or not</param>
        /// <returns><c>true</c> if the button was pressed. Otherwise <c>false</c>.</returns>
        public static bool ToolbarButton(string label, bool selected = false)
        {
            return ToolbarButton(GUIHelper.TempContent(label), selected);
        }

        /// <summary>
        /// Draws a toolbar toggle.
        /// </summary>
        /// <param name="isActive">Current state of the toggle.</param>
        /// <param name="icon">The icon for the toggle.</param>
        /// <returns>The state of the toggle.</returns>
        public static bool ToolbarToggle(bool isActive, EditorIcon icon)
        {
            return ToolbarToggle(isActive, icon.Active);
        }

        /// <summary>
        /// Draws a toolbar toggle.
        /// </summary>
        /// <param name="isActive">Current state of the toggle.</param>
        /// <param name="icon">The icon for the toggle.</param>
        /// <returns>The state of the toggle.</returns>
        public static bool ToolbarToggle(bool isActive, Texture icon)
        {
            if (GUILayout.Toggle(isActive, icon, SirenixGUIStyles.ToolbarButton, GUILayoutOptions.Width(currentDrawingToolbarHeight).Height(currentDrawingToolbarHeight)))
            {
                if (isActive == false)
                {
                    GUIHelper.RemoveFocusControl();
                    GUIHelper.RequestRepaint();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a toolbar toggle.
        /// </summary>
        /// <param name="isActive">Current state of the toggle.</param>
        /// <param name="content">The GUI content for the button.</param>
        /// <returns>The state of the toggle.</returns>
        public static bool ToolbarToggle(bool isActive, GUIContent content)
        {
            if (GUILayout.Toggle(isActive, content, SirenixGUIStyles.ToolbarButton, GUILayoutOptions.Height(currentDrawingToolbarHeight)))
            {
                if (isActive == false)
                {
                    GUIHelper.RemoveFocusControl();
                    GUIHelper.RequestRepaint();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a toolbar toggle.
        /// </summary>
        /// <param name="isActive">Current state of the toggle.</param>
        /// <param name="text">The text for the toggle.</param>
        /// <returns>The state of the toggle.</returns>
        public static bool ToolbarToggle(bool isActive, string text)
        {
            return ToolbarToggle(isActive, GUIHelper.TempContent(text));

            //EditorStyles.toolbarButton.fixedHeight = currentDrawingToolbarHeight;
            //if (GUILayout.Toggle(isActive, text, EditorStyles.toolbarButton, GUILayoutOptions.Height(currentDrawingToolbarHeight)))
            //{
            //    if (isActive == false)
            //    {
            //        GUIHelper.FocusControl();
            //        GUIHelper.RequestRepaint();
            //    }
            //    return true;
            //}
            //return false;
        }

        /// <summary>
        /// Draws a toolbar tab.
        /// </summary>
        /// <param name="isActive">If <c>true</c> the tab will be the active tab.</param>
        /// <param name="label">Name for the tab.</param>
        /// <returns>State of isActive.</returns>
        public static bool ToolbarTab(bool isActive, string label)
        {
            return ToolbarTab(isActive, GUIHelper.TempContent(label));
        }

        /// <summary>
        /// Draws a toolbar tab.
        /// </summary>
        /// <param name="isActive">If <c>true</c> the tab will be the active tab.</param>
        /// <param name="label">Label for the tab.</param>
        /// <returns>State of isActive.</returns>
        public static bool ToolbarTab(bool isActive, GUIContent label)
        {
            if (GUILayout.Toggle(isActive, label, SirenixGUIStyles.ToolbarTab, GUILayoutOptions.Height(currentDrawingToolbarHeight)))
            {
                if (isActive == false)
                {
                    GUIHelper.RemoveFocusControl();
                    GUIHelper.RequestRepaint();
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Draws a solid color rectangle.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="color">The color.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawSolidRect(Rect rect, Color color, bool usePlaymodeTint = true)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (usePlaymodeTint)
                {
                    EditorGUI.DrawRect(rect, color);
                }
                else
                {
                    GUIHelper.PushColor(color);
                    GUI.DrawTexture(rect, EditorGUIUtility.whiteTexture);
                    GUIHelper.PopColor();
                }
            }
        }

        /// <summary>
        /// Draws a solid color rectangle.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <param name="color">The color.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        /// <returns>The rect created.</returns>
        public static Rect DrawSolidRect(float width, float height, Color color, bool usePlaymodeTint = true)
        {
            var rect = GUILayoutUtility.GetRect(width, height, GUILayoutOptions.ExpandWidth(false));
            DrawSolidRect(rect, color, usePlaymodeTint);
            return rect;
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="borderWidth">The width of the border on all sides.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int borderWidth, bool usePlaymodeTint = true)
        {
            DrawBorders(rect, borderWidth, borderWidth, borderWidth, borderWidth, SirenixGUIStyles.BorderColor, usePlaymodeTint);
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="borderWidth">The width of the border on all sides.</param>
        /// <param name="color">The color of the border.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int borderWidth, Color color, bool usePlaymodeTint = true)
        {
            DrawBorders(rect, borderWidth, borderWidth, borderWidth, borderWidth, color, usePlaymodeTint);
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="left">The left size.</param>
        /// <param name="right">The right size.</param>
        /// <param name="top">The top size.</param>
        /// <param name="bottom">The bottom size.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int left, int right, int top, int bottom, bool usePlaymodeTint = true)
        {
            DrawBorders(rect, left, right, top, bottom, SirenixGUIStyles.BorderColor, usePlaymodeTint);
        }

        /// <summary>
        /// Draws borders around a rect.
        /// </summary>
        /// <param name="rect">The rect.</param>
        /// <param name="left">The left size.</param>
        /// <param name="right">The right size.</param>
        /// <param name="top">The top size.</param>
        /// <param name="bottom">The bottom size.</param>
        /// <param name="color">The color of the borders.</param>
        /// <param name="usePlaymodeTint">If <c>true</c> applies the user's playmdoe tint to the rect in playmode.</param>
        public static void DrawBorders(Rect rect, int left, int right, int top, int bottom, Color color, bool usePlaymodeTint = true)
        {
            if (Event.current.type == EventType.Repaint)
            {
                if (left > 0)
                {
                    var borderRect = rect;
                    borderRect.width = left;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }

                if (top > 0)
                {
                    var borderRect = rect;
                    borderRect.height = top;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }

                if (right > 0)
                {
                    var borderRect = rect;
                    borderRect.x += rect.width - right;
                    borderRect.width = right;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }

                if (bottom > 0)
                {
                    var borderRect = rect;
                    borderRect.y += rect.height - bottom;
                    borderRect.height = bottom;
                    DrawSolidRect(borderRect, color, usePlaymodeTint);
                }
            }
        }

        /// <summary>
        /// Draws a toolbar search field.
        /// </summary>
        /// <param name="searchText">The current search text.</param>
        /// <param name="forceFocus">If set to <c>true</c> the force focus on the field.</param>
        /// <param name="marginLeftRight">The left and right margin.</param>
        /// <returns>The current search text.</returns>
        public static string ToolbarSearchField(string searchText, bool forceFocus = false, float marginLeftRight = 5)
        {
            searchText = searchText ?? "";
            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, SirenixGUIStyles.ToolbarSearchTextField, GUILayoutOptions.ExpandWidth(true));
            rect.y += (currentDrawingToolbarHeight - 19) * 0.5f;
            rect.x += marginLeftRight;
            rect.width -= marginLeftRight * 2;
            rect.width -= 16;

            if (forceFocus)
            {
                GUI.SetNextControlName("ToolbarSearchField");
            }

            bool ignore = Event.current.keyCode == KeyCode.DownArrow || Event.current.keyCode == KeyCode.UpArrow;

            if (ignore)
            {
                GUIHelper.PushEventType(EventType.Ignore);
            }

            searchText = EditorGUI.TextField(rect, searchText, SirenixGUIStyles.ToolbarSearchTextField);

            if (ignore)
            {
                GUIHelper.PopEventType();
            }

            if (forceFocus)
            {
                GUI.FocusControl("ToolbarSearchField");
            }

            rect.x += rect.width;
            rect.width = 16;

            if (GUI.Button(rect, GUIContent.none, SirenixGUIStyles.ToolbarSearchCancelButton))
            {
                searchText = "";
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
            }

            return searchText;
        }

        private static bool ignoreNextLetterInSearchField = false;

        /// <summary>
        /// Draws a search field.
        /// </summary>
        public static string SearchField(Rect rect, string searchText, bool forceFocus = false, string controlName = "SirenixSearchField")
        {
            GUIHelper.PushIndentLevel(0);
            rect = rect.AlignTop(16);
            rect.width -= 16;

            searchText = searchText ?? "";

            var hasFocus = GUI.GetNameOfFocusedControl() == controlName;

            var ignore = hasFocus && Event.current.type == EventType.Used;

            // It seems the TextField just doesn't care to ignore
            if (ignore)
            {
                ignoreNextLetterInSearchField = true;
            }
            else if (ignoreNextLetterInSearchField && hasFocus)
            {
                Event.current.character = '\0';
                ignoreNextLetterInSearchField = false;
            }

            GUI.SetNextControlName(controlName);
            var tmpText = EditorGUI.TextField(rect, searchText, SirenixGUIStyles.ToolbarSearchTextField);

            if (!ignore)
            {
                searchText = tmpText;
            }

            if (Event.current.type == EventType.Repaint && string.IsNullOrEmpty(searchText))
            {
                float yOffset = 2;
                if (UnityVersion.IsVersionOrGreater(2019, 3))
                {
                    yOffset = 0;
                }

                GUI.Label(rect.AddXMin(14).SubY(yOffset), GUIHelper.TempContent("Search"), SirenixGUIStyles.LeftAlignedGreyMiniLabel);
            }

            if (forceFocus)
            {
                EditorGUI.FocusTextInControl(controlName);
            }

            rect.x += rect.width;
            rect.width = 16;

            if (GUI.Button(rect, GUIContent.none, SirenixGUIStyles.ToolbarSearchCancelButton))
            {
                searchText = "";
                GUIHelper.RemoveFocusControl();
                GUIHelper.RequestRepaint();
                GUI.changed = true;
            }

            GUIHelper.PopIndentLevel();
            return searchText;
        }

        /// <summary>
        /// Begins a horizontal toolbar. Remember to end with <see cref="EndHorizontalToolbar"/>.
        /// </summary>
        /// <param name="height">The height of the toolbar.</param>
        /// <param name="paddingTop">Padding for the top of the toolbar.</param>
        /// <returns>The rect of the horizontal toolbar.</returns>
        public static Rect BeginHorizontalToolbar(float height = 22, int paddingTop = 4)
        {
            return BeginHorizontalToolbar(SirenixGUIStyles.ToolbarBackground, height, paddingTop);
        }

        /// <summary>
        /// Begins a horizontal toolbar. Remember to end with <see cref="EndHorizontalToolbar" />.
        /// </summary>
        /// <param name="style">The style for the toolbar.</param>
        /// <param name="height">The height of the toolbar.</param>
        /// <param name="topPadding">The top padding.</param>
        /// <returns>
        /// The rect of the horizontal toolbar.
        /// </returns>
        public static Rect BeginHorizontalToolbar(GUIStyle style, float height = 22, int topPadding = 4)
        {
            currentDrawingToolbarHeight = height;
            var rect = EditorGUILayout.BeginHorizontal(style, GUILayoutOptions.Height(height).ExpandWidth(false));
            GUIHelper.PushHierarchyMode(false);
            GUIHelper.PushIndentLevel(0);

            return rect;
        }

        /// <summary>
        /// Ends a horizontal toolbar started by <see cref="BeginHorizontalToolbar(int, int)"/>.
        /// </summary>
        public static void EndHorizontalToolbar()
        {
            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUIHelper.GetCurrentLayoutRect();
                rect.yMin -= 1;
                DrawBorders(rect, 1);
            }

            GUIHelper.PopIndentLevel();
            GUIHelper.PopHierarchyMode();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Begins a horizontal indentation. Remember to end with <see cref="EndIndentedHorizontal"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        public static void BeginIndentedHorizontal(params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(GUIStyle.none, options);
            if (EditorGUI.indentLevel != 0)
            {
                var lblWith = GUIHelper.BetterLabelWidth - GUIHelper.CurrentIndentAmount;
                var overflow = 0f;
                if (lblWith < 1)
                {
                    lblWith = 1;
                    overflow = 1 - lblWith;
                }
                GUILayout.Space(overflow);
                GUIHelper.PushLabelWidth(lblWith);
                IndentSpace();
            }
            GUIHelper.PushIndentLevel(0);
        }

        /// <summary>
        /// Begins a horizontal indentation. Remember to end with <see cref="EndIndentedHorizontal"/>.
        /// </summary>
        /// <param name="style">The style of the indentation.</param>
        /// <param name="options">The GUI layout options.</param>
        public static void BeginIndentedHorizontal(GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal(style, options);
            if (EditorGUI.indentLevel != 0)
            {
                var lblWith = GUIHelper.BetterLabelWidth - GUIHelper.CurrentIndentAmount;
                var overflow = 0f;
                if (lblWith < 1)
                {
                    lblWith = 1;
                    overflow = 1 - lblWith;
                }
                GUILayout.Space(overflow);
                GUIHelper.PushLabelWidth(lblWith);
                IndentSpace();
            }
            GUIHelper.PushIndentLevel(0);
        }

        /// <summary>
        /// Ends a identation horizontal layout group started by <see cref="BeginIndentedHorizontal(GUILayoutOption[])"/>.
        /// </summary>
        public static void EndIndentedHorizontal()
        {
            GUIHelper.PopIndentLevel();
            GUILayout.EndHorizontal();
            if (EditorGUI.indentLevel != 0)
            {
                GUIHelper.PopLabelWidth();
            }
        }

        /// <summary>
        /// Begins a vertical indentation. Remember to end with <see cref="EndIndentedVertical"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        public static Rect BeginIndentedVertical(params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            if (EditorGUI.indentLevel != 0)
            {
                var lblWith = GUIHelper.BetterLabelWidth - GUIHelper.CurrentIndentAmount;
                var overflow = 0f;
                if (lblWith < 1)
                {
                    lblWith = 1;
                    overflow = 1 - lblWith;
                }
                GUILayout.Space(overflow);
                GUIHelper.PushLabelWidth(lblWith);
                IndentSpace();
            }
            GUIHelper.PushIndentLevel(0);
            return EditorGUILayout.BeginVertical(options);
        }

        /// <summary>
        /// Begins a vertical indentation. Remember to end with <see cref="EndIndentedVertical"/>.
        /// </summary>
        /// <param name="style">The style of the indentation.</param>
        /// <param name="options">The GUI layout options.</param>
        public static Rect BeginIndentedVertical(GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayout.BeginHorizontal();
            if (EditorGUI.indentLevel != 0)
            {
                var lblWith = GUIHelper.BetterLabelWidth - GUIHelper.CurrentIndentAmount;
                var overflow = 0f;
                if (lblWith < 1)
                {
                    lblWith = 1;
                    overflow = 1 - lblWith;
                }
                GUILayout.Space(overflow);
                GUIHelper.PushLabelWidth(lblWith);
                IndentSpace();
            }
            GUIHelper.PushIndentLevel(0);

            if (style == null)
                return EditorGUILayout.BeginVertical(options);
            else
                return EditorGUILayout.BeginVertical(style, options);
        }

        /// <summary>
        /// Ends a identation vertical layout group started by <see cref="BeginIndentedVertical(GUILayoutOption[])"/>.
        /// </summary>
        public static void EndIndentedVertical()
        {
            EditorGUILayout.EndVertical();
            GUIHelper.PopIndentLevel();
            GUILayout.EndHorizontal();

            if (EditorGUI.indentLevel != 0)
            {
                GUIHelper.PopLabelWidth();
            }
        }

        /// <summary>
        /// Indents by the current indent value, <see cref="GUIHelper.CurrentIndentAmount"/>.
        /// </summary>
        public static void IndentSpace()
        {
            GUILayout.Space(GUIHelper.CurrentIndentAmount);
        }

        /// <summary>
        /// Draws a menu button.
        /// </summary>
        /// <param name="indent">The indent of the button.</param>
        /// <param name="text">The text of the button.</param>
        /// <param name="isActive">The current state of the button.</param>
        /// <param name="icon">The texture icon for the button.</param>
        /// <returns>The current state of the button.</returns>
        public static bool MenuButton(int indent, string text, bool isActive, Texture icon)
        {
            bool isDown = false;

            var rect = EditorGUILayout.BeginHorizontal(SirenixGUIStyles.MenuButtonBackground);
            bool isMouseOver = rect.Contains(Event.current.mousePosition);
            if (isActive)
            {
                DrawSolidRect(rect, isMouseOver ? SirenixGUIStyles.MenuButtonActiveBgColor : SirenixGUIStyles.MenuButtonActiveBgColor);
            }
            else
            {
                DrawSolidRect(rect, isMouseOver ? SirenixGUIStyles.MenuButtonHoverColor : SirenixGUIStyles.MenuButtonColor);
            }

            DrawBorders(rect, 0, 0, 0, 1, SirenixGUIStyles.MenuButtonBorderColor);

            if (Event.current.type == EventType.MouseDown)
            {
                if (isMouseOver)
                {
                    Event.current.Use();
                    isDown = true;
                }
                GUIHelper.RequestRepaint();
            }

            var style = new GUIStyle(EditorStyles.label);
            style.fixedHeight = 20;
            if (isActive)
            {
                style.normal.textColor = Color.white;
            }

            GUILayout.Space(indent * 10);

            //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
            GUILayout.Label(new GUIContent(text, icon), style);
            //GL.sRGBWrite = false;
            EditorGUILayout.EndHorizontal();
            return isDown;
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        public static bool BeginFadeGroup(object key, bool isVisible)
        {
            return BeginFadeGroup(key, isVisible, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup" />.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="t">A value between 0 and 1 indicating how expanded the fade group is.</param>
        public static bool BeginFadeGroup(object key, bool isVisible, out float t)
        {
            return BeginFadeGroup(key, isVisible, out t, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="primaryKey">The primary key for the fade group.</param>
        /// <param name="secondaryKey">The secondly key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        public static bool BeginFadeGroup(object primaryKey, object secondaryKey, bool isVisible)
        {
            return BeginFadeGroup(primaryKey, secondaryKey, isVisible, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="name">The name of the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        public static bool BeginFadeGroup(object key, string name, bool isVisible)
        {
            return BeginFadeGroup(key, name, isVisible, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object key, bool isVisible, float duration)
        {
            var t = GUIHelper.GetTemporaryContext(fadeGroupKey, key, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                var target = isVisible ? 1 : 0;
                if (target != t.Value)
                    t.Value = Mathf.MoveTowards(t.Value, isVisible ? 1 : 0, GUITimeHelper.LayoutDeltaTime * (1f / duration));
            }
            return BeginFadeGroup(t.Value);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup" />.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="t">A value between 0 and 1 indicating how expanded the fade group is.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object key, bool isVisible, out float t, float duration)
        {
            var tt = GUIHelper.GetTemporaryContext(fadeGroupKey, key, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                var target = isVisible ? 1 : 0;
                if (target != tt.Value)
                    tt.Value = Mathf.MoveTowards(tt.Value, isVisible ? 1 : 0, GUITimeHelper.LayoutDeltaTime * (1f / duration));
            }
            t = tt.Value;
            return BeginFadeGroup(tt.Value);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="primaryKey">The primary key for the fade group.</param>
        /// <param name="secondaryKey">The secondly key for the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object primaryKey, object secondaryKey, bool isVisible, float duration)
        {
            var t = GUIHelper.GetTemporaryContext(primaryKey, secondaryKey, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                var target = isVisible ? 1 : 0;
                if (target != t.Value)
                    t.Value = Mathf.MoveTowards(t.Value, isVisible ? 1 : 0, GUITimeHelper.LayoutDeltaTime * (1f / duration));
            }
            return BeginFadeGroup(t.Value);
        }

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="key">The key for the fade group.</param>
        /// <param name="name">The name of the fade group.</param>
        /// <param name="isVisible">Current state of the fade group.</param>
        /// <param name="duration">The duration of fade in and out.</param>
        public static bool BeginFadeGroup(object key, string name, bool isVisible, float duration)
        {
            var t = GUIHelper.GetTemporaryContext(key, name, isVisible ? 1f : 0f);
            if (Event.current.type == EventType.Layout)
            {
                var target = isVisible ? 1 : 0;
                if (target != t.Value)
                    t.Value = Mathf.MoveTowards(t.Value, isVisible ? 1 : 0, GUITimeHelper.LayoutDeltaTime * (1f / duration));
            }
            return BeginFadeGroup(t.Value);
        }

        private static float fadeGrouPush = 0;

        /// <summary>
        /// Begins a fade group. Remember to end with <see cref="EndFadeGroup"/>.
        /// </summary>
        /// <param name="t">The current fading value between 0 and 1.</param>
        public static bool BeginFadeGroup(float t)
        {
            if (t > 0 && t < 1 && animatingFadeGroupIndex == -1)
            {
                var currContextWidth = GUIHelper.ContextWidth;
                fadeGroupFix = fadeGroupFix ?? new GUIStyle() { padding = new RectOffset(4, 4, 4, 4) };
                t = Mathf.Clamp01(t * t * (3f - 2f * t));
                EditorGUILayout.BeginFadeGroup(t);
                fadeGrouPush = (1 - t) * 10f;
                //GUILayout.Space(-fadeGrouPush - 4);
                //GUILayout.BeginHorizontal();
                //GUILayout.Space(-4);
                GUILayout.BeginVertical();
                //GUILayout.Space(0);
                GUIHelper.PushColor(GUI.color * new Color(1, 1, 1, t));
                animatingFadeGroupIndex = currentFadeGroupIndex;
                GUIHelper.RequestRepaint();
                GUIHelper.PushContextWidth(currContextWidth);
            }
            currentFadeGroupIndex++;
            return t > 0;
        }

        /// <summary>
        /// Ends a fade group started by any BeginFadeGroup.
        /// </summary>
        public static void EndFadeGroup()
        {
            currentFadeGroupIndex--;
            if (animatingFadeGroupIndex == currentFadeGroupIndex)
            {
                GUILayout.EndVertical();
                //GUILayout.Space(-4);
                //GUILayout.EndHorizontal();
                //GUILayout.Space(fadeGrouPush - 4);
                EditorGUILayout.EndFadeGroup();
                GUIHelper.PopColor();
                animatingFadeGroupIndex = -1;
                GUIHelper.PopContextWidth();
            }
        }

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="style">The GUI style.</param>
        /// <returns>
        /// The current state of the foldout.
        /// </returns>
        public static bool Foldout(bool isVisible, string label, GUIStyle style = null)
        {
            return Foldout(isVisible, new GUIContent(label), style);
        }

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="style">The GUI style.</param>
        public static bool Foldout(bool isVisible, GUIContent label, GUIStyle style = null)
        {
            var tmp = EditorGUIUtility.fieldWidth;
            EditorGUIUtility.fieldWidth = 10;
            var rect = EditorGUILayout.GetControlRect(false);
            EditorGUIUtility.fieldWidth = tmp;
            //Rect rect = GUILayoutUtility.GetRect(label, SirenixGUIStyles.Foldout);
            //rect.height = EditorGUIUtility.singleLineHeight;

            return Foldout(rect, isVisible, label, style);
        }

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="valueRect">The value rect.</param>
        /// <param name="style">The GUI style.</param>
        public static bool Foldout(bool isVisible, GUIContent label, out Rect valueRect, GUIStyle style = null)
        {
            Rect labelRect = EditorGUILayout.GetControlRect(false);
            valueRect = labelRect;

            if (label == null)
            {
                label = new GUIContent(" ");

                if (EditorGUIUtility.hierarchyMode)
                {
                    labelRect.width = 2;
                }
                else
                {
                    labelRect.width = 18;
                    valueRect.xMin += 18;
                }
            }
            else
            {
                var indent = GUIHelper.CurrentIndentAmount;
                labelRect = new Rect(labelRect.x, labelRect.y, GUIHelper.BetterLabelWidth - indent, labelRect.height);
                valueRect.xMin = labelRect.xMax;
            }

            return Foldout(labelRect, isVisible, label);
        }

        //public static bool Foldout(bool isVisible, GUIContent label, GUIStyle style = null)
        //{
        //    var tmp = EditorGUIUtility.fieldWidth;
        //    EditorGUIUtility.fieldWidth = 10;
        //    var rect = EditorGUILayout.GetControlRect(false);
        //    EditorGUIUtility.fieldWidth = tmp;
        //    //Rect rect = GUILayoutUtility.GetRect(label, SirenixGUIStyles.Foldout);
        //    //rect.height = EditorGUIUtility.singleLineHeight;

        //    return Foldout(rect, isVisible, label, style);
        //}

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="rect">The rect to draw the foldout field in.</param>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="style">The style.</param>
        public static bool Foldout(Rect rect, bool isVisible, string label, GUIStyle style = null)
        {
            return Foldout(rect, isVisible, GUIHelper.TempContent(label), style);
        }

        /// <summary>
        /// Draws a foldout field where clicking on the label toggles to the foldout too.
        /// </summary>
        /// <param name="rect">The rect to draw the foldout field in.</param>
        /// <param name="isVisible">The current state of the foldout.</param>
        /// <param name="label">The label of the foldout.</param>
        /// <param name="style">The style.</param>
        public static bool Foldout(Rect rect, bool isVisible, GUIContent label, GUIStyle style = null)
        {
            style = style ?? SirenixGUIStyles.Foldout;

            var e = Event.current.type;
            bool isHovering = false;
            if (e != EventType.Layout)
            {
                // Swallow foldout icon as well
                //rect.x -= 9;
                //rect.width += 9;
                isHovering = rect.Contains(Event.current.mousePosition);
                //rect.width -= 9;
                //rect.x += 9;
            }

            if (isHovering)
            {
                GUIHelper.PushLabelColor(SirenixGUIStyles.HighlightedTextColor);
            }

            if (isHovering && e == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint();
            }

            if (e == EventType.MouseDown && isHovering && Event.current.button == 0)
            {
                // Foldout works when GUI.enabled = false
                // Enable GUI, in order to Use() the the event properly.
                isVisible = !isVisible;
                GUIHelper.RequestRepaint();
                GUIHelper.PushGUIEnabled(true);
                Event.current.Use();
                GUIHelper.PopGUIEnabled();
                GUIHelper.RemoveFocusControl();
            }

            isVisible = EditorGUI.Foldout(rect, isVisible, label, style);

            if (isHovering)
            {
                GUIHelper.PopLabelColor();
            }

            return isVisible;
        }

        public static int FoldoutWidth => EditorStyles.foldout.padding.left - EditorStyles.label.padding.left;

        public static bool Foldout(ref Rect position, int id, GUIContent label, bool isExpanded, bool toggleOnLabelClick)
        {
            bool hasLabel = label != null && !string.IsNullOrEmpty(label.text);

            position = EditorGUI.IndentedRect(position);

            Rect foldoutPosition;

            int foldoutWidth = FoldoutWidth;
            
            if (EditorGUIUtility.hierarchyMode)
            {
                foldoutPosition = position;
                foldoutPosition.xMin -= foldoutWidth;
                foldoutPosition.width = foldoutWidth;
            }
            else
            {
                foldoutPosition = position.TakeFromLeft(foldoutWidth);
            }

            if (hasLabel)
            {
                if (toggleOnLabelClick)
                {
                    foldoutPosition.width = position.width + foldoutWidth;
                }

                EditorGUI.HandlePrefixLabel(position, position, label, id);
            }
            
            bool isHover = Event.current.IsMouseOver(foldoutPosition);

            bool hasKeyboardFocus = id == GUIUtility.keyboardControl;
            
            switch (Event.current.type)
            {
                case EventType.Repaint:
                    EditorStyles.foldout.Draw(foldoutPosition, isHover, false, isExpanded, hasKeyboardFocus);
                    break;

                case EventType.KeyDown:
                    if (!hasKeyboardFocus)
                    {
                        break;
                    }

                    switch (Event.current.keyCode)
                    {
                        case KeyCode.LeftArrow:
                            if (!isExpanded)
                            {
                                break;
                            }

                            isExpanded = false;
                            GUI.changed = true;

                            Event.current.Use();
                            break;

                        case KeyCode.RightArrow:
                            if (isExpanded)
                            {
                                break;
                            }

                            isExpanded = true;
                            GUI.changed = true;

                            Event.current.Use();
                            break;
                    }

                    break;

                case EventType.MouseDown:
                    if (!isHover || Event.current.button != 0)
                    {
                        break;
                    }

                    GUIUtility.keyboardControl = id;
                    isExpanded = !isExpanded;
                    GUI.changed = true;

                    break;
            }

            return isExpanded;
        }

        public static Rect PrefixRect(Rect position, out Rect prefixPosition)
        {
            prefixPosition = position;
            prefixPosition.width = EditorGUIUtility.labelWidth;
            prefixPosition.height = EditorGUIUtility.singleLineHeight;

            prefixPosition = EditorGUI.IndentedRect(prefixPosition);

            return position.AlignRight(position.width - EditorGUIUtility.labelWidth - 2.0f);
        }

        public static Rect PrefixRect(Rect position, GUIContent label, out Rect prefixPosition)
        {
            if (label != null && (!string.IsNullOrEmpty(label.text) || label.image != null))
            {
                return PrefixRect(position, out prefixPosition);
            }

            prefixPosition = Rect.zero;
            return EditorGUI.IndentedRect(position);
        }

        /// <summary>
        /// Begins drawing a box. Remember to end with <see cref="EndToolbarBox"/>.
        /// </summary>
        /// <param name="label">The label of the box.</param>
        /// <param name="centerLabel">If set to <c>true</c> then center label.</param>
        /// <param name="options">The GUI layout options.</param>
        public static Rect BeginBox(string label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            if (string.IsNullOrEmpty(label))
            {
                return BeginBox(options);
            }
            else
            {
                return BeginBox(GUIHelper.TempContent(label), centerLabel, options);
            }
        }

        /// <summary>
        /// Begins drawing a box. Remember to end with <see cref="EndToolbarBox"/>.
        /// </summary>
        /// <param name="label">The label of the box.</param>
        /// <param name="centerLabel">If set to <c>true</c> then center label.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginBox(GUIContent label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            var rect = BeginBox(options);

            if (label != null)
            {
                BeginBoxHeader();
                var tmp = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 10;
                var lblRect = EditorGUILayout.GetControlRect(false);
                EditorGUIUtility.fieldWidth = tmp;
                GUI.Label(lblRect, label, centerLabel ? SirenixGUIStyles.LabelCentered : SirenixGUIStyles.Label);
                EndBoxHeader();
            }

            return rect;
        }

        /// <summary>
        /// Begins drawing a box. Remember to end with <see cref="EndToolbarBox"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        public static Rect BeginBox(params GUILayoutOption[] options)
        {
            BeginIndentedVertical(SirenixGUIStyles.BoxContainer, options);
            var labelWidth = GUIHelper.BetterLabelWidth - 4;
            GUIHelper.PushHierarchyMode(false);
            GUIHelper.PushLabelWidth(labelWidth);
            return GUIHelper.GetCurrentLayoutRect();
        }

        /// <summary>
        /// Ends drawing a box started by any BeginBox.
        /// </summary>
        public static void EndBox()
        {
            GUIHelper.PopLabelWidth();
            GUIHelper.PopHierarchyMode();
            EndIndentedVertical();
            //GUILayout.Space(1);
        }

        /// <summary>
        /// Begins drawing a box header. Remember to end with <seealso cref="EndToolbarBoxHeader"/>.
        /// </summary>
        public static Rect BeginBoxHeader()
        {
            GUILayout.Space(-3);
            var headerBgRect = EditorGUILayout.BeginHorizontal(SirenixGUIStyles.BoxHeaderStyle, GUILayoutOptions.ExpandWidth(true));

            if (Event.current.type == EventType.Repaint)
            {
                headerBgRect.x -= 3;
                headerBgRect.width += 6;
                //headerBgRect.y -= 2;
                //headerBgRect.height += 4;
                GUIHelper.PushColor(SirenixGUIStyles.HeaderBoxBackgroundColor);
                GUI.DrawTexture(headerBgRect, Texture2D.whiteTexture);
                GUIHelper.PopColor();
                DrawBorders(headerBgRect, 0, 0, 0, 1, new Color(0, 0, 0, 0.4f));
            }
            GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth - 4);
            return headerBgRect;
        }

        /// <summary>
        /// Ends drawing a box header started by <see cref="BeginToolbarBoxHeader"/>,
        /// </summary>
        public static void EndBoxHeader()
        {
            GUIHelper.PopLabelWidth();
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Begins drawing a box with toolbar style header. Remember to end with <seealso cref="EndToolbarBox"/>.
        /// </summary>
        /// <param name="label">Label for box header.</param>
        /// <param name="centerLabel">If <c>true</c> the label will be drawn in the center of the box header.</param>
        /// <param name="options">GUILayout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginToolbarBox(string label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            if (string.IsNullOrEmpty(label))
            {
                return BeginToolbarBox(options);
            }
            else
            {
                return BeginToolbarBox(GUIHelper.TempContent(label), centerLabel, options);
            }
        }

        /// <summary>
        /// Begins drawing a box with toolbar style header. Remember to end with <seealso cref="EndToolbarBox"/>.
        /// </summary>
        /// <param name="label">Label for box header.</param>
        /// <param name="centerLabel">If <c>true</c> the label will be drawn in the center of the box header.</param>
        /// <param name="options">GUILayout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginToolbarBox(GUIContent label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            var rect = BeginToolbarBox(options);

            if (label != null)
            {
                BeginToolbarBoxHeader();
                var tmp = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 10;
                var lblRect = EditorGUILayout.GetControlRect(false);
                EditorGUIUtility.fieldWidth = tmp;
                GUI.Label(lblRect, label, centerLabel ? SirenixGUIStyles.LabelCentered : SirenixGUIStyles.Label);
                EndToolbarBoxHeader();
            }

            return rect;
        }

        /// <summary>
        /// Begins drawing a box with toolbar style header. Remember to end with <seealso cref="EndToolbarBox"/>.
        /// </summary>
        /// <param name="options">GUILayout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginToolbarBox(params GUILayoutOption[] options)
        {
            BeginIndentedVertical(SirenixGUIStyles.BoxContainer, options);
            GUIHelper.PushHierarchyMode(false);
            GUIHelper.PushLabelWidth(GUIHelper.BetterLabelWidth - 4);
            return GUIHelper.GetCurrentLayoutRect();
        }

        /// <summary>
        /// Ends the drawing a box with a toolbar style header started by <see cref="BeginToolbarBox(GUILayoutOption[])"/>.
        /// </summary>
        public static void EndToolbarBox()
        {
            GUIHelper.PopLabelWidth();
            GUIHelper.PopHierarchyMode();
            EndIndentedVertical();
        }

        /// <summary>
        /// Begins drawing a toolbar style box header. Remember to end with <see cref="EndToolbarBoxHeader"/>.
        /// </summary>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginToolbarBoxHeader(float height = 22)
        {
            GUILayout.Space(-3);
            currentDrawingToolbarHeight = height;
            var headerBgRect = EditorGUILayout.BeginHorizontal(SirenixGUIStyles.BoxHeaderStyle, GUILayoutOptions.Height(height).ExpandWidth(true));
            GUILayout.Space(0);

            if (Event.current.type == EventType.Repaint)
            {
                var rect = headerBgRect;
                rect.x -= 3;
                rect.width += 6;
                //rect.y -= 1;
                rect.height += 1;
                SirenixGUIStyles.ToolbarBackground.Draw(rect, GUIContent.none, 0);
            }
            return headerBgRect;
        }

        /// <summary>
        /// Ends the drawing of a toolbar style box header started by <see cref="BeginToolbarBoxHeader"/>.
        /// </summary>
        public static void EndToolbarBoxHeader()
        {
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Begins drawing a legend style box. Remember to end with <see cref="EndLegendBox"/>.
        /// </summary>
        /// <param name="label">The label for the legend style box.</param>
        /// <param name="centerLabel">If <c>true</c> the label will be drawn in the center of the box.</param>
        /// <param name="options">GUILayout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginLegendBox(string label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            return BeginLegendBox(string.IsNullOrEmpty(label) ? null : GUIHelper.TempContent(label), centerLabel, options);
        }

        /// <summary>
        /// Begins drawing a legend style box. Remember to end with <see cref="EndLegendBox"/>.
        /// </summary>
        /// <param name="label">The label for the legend style box.</param>
        /// <param name="centerLabel">If <c>true</c> the label will be drawn in the center of the box.</param>
        /// <param name="options">GUILayout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginLegendBox(GUIContent label, bool centerLabel = false, params GUILayoutOption[] options)
        {
            if (label != null)
            {
                BeginIndentedVertical(legendBoxPadding, options);
            }
            else
            {
                BeginIndentedVertical(legendBoxNoLabelPadding, options);
            }

            var labelWidth = GUIHelper.BetterLabelWidth - 4;
            GUIHelper.PushHierarchyMode(false);
            GUIHelper.PushLabelWidth(labelWidth);
            Rect rect = GUIHelper.GetCurrentLayoutRect();

            if (Event.current.type == EventType.Repaint)
            {
                Rect box = rect;

                if (label != null)
                {
                    box.yMin += 5;
                }

                if (label != null)
                {
                    var size = SirenixGUIStyles.Label.CalcSize(label);
                    var header = centerLabel ?
                        new Rect(rect.center.x - size.x / 2 - 4, rect.y - 3, size.x + 8, 16) :
                        new Rect(rect.x + 4, rect.y - 3, size.x + 8, 16);

                    if (header.xMax > rect.xMax - 4)
                    {
                        header.xMax = rect.xMax - 4;
                    }
                    if (header.xMin < rect.xMin + 4)
                    {
                        header.xMin = rect.xMin + 4;
                    }

                    SirenixEditorGUI.DrawBorders(box, 1, 1, 0, 1);
                    SirenixEditorGUI.DrawSolidRect(new Rect(box.x, box.y, header.xMin - box.xMin, 1), SirenixGUIStyles.BorderColor);
                    SirenixEditorGUI.DrawSolidRect(new Rect(header.xMax, box.y, box.xMax - header.xMax, 1), SirenixGUIStyles.BorderColor);
                    GUI.Label(header.HorizontalPadding(4, 4), label);
                }
                else
                {
                    SirenixEditorGUI.DrawBorders(box, 1);
                }
            }

            return rect;
        }

        /// <summary>
        /// Begins drawing a legend style box. Remember to end with <see cref="EndLegendBox"/>.
        /// </summary>
        /// <param name="options">GUILayout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginLegendBox(params GUILayoutOption[] options)
        {
            return BeginLegendBox((GUIContent)null, false, options);
        }

        /// <summary>
        /// Ends the drawing of a legend style box started by <see cref="BeginLegendBox(GUILayoutOption[])"/>
        /// </summary>
        public static void EndLegendBox()
        {
            GUIHelper.PopLabelWidth();
            GUIHelper.PopHierarchyMode();
            EndIndentedVertical();
            GUILayout.Space(1);
        }

        /// <summary>
        /// Begins drawing an inline box. Remember to end with <see cref="EndInlineBox"/>.
        /// </summary>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect of the box.</returns>
        public static Rect BeginInlineBox(params GUILayoutOption[] options)
        {
            GUILayout.BeginVertical(SirenixGUIStyles.BoxContainer, options);
            GUIHelper.PushHierarchyMode(false);
            return GUIHelper.GetCurrentLayoutRect();
        }

        /// <summary>
        /// Ends drawing an inline box started by any BeginInlineBox.
        /// </summary>
        public static void EndInlineBox()
        {
            GUIHelper.PopHierarchyMode();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Starts the shaking animation of a shaking group.
        /// </summary>
        [Obsolete("Use keyless overload instead.")]
        public static void StartShakingGroup(object key)
        {
            StartShakingGroup(key, ShakingAnimationDuration);
        }

        /// <summary>
        /// Starts the shaking animation of a shaking group.
        /// </summary>
        [Obsolete("Use keyless overload instead. Note that the 'duration' parameter is now provided to the BeginShakeableGroup method instead.")]
        public static void StartShakingGroup(object key, float duration)
        {
            var c = GUIHelper.GetTemporaryContext<ShakeGroup>(shakeableGroupKey, key, ShakeGroup.Create(duration));
            c.Value.Time = 0;
            c.Value.Duration = duration;
            c.Value.IsShaking = true;
        }

        /// <summary>
        /// Begins a shakeable group.
        /// </summary>
        [Obsolete("Use keyless overload instead.")]
        public static void BeginShakeableGroup(object key)
        {
            if (Event.current.OnRepaint())
            {
                const float speed = 50;
                const float dist = 5;
                var c = GUIHelper.GetTemporaryContext<ShakeGroup>(shakeableGroupKey, key, ShakeGroup.Create());

                if (c.Value.Duration == 0)
                    return;

                if (c.Value.IsShaking)
                {
                    c.Value.IsShaking = c.Value.Time <= c.Value.Duration;

                    c.Value.Time += GUITimeHelper.RepaintDeltaTime;
                    var t = c.Value.Time / c.Value.Duration;

                    GUIHelper.PushMatrix(
                        GUI.matrix *
                        Matrix4x4.TRS(                                                           // ease in fast!
                            new Vector3(Mathf.Cos(c.Value.Time * speed) * dist * MathUtilities.EaseInOut(t), 0f),
                            Quaternion.identity,
                            Vector3.one));
                    GUIHelper.RequestRepaint();
                }
            }
        }

        /// <summary>
        /// Ends the shakeable group.
        /// </summary>
        [Obsolete("Use keyless overload instead.")]
        public static void EndShakeableGroup(object key)
        {
            if (Event.current.OnRepaint())
            {
                var c = GUIHelper.GetTemporaryContext<ShakeGroup>(shakeableGroupKey, key, ShakeGroup.Create());

                if (c.Value.Duration == 0f)
                {
                    return;
                }

                if (c.Value.IsShaking)
                {
                    GUIHelper.PopMatrix();
                }
            }
        }

        /// <summary>
        /// Begins a shakeable group.
        /// </summary>
        public static void BeginShakeableGroup() => BeginShakeableGroup(ShakingAnimationDuration);

        /// <summary>
        /// Begins a shakeable group.
        /// </summary>
        public static void BeginShakeableGroup(float duration)
        {
            int control = GUIUtility.GetControlID(FocusType.Passive);
            shakingGroupControlIdStack.Push(control);

            if (Event.current.type == EventType.Repaint && currentShakingGroupControlId == control)
            {
                const float speed = 50;
                const float dist = 5;

                var time = (float)(EditorApplication.timeSinceStartup - shakingGroupStartTime);

                if (duration <= 0f || time >= duration)
                {
                    currentShakingGroupControlId = -1;
                }
                else
                {
                    GUIHelper.PushMatrix(
                        GUI.matrix *
                        Matrix4x4.TRS(
                            new Vector3(Mathf.Cos(time * speed) * dist * MathUtilities.EaseInOut(time / duration), 0f, 0f),
                            Quaternion.identity,
                            Vector3.one));
                    GUIHelper.RequestRepaint();
                }
            }
        }

        /// <summary>
        /// Starts the shaking animation of a shaking group.
        /// </summary>
        public static void StartShakingGroup()
        {
            if (shakingGroupControlIdStack.Count > 0) // Or throw error?
            {
                var control = shakingGroupControlIdStack.Peek();
                currentShakingGroupControlId = control;
                shakingGroupStartTime = EditorApplication.timeSinceStartup;
            }
        }

        /// <summary>
        /// Ends the shakeable group.
        /// </summary>
        public static void EndShakeableGroup()
        {
            var control = shakingGroupControlIdStack.Pop();

            if (Event.current.type == EventType.Repaint && currentShakingGroupControlId == control)
            {
                GUIHelper.PopMatrix();
            }
        }

        /// <summary>
        /// Begins drawing a vertical menu list.
        /// </summary>
        /// <param name="key">The key for the menu list.</param>
        /// <returns>The rect created.</returns>
        public static Rect BeginVerticalMenuList(object key)
        {
            var context = GUIHelper.GetTemporaryContext<VerticalMenuListInfo>(menuListKey, key).Value;

            if (Event.current.type == EventType.Repaint)
            {
                GUIHelper.BeginLayoutMeasuring();
            }

            currentVerticalMenuListInfo = context;
            currentVerticalMenuListInfo.CurrentIndex = 0;
            context.ScrollPositiion = EditorGUILayout.BeginScrollView(context.ScrollPositiion, false, false);

            if (Event.current.type == EventType.KeyDown && context.MenuItemCount > 0)
            {
                if (Event.current.keyCode == KeyCode.DownArrow)
                {
                    context.NextSelectedIndex = context.SelectedItemIndex + 1;
                    context.ScrollToCurrent = true;
                    Event.current.Use();
                }
                else if (Event.current.keyCode == KeyCode.UpArrow)
                {
                    context.NextSelectedIndex = context.SelectedItemIndex - 1;
                    context.ScrollToCurrent = true;
                    Event.current.Use();
                }
            }

            if (context.NextSelectedIndex.HasValue && Event.current.type == EventType.Layout)
            {
                context.SelectedItemIndex = Mathf.Clamp(context.NextSelectedIndex.Value, 0, context.MenuItemCount - 1);
                context.NextSelectedIndex = null;
            }

            return BeginVerticalList(false);
        }

        /// <summary>
        /// Begins drawing a menu list item. Remember to end with <see cref="EndMenuListItem"/>
        /// </summary>
        /// <param name="isSelected">Value indicating whether the item is selected.</param>
        /// <param name="isMouseDown">Value indicating if the mouse is pressed on the item.</param>
        /// <param name="setAsSelected">If set to <c>true</c> the item is set as selected..</param>
        /// <returns>The rect used for the item.</returns>
        public static Rect BeginMenuListItem(out bool isSelected, out bool isMouseDown, bool setAsSelected = false)
        {
            isMouseDown = false;

            if (setAsSelected)
            {
                currentVerticalMenuListInfo.NextSelectedIndex = currentVerticalMenuListInfo.CurrentIndex;
            }

            isSelected = currentVerticalMenuListInfo.SelectedItemIndex == currentVerticalMenuListInfo.CurrentIndex;

            var rect = EditorGUILayout.BeginVertical(SirenixGUIStyles.ListItem);

            if (isSelected)
            {
                popMenuItemLabelColor = true;
                GUIHelper.PushLabelColor(Color.white);
                isSelected = true;

                if (currentVerticalMenuListInfo.ScrollToCurrent && Event.current.type == EventType.Repaint)
                {
                    float from = currentVerticalMenuListInfo.ScrollPositiion.y;
                    float to = from + currentVerticalMenuListInfo.Height;

                    if (rect.y < from)
                    {
                        currentVerticalMenuListInfo.ScrollPositiion.y = rect.y;
                    }
                    else if (rect.yMax > to)
                    {
                        currentVerticalMenuListInfo.ScrollPositiion.y = rect.yMax - currentVerticalMenuListInfo.Height;
                    }

                    currentVerticalMenuListInfo.ScrollToCurrent = false;
                }
            }

            if (Event.current.type == EventType.MouseMove && rect.Contains(Event.current.mousePosition))
            {
                currentVerticalMenuListInfo.NextSelectedIndex = currentVerticalMenuListInfo.CurrentIndex;
            }
            else if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
            {
                isMouseDown = true;
                Event.current.Use();
            }

            currentVerticalMenuListInfo.CurrentIndex++;

            DrawSolidRect(rect, isSelected ? SirenixGUIStyles.MenuButtonActiveBgColor : (currentVerticalMenuListInfo.CurrentIndex % 2 == 0 ? SirenixGUIStyles.ListItemColorEven : SirenixGUIStyles.ListItemColorOdd));

            return rect;
        }

        /// <summary>
        /// Ends drawing a menu list item started by <see cref="BeginMenuListItem(out bool, out bool, bool)"/>
        /// </summary>
        public static void EndMenuListItem()
        {
            if (popMenuItemLabelColor)
            {
                GUIHelper.PopLabelColor();
                popMenuItemLabelColor = false;
            }
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Ends drawing a vertical menu list started by <see cref="BeginVerticalMenuList(object)"/>
        /// </summary>
        public static void EndVerticalMenuList()
        {
            EndVerticalList();
            EditorGUILayout.EndScrollView();

            currentVerticalMenuListInfo.MenuItemCount = currentVerticalMenuListInfo.CurrentIndex;

            if (Event.current.type == EventType.Repaint)
            {
                var rect = GUIHelper.EndLayoutMeasuring();
                currentVerticalMenuListInfo.Height = rect.height;
            }
        }

        /// <summary>
        /// Begins drawing a vertical list.
        /// </summary>
        /// <param name="drawBorder">If set to <c>true</c> borders will be drawn around the vertical list.</param>
        /// <param name="drawDarkBg">If set to <c>true</c> a dark background will be drawn.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect used for the list.</returns>
        public static Rect BeginVerticalList(bool drawBorder = true, bool drawDarkBg = true, params GUILayoutOption[] options)
        {
            currentScope++;
            currentListItemIndecies.SetLength(Mathf.Max(currentListItemIndecies.Count, currentScope + 1));
            currentListItemIndecies[currentScope] = 0;

            if (Event.current.type == EventType.MouseMove)
            {
                GUIHelper.RequestRepaint();
            }

            var rect = EditorGUILayout.BeginVertical(options);

            if (drawDarkBg)
            {
                DrawSolidRect(rect, SirenixGUIStyles.ListItemDragBgColor);
            }

            if (drawBorder)
            {
                verticalListBorderRects.Push(rect);
            }
            else
            {
                verticalListBorderRects.Push(new Rect(-1, rect.y, rect.width, rect.height));
            }

            return rect;
        }

        /// <summary>
        /// Ends drawing a vertical list started by <see cref="BeginVerticalList(bool, bool, GUILayoutOption[])"/>.
        /// </summary>
        public static void EndVerticalList()
        {
            currentScope--;
            var rect = verticalListBorderRects.Pop();
            if (rect.x > 0)
            {
                rect.y -= 1;
                rect.height += 1;
                DrawBorders(rect, 1, 1, 1, 1);
            }
            rect = GUIHelper.GetCurrentLayoutRect();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Begins drawing a list item.
        /// </summary>
        /// <param name="allowHover">If set to <c>true</c> the item can be hovered with the mouse.</param>
        /// <param name="style">The style for the vertical list item.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <returns>The rect used for the item.</returns>
        public static Rect BeginListItem(bool allowHover = true, GUIStyle style = null, params GUILayoutOption[] options)
        {
            bool isMouseOver;
            return BeginListItem(allowHover, style, out isMouseOver, SirenixGUIStyles.ListItemColorEven, SirenixGUIStyles.ListItemColorOdd, SirenixGUIStyles.ListItemColorHoverEven, SirenixGUIStyles.ListItemColorHoverOdd, options);
        }

        /// <summary>
        /// Begins drawing a list item.
        /// </summary>
        /// <param name="allowHover">If set to <c>true</c> the item can be hovered with the mouse.</param>
        /// <param name="style">The style for the vertical list item.</param>
        /// <param name="options">The GUI layout options.</param>
        /// <param name="evenColor">The color for even elements.</param>
        /// <param name="oddColor">The color for odd elements.</param>
        /// <param name="evenHoverColor">The color for even elements when hovered.</param>
        /// <param name="oddHoverColor">The color for odd elements when hovered.</param>
        /// <returns>The rect used for the item.</returns>
        public static Rect BeginListItem(bool allowHover, GUIStyle style, Color evenColor, Color oddColor, Color evenHoverColor, Color oddHoverColor, params GUILayoutOption[] options)
        {
            bool isMouseOver;
            return BeginListItem(allowHover, style, out isMouseOver, evenColor, oddColor, evenHoverColor, oddHoverColor, options);
        }

        private static Rect BeginListItem(bool allowHover, GUIStyle style, out bool isMouseOver, Color evenColor, Color oddColor, Color evenHoverColor, Color oddHoverColor, params GUILayoutOption[] options)
        {
            currentListItemIndecies.SetLength(Mathf.Max(currentListItemIndecies.Count, currentScope));
            int i = currentListItemIndecies[currentScope];
            currentListItemIndecies[currentScope] = i + 1;

            GUILayout.BeginVertical(style ?? SirenixGUIStyles.ListItem, options);
            var rect = GUIHelper.GetCurrentLayoutRect();
            isMouseOver = rect.Contains(Event.current.mousePosition);

            if (Event.current.type == EventType.Repaint)
            {
                Color color = i % 2 == 0 ? evenColor : oddColor;
                Color hover = color;
                if (DragAndDropManager.IsDragInProgress == false && allowHover)
                {
                    hover = i % 2 == 0 ? evenHoverColor : oddHoverColor;
                }
                DrawSolidRect(rect, isMouseOver ? hover : color);
            }

            return rect;
        }

        /// <summary>
        /// Ends drawing a list item started by <see cref="BeginListItem(bool, GUIStyle, GUILayoutOption[])"/>.
        /// </summary>
        public static void EndListItem()
        {
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Creates a animated tab group.
        /// </summary>
        /// <param name="key">The key for the tab group..</param>
        /// <returns>An animated tab group.</returns>
        public static GUITabGroup CreateAnimatedTabGroup(object key)
        {
            var t = GUIHelper.GetTemporaryContext<GUITabGroup>(animatedTabGroupKey, key).Value;
            t.AnimationSpeed = 1f / SirenixEditorGUI.TabPageSlideAnimationDuration;
            return t;
        }

        /// <summary>
        /// Begins drawing a toggle group. Remember to end with <see cref="EndToggleGroup"/>.
        /// </summary>
        /// <param name="key">The key of the group.</param>
        /// <param name="enabled">Value indicating if the group is enabled.</param>
        /// <param name="visible">Value indicating if the group is visible.</param>
        /// <param name="title">The title of the group.</param>
        /// <returns>Value indicating if the group is toggled.</returns>
        public static bool BeginToggleGroup(object key, ref bool enabled, ref bool visible, string title)
        {
            return BeginToggleGroup(key, ref enabled, ref visible, title, DefaultFadeGroupDuration);
        }

        /// <summary>
        /// Begins drawing a toggle group. Remember to end with <see cref="EndToggleGroup"/>.
        /// </summary>
        /// <param name="key">The key of the group.</param>
        /// <param name="enabled">Value indicating if the group is enabled.</param>
        /// <param name="visible">Value indicating if the group is visible.</param>
        /// <param name="title">The title of the group.</param>
        /// <param name="animationDuration">Duration of the animation.</param>
        /// <returns>Value indicating if the group is toggled.</returns>
        public static bool BeginToggleGroup(object key, ref bool enabled, ref bool visible, string title, float animationDuration)
        {
            var rect = GUILayoutUtility.GetRect(16, SirenixGUIStyles.ToggleGroupTitleBg.fixedHeight, SirenixGUIStyles.ToggleGroupTitleBg);
            rect = EditorGUI.IndentedRect(rect);
            GUIHelper.IndentRect(ref rect);
            rect.xMin += 3;
            rect.xMax -= 3;
            GUI.Box(rect, title, SirenixGUIStyles.ToggleGroupTitleBg);
            if (Event.current.type == EventType.Repaint)
            {
                var toggleIconRect = rect;
                toggleIconRect.xMin = toggleIconRect.xMax - 20;
                toggleIconRect.size *= 0.8f;
                //GL.sRGBWrite = QualitySettings.activeColorSpace == ColorSpace.Linear;
                GUI.DrawTexture(toggleIconRect, visible ? EditorIcons.TriangleDown.Active : EditorIcons.TriangleLeft.Active);
                //GL.sRGBWrite = false;
            }

            var toggleRect = new Rect(rect.x + 4f, rect.y + 4f, 13f, 13f);
            var e = Event.current;

            if (Event.current.type == EventType.Repaint)
            {
                SirenixGUIStyles.ToggleGroupCheckbox.Draw(toggleRect, false, false, enabled, false);
            }

            if (rect.Contains(e.mousePosition) && Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                toggleRect.x -= 2;
                toggleRect.y -= 2;
                toggleRect.width += 4;
                toggleRect.height += 4;

                if (toggleRect.Contains(e.mousePosition))
                {
                    GUIHelper.RequestRepaint();
                    enabled = !enabled;
                    e.Use();
                    GUIHelper.RemoveFocusControl();
                }
                else
                {
                    GUIHelper.RequestRepaint();
                    visible = !visible;
                }
            }

            var result = BeginFadeGroup(key, visible, animationDuration);
            GUILayout.BeginVertical(SirenixGUIStyles.None);
            GUIHelper.PushGUIEnabled(enabled);
            GUIHelper.PushHierarchyMode(false);
            EditorGUI.indentLevel++;
            return result;
        }

        /// <summary>
        /// Ends drawing a toggle group started by <see cref="BeginToggleGroup(object, ref bool, ref bool, string, float)"/>.
        /// </summary>
        public static void EndToggleGroup()
        {
            EditorGUI.indentLevel--;
            GUIHelper.PopHierarchyMode();
            GUIHelper.PopGUIEnabled();
            GUILayout.EndVertical();
            EndFadeGroup();
        }

        /// <summary>
        /// Begins drawing a horizontal auto scroll box. Remember to end with <see cref="EndHorizontalAutoScrollBox"/>.
        /// </summary>
        /// <param name="key">The for the field.</param>
        /// <param name="options">The GUILayout options.</param>
        /// <returns>The rect used for the field.</returns>
        public static Rect BeginHorizontalAutoScrollBox(object key, params GUILayoutOption[] options)
        {
            if (currentBeginAutoScrollBoxInfo != null && currentBeginAutoScrollBoxInfo.IsActive == true)
            {
                Debug.LogError("EndAutoScrollBox must be called before beginning another.");
            }

            var config = GUIHelper.GetTemporaryNullableContext<BeginAutoScrollBoxInfo>(drawColorPaletteColorPickerKey, key);
            config.Value = currentBeginAutoScrollBoxInfo = config.Value ?? new BeginAutoScrollBoxInfo();
            config.Value.IsActive = true;
            config.Value.TmpOuterRect = EditorGUILayout.BeginVertical(options);

            if (Event.current.type == EventType.Repaint)
            {
                currentBeginAutoScrollBoxInfo.OuterRect = currentBeginAutoScrollBoxInfo.TmpOuterRect;
            }

            bool wasScrollWheel = Event.current.type == EventType.ScrollWheel;
            if (wasScrollWheel) Event.current.type = EventType.Ignore;
            GUILayout.BeginScrollView(config.Value.ScrollPosition, GUIStyle.none, GUIStyle.none, options);
            if (wasScrollWheel) Event.current.type = EventType.ScrollWheel;

            currentBeginAutoScrollBoxInfo.InnerRect = EditorGUILayout.BeginHorizontal(options);
            return currentBeginAutoScrollBoxInfo.OuterRect;
        }

        /// <summary>
        /// Ends drawing a horizontal auto scroll box started by <see cref="BeginHorizontalAutoScrollBox(object, GUILayoutOption[])"/>.
        /// </summary>
        public static void EndHorizontalAutoScrollBox()
        {
            if (currentBeginAutoScrollBoxInfo == null || currentBeginAutoScrollBoxInfo.IsActive == false)
            {
                Debug.LogError("EndAutoScrollBox was called before BeginAutoScrollBox.");
            }
            EditorGUILayout.EndHorizontal();

            bool wasScrollWheel = Event.current.type == EventType.ScrollWheel;
            if (wasScrollWheel) Event.current.type = EventType.Ignore;
            GUILayout.EndScrollView();
            if (wasScrollWheel) Event.current.type = EventType.ScrollWheel;

            EditorGUILayout.EndVertical();

            currentBeginAutoScrollBoxInfo.TmpOuterRect.x += 10;
            currentBeginAutoScrollBoxInfo.TmpOuterRect.width -= 20;
            currentBeginAutoScrollBoxInfo.TmpOuterRect.y -= 10;
            currentBeginAutoScrollBoxInfo.TmpOuterRect.height += 20;
            if (currentBeginAutoScrollBoxInfo.TmpOuterRect.Contains(Event.current.mousePosition))
            {
                float overflow = Mathf.Max(0, currentBeginAutoScrollBoxInfo.InnerRect.width - currentBeginAutoScrollBoxInfo.TmpOuterRect.width);
                float percentage = (Event.current.mousePosition.x - currentBeginAutoScrollBoxInfo.TmpOuterRect.x) / currentBeginAutoScrollBoxInfo.TmpOuterRect.width;
                currentBeginAutoScrollBoxInfo.ScrollPosition.x = percentage * overflow;
                GUIHelper.RequestRepaint();
            }
            currentBeginAutoScrollBoxInfo.IsActive = false;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static int SlideRectInt(Rect rect, int id, int t)
        {
            if (!GUI.enabled) return t;

            long value = t;
            double dummy = 0;
            EditorGUI_Internals.DragNumberValue(rect, id, false, ref dummy, ref value, EditorGUI_Internals.GetIntDragSensitivity());

            return (int)value;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static long SlideRectLong(Rect rect, int id, long t)
        {
            if (!GUI.enabled) return t;

            double dummy = 0;
            EditorGUI_Internals.DragNumberValue(rect, id, false, ref dummy, ref t, EditorGUI_Internals.GetIntDragSensitivity());

            return t;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static float SlideRect(Rect rect, int id, float t)
        {
            if (!GUI.enabled) return t;

            double value = t;
            long dummy = 0;
            EditorGUI_Internals.DragNumberValue(rect, id, true, ref value, ref dummy, EditorGUI_Internals.GetFloatDragSensitivity());

            return (float)value;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled to change a value up or down.
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="id">The control ID for the sliding.</param>
        /// <param name="t">The current value.</param>
        /// <returns>
        /// The current value.
        /// </returns>
        public static double SlideRectDouble(Rect rect, int id, double t)
        {
            if (!GUI.enabled) return t;

            long dummy = 0;
            EditorGUI_Internals.DragNumberValue(rect, id, true, ref t, ref dummy, EditorGUI_Internals.GetFloatDragSensitivity());

            return t;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled
        /// </summary>
        /// <param name="rect">The grabbable rect.</param>
        /// <param name="cursor">The cursor.</param>
        /// <returns>
        /// The the mouse delta position.
        /// </returns>
        public static Vector2 SlideRect(Rect rect, MouseCursor cursor = MouseCursor.SlideArrow)
        {
            if (!GUI.enabled) return Vector2.zero;

            EditorGUIUtility.AddCursorRect(rect, cursor);

            var id = GUIUtility.GetControlID(FocusType.Passive);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                EditorGUIUtility.SetWantsMouseJumping(1);
                Event.current.Use();
            }
            else if (GUIUtility.hotControl == id)
            {
                // Update T
                if (Event.current.type == EventType.MouseDrag)
                {
                    Event.current.Use();
                    GUI.changed = true;
                    return Event.current.delta;
                }

                // Release
                else if (Event.current.type == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    EditorGUIUtility.SetWantsMouseJumping(0);
                    Event.current.Use();
                }
            }
            return Vector2.zero;
        }

        /// <summary>
        /// Creates a rect that can be grabbed and pulled
        /// </summary>
        /// <param name="position">The position.</param>
        /// <param name="rect">The grabbable rect.</param>
        /// <returns>
        /// The the mouse delta position.
        /// </returns>
        public static Vector2 SlideRect(Vector2 position, Rect rect)
        {
            if (!GUI.enabled) return position;

            EditorGUIUtility.AddCursorRect(rect, MouseCursor.SlideArrow);

            var id = GUIUtility.GetControlID(FocusType.Passive);

            if (GUI.enabled && Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.hotControl = id;
                Event.current.Use();
                var p = Event.current.mousePosition - rect.position;
                return p;
            }
            else if (GUIUtility.hotControl == id)
            {
                if (Event.current.type == EventType.MouseDrag)
                {
                    GUI.changed = true;
                    Event.current.Use();
                    var p = Event.current.mousePosition - rect.position;
                    return p;
                }
                else if (Event.current.type == EventType.MouseUp)
                {
                    GUIUtility.hotControl = 0;
                    Event.current.Use();
                }
            }
            return position;
        }

        /// <summary>
        /// Draws a field for a value of type T - dynamically choosing an appropriate drawer for the type.
        /// Currently supported are: char, string, sbyte, byte, short, ushort, int, uint, long, ulong, float, double, decimal, Guid and all enums.
        /// </summary>
        /// <typeparam name="T">The type of the value to draw.</typeparam>
        /// <param name="label">The label of the fields.</param>
        /// <param name="value">The value to draw.</param>
        /// <param name="options">The layout options.</param>
        /// <returns>The possibly changed value.</returns>
        public static T DynamicPrimitiveField<T>(GUIContent label, T value, params GUILayoutOption[] options)
        {
            options = options ?? GUILayoutOptions.EmptyGUIOptions;

            Delegate del;

            if (typeof(T).IsEnum)
            {
                if (typeof(T).GetAttribute<FlagsAttribute>() != null)
                {
                    return (T)(object)SirenixEditorFields.EnumDropdown(label, (Enum)(object)value, options);
                }
                else
                {
                    return (T)(object)SirenixEditorFields.EnumDropdown(label, (Enum)(object)value, options);
                }
            }

            if (!DynamicFieldDrawers.TryGetValue(typeof(T), out del))
            {
                EditorGUILayout.LabelField(label, new GUIContent("DynamicPrimitiveField does not support drawing the type '" + typeof(T).GetNiceName() + "'."));
                return value;
            }
            else
            {
                var drawerFunc = (Func<GUIContent, T, GUILayoutOption[], T>)del;
                return drawerFunc(label, value, options);
            }
        }

        /// <summary>
        /// Checks whether a given type can be drawn as a dynamic field by <see cref="DynamicPrimitiveField{T}(GUIContent, T, GUILayoutOption[])"/>
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns>True if the type can be drawn, otherwise false.</returns>
        public static bool DynamicPrimitiveFieldCanDraw<T>()
        {
            return typeof(T).IsEnum || DynamicFieldDrawers.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Gets the feature rich control rect.
        /// </summary>
        public static Rect GetFeatureRichControlRect(GUIContent label, out int controlId, out bool hasKeyboardFocus, out Rect valueRect, params GUILayoutOption[] options)
        {
            return GetFeatureRichControlRect(label, 0, out controlId, out hasKeyboardFocus, out valueRect, options);
        }

        /// <summary>
        /// Gets the feature rich control rect.
        /// </summary>
        public static Rect GetFeatureRichControlRect(GUIContent label, int height, out int controlId, out bool hasKeyboardFocus, out Rect valueRect, params GUILayoutOption[] options)
        {
            Rect totalRect;

            if (height == 0)
            {
                valueRect = EditorGUILayout.GetControlRect(label != null, options);
            }
            else
            {
                valueRect = EditorGUILayout.GetControlRect(label != null, height, options);
            }
            totalRect = valueRect;

            controlId = GUIUtility.GetControlID(FocusType.Keyboard);

            if (label == null)
            {
                valueRect = EditorGUI.IndentedRect(valueRect);
            }
            else
            {
                totalRect.xMin += EditorGUI.indentLevel * 15f;
                valueRect = EditorGUI.PrefixLabel(valueRect, controlId, label);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && totalRect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = controlId;
            }

            hasKeyboardFocus = GUIUtility.keyboardControl == controlId && GUIHelper.CurrentWindow == EditorWindow.focusedWindow;
            return totalRect;
        }

        /// <summary>
        /// Creates a control ID that handles keyboard control, focused editor window, indentation and prefix label correctly.
        /// </summary>
        /// <param name="rect">The rect to make a feature rich control for.</param>
        /// <param name="label">The label for the control. Leave <c>null</c> for no label.</param>
        /// <param name="controlId">The created control ID.</param>
        /// <param name="hasKeyboardFocus">A value indicating whether or not the control has keyboard focus.</param>
        public static Rect GetFeatureRichControl(Rect rect, GUIContent label, out int controlId, out bool hasKeyboardFocus)
        {
            controlId = GUIUtility.GetControlID(FocusType.Keyboard);

            Rect valueRect;

            if (label == null)
            {
                valueRect = EditorGUI.IndentedRect(rect);
            }
            else
            {
                valueRect = EditorGUI.PrefixLabel(rect, controlId, label);
            }

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = controlId;
            }

            hasKeyboardFocus = GUIUtility.keyboardControl == controlId && GUIHelper.CurrentWindow == EditorWindow.focusedWindow;
            return valueRect;
        }

        /// <summary>
        /// Creates a control ID that handles keyboard control, focused editor window, indentation and prefix label correctly.
        /// </summary>
        /// <param name="rect">The rect to make a feature rich control for.</param>
        /// <param name="controlId">The created control ID.</param>
        /// <param name="hasKeyboardFocus">A value indicating whether or not the control has keyboard focus.</param>
        public static Rect GetFeatureRichControl(Rect rect, out int controlId, out bool hasKeyboardFocus)
        {
            return GetFeatureRichControl(rect, null, out controlId, out hasKeyboardFocus);
        }

        private static readonly Rect DefaultUVRect = new Rect(Vector2.zero, Vector2.one);
        
        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="slice">The amount of slicing applied to the texture on all sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, int slice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, DefaultUVRect, slice, slice, slice, slice, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="horizontalSlice">The amount of slicing applied to the left and right sides.</param>
        /// <param name="verticalSlice">The amount of slicing applied to the top and bottom sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, int horizontalSlice, int verticalSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, DefaultUVRect, horizontalSlice, horizontalSlice, verticalSlice, verticalSlice, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="leftSlice">The amount of slicing applied to the left side.</param>
        /// <param name="rightSlice">The amount of slicing applied to the right side.</param>
        /// <param name="topSlice">The amount of slicing applied to the top side.</param>
        /// <param name="bottomSlice">The amount of slicing applied to the bottom side.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, int leftSlice, int rightSlice, int topSlice, int bottomSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, DefaultUVRect, leftSlice, rightSlice, topSlice, bottomSlice, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="uvRect">The UV-coordinates to use.</param>
        /// <param name="slice">The amount of slicing applied to the texture on all sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Rect uvRect, int slice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, uvRect, slice, slice, slice, slice, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="uvRect">The UV-coordinates to use.</param>
        /// <param name="horizontalSlice">The amount of slicing applied to the left and right sides.</param>
        /// <param name="verticalSlice">The amount of slicing applied to the top and bottom sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Rect uvRect, int horizontalSlice, int verticalSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, uvRect, horizontalSlice, horizontalSlice, verticalSlice, verticalSlice, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="uvRect">The UV-coordinates to use.</param>
        /// <param name="leftSlice">The amount of slicing applied to the left side.</param>
        /// <param name="rightSlice">The amount of slicing applied to the right side.</param>
        /// <param name="topSlice">The amount of slicing applied to the top side.</param>
        /// <param name="bottomSlice">The amount of slicing applied to the bottom side.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Rect uvRect, int leftSlice, int rightSlice, int topSlice, int bottomSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, uvRect, leftSlice, rightSlice, topSlice, bottomSlice, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="color">The <see cref="Color"/> that modulates the output.</param>
        /// <param name="slice">The amount of slicing applied to the texture on all sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Color color, int slice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, DefaultUVRect, slice, slice, slice, slice, color, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="color">The <see cref="Color"/> that modulates the output.</param>
        /// <param name="horizontalSlice">The amount of slicing applied to the left and right sides.</param>
        /// <param name="verticalSlice">The amount of slicing applied to the top and bottom sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Color color, int horizontalSlice, int verticalSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, DefaultUVRect, horizontalSlice, horizontalSlice, verticalSlice, verticalSlice, color,
                                 GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="color">The <see cref="Color"/> that modulates the output.</param>
        /// <param name="leftSlice">The amount of slicing applied to the left side.</param>
        /// <param name="rightSlice">The amount of slicing applied to the right side.</param>
        /// <param name="topSlice">The amount of slicing applied to the top side.</param>
        /// <param name="bottomSlice">The amount of slicing applied to the bottom side.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Color color, int leftSlice, int rightSlice, int topSlice, int bottomSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, DefaultUVRect, leftSlice, rightSlice, topSlice, bottomSlice, color, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="color">The <see cref="Color"/> that modulates the output.</param>
        /// <param name="uvRect">The UV-coordinates to use.</param>
        /// <param name="slice">The amount of slicing applied to the texture on all sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Color color, Rect uvRect, int slice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, uvRect, slice, slice, slice, slice, color, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="color">The <see cref="Color"/> that modulates the output.</param>
        /// <param name="uvRect">The UV-coordinates to use.</param>
        /// <param name="horizontalSlice">The amount of slicing applied to the left and right sides.</param>
        /// <param name="verticalSlice">The amount of slicing applied to the top and bottom sides.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Color color, Rect uvRect, int horizontalSlice, int verticalSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, uvRect, horizontalSlice, horizontalSlice, verticalSlice, verticalSlice, color, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a <see cref="Texture"/> with support for slicing the Texture.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="color">The <see cref="Color"/> that modulates the output.</param>
        /// <param name="uvRect">The UV-coordinates to use.</param>
        /// <param name="leftSlice">The amount of slicing applied to the left side.</param>
        /// <param name="rightSlice">The amount of slicing applied to the right side.</param>
        /// <param name="topSlice">The amount of slicing applied to the top side.</param>
        /// <param name="bottomSlice">The amount of slicing applied to the bottom side.</param>
        public static void DrawTextureSliced(Rect rect, Texture texture, Color color, Rect uvRect, int leftSlice, int rightSlice, int topSlice, int bottomSlice)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            Graphics.DrawTexture(rect, texture, uvRect, leftSlice, rightSlice, topSlice, bottomSlice, color, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a repeating <see cref="Texture"/> in a given <see cref="Rect"/>.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="scale">Amount of scaling applied to the output.</param>
        public static void DrawTextureRepeat(Rect rect, Texture texture, float scale = 1.0f) => DrawTextureRepeat(rect, texture, Color.gray, scale);
        
        /// <summary>
        /// Draws a repeating <see cref="Texture"/> in a given <see cref="Rect"/>.
        /// </summary>
        /// <param name="rect">The <see cref="Rect"/> to draw the <see cref="Texture"/> in.</param>
        /// <param name="texture">The <see cref="Texture"/> to draw.</param>
        /// <param name="color">The <see cref="Color"/> that modulates the output.</param>
        /// <param name="scale">Amount of scaling applied to the output.</param>
        public static void DrawTextureRepeat(Rect rect, Texture texture, Color color, float scale = 1.0f)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }
            
            var uvRect = new Rect(0, 0, rect.width / texture.width * scale, rect.height / texture.height * scale);
            Graphics.DrawTexture(rect, texture, uvRect, 0, 0, 0, 0, color, GUI_Internals.BlendMaterial);
        }

        /// <summary>
        /// Draws a button with a given <see cref="ButtonSizes">Size</see>.
        /// </summary>
        /// <param name="text">Text to display on the button.</param>
        /// <param name="size">The size of the button.</param>
        /// <returns><c>true</c> when the button is pressed; otherwise <c>false</c>.</returns>
        public static bool Button(string text, ButtonSizes size)
        {
            Rect rect = GUILayoutUtility.GetRect(0, (int) size);
            return GUI.Button(rect, text);
        }

        public static void DrawRoundRect(Rect position, Color color, float cornerRadii)
        {
            GUI.DrawTexture(position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1.0f, color, 0, cornerRadii);
        }

        public static void DrawRoundRect(Rect position, Color color, float cornerRadii, Color borderColor, float borderThickness)
        {
            GUI.DrawTexture(position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1.0f, color, 0, cornerRadii);
            GUI.DrawTexture(position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1.0f, borderColor, borderThickness, cornerRadii);
        }

        public static void DrawRoundRect(Rect position,
                                         Color color,
                                         float topLeftCornerRadius,
                                         float topRightCornerRadius,
                                         float bottomLeftCornerRadius,
                                         float bottomRightCornerRadius)
        {
            var cornerRadii = new Vector4(topLeftCornerRadius, topRightCornerRadius, bottomRightCornerRadius, bottomLeftCornerRadius);

            GUI.DrawTexture(position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1.0f, color, Vector4.zero, cornerRadii);
        }

        public static void DrawRoundRect(Rect position,
                                         Color color,
                                         float topLeftCornerRadius,
                                         float topRightCornerRadius,
                                         float bottomLeftCornerRadius,
                                         float bottomRightCornerRadius,
                                         Color borderColor,
                                         float borderThickness)
        {
            var cornerRadii = new Vector4(topLeftCornerRadius, topRightCornerRadius, bottomRightCornerRadius, bottomLeftCornerRadius);
            var borderThicknessVec4 = new Vector4(borderThickness, borderThickness, borderThickness, borderThickness);

            GUI.DrawTexture(position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1.0f, color, Vector4.zero, cornerRadii);
            GUI.DrawTexture(position, Texture2D.whiteTexture, ScaleMode.StretchToFill, false, 1.0f, borderColor, borderThicknessVec4, cornerRadii);
        }

        public static bool DoButton(Rect position, out bool isMouseOver)
        {
            isMouseOver = position.Contains(Event.current.mousePosition);

            return isMouseOver && Event.current.type == EventType.MouseDown && Event.current.button == 0;
        }

        public static bool DoButton(Rect position, out int controlId, out bool isMouseOver, out bool isActive)
        {
            controlId = GUIUtility.GetControlID(FocusType.Keyboard);

            isMouseOver = Event.current.IsMouseOver(position);

            return DoButton(controlId, isMouseOver, out isActive);
        }

        public static bool DoButton(Rect position, int controlId, out bool isMouseOver, out bool isActive)
        {
            isMouseOver = Event.current.IsMouseOver(position);

            return DoButton(controlId, isMouseOver, out isActive);
        }

        public static bool DoButton(int controlId, bool isMouseOver, out bool isActive)
        {
            Event e = Event.current;

            if (controlId == 0)
            {
                bool isPressed = isMouseOver && e.type == EventType.MouseDown && e.button == 0;

                if (isPressed)
                {
                    e.Use();

                    isActive = true;
                    return true;
                }

                isActive = false;
                return false;
            }

            isActive = GUIUtility.hotControl == controlId;

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (isMouseOver && e.button == 0)
                    {
                        e.Use();

                        GUIUtility.hotControl = controlId;
                    }

                    break;

                case EventType.MouseUp:
                    if (isActive)
                    {
                        e.Use();

                        GUIUtility.hotControl = 0;

                        if (isMouseOver)
                        {
                            return true;
                        }
                    }

                    break;

                case EventType.MouseDrag:
                    if (isActive)
                    {
                        e.Use();

                        if (!isMouseOver)
                        {
                            GUIUtility.hotControl = 0;
                        }
                    }

                    break;

                case EventType.KeyDown:
                    if (e.modifiers != EventModifiers.None)
                    {
                        break;
                    }

                    bool isKeyboardFocus = GUIUtility.keyboardControl == controlId;

                    bool isReturnPressed = isKeyboardFocus &&
                                           (e.keyCode == KeyCode.Return ||
                                            e.keyCode == KeyCode.KeypadEnter ||
                                            e.keyCode == KeyCode.Space);

                    if (isReturnPressed)
                    {
                        e.Use();

                        return true;
                    }

                    break;
            }

            return false;
        }

        internal static void BeginDrawOpenInspector(Rect rect, UnityEngine.Object obj, Rect btnRect)
        {
            GUI.Label(rect, GUIHelper.TempContent(string.Empty, "Inspect Object"));

            if (Event.current.isMouse && Event.current.IsMouseOver(rect))
            {
                GUIHelper.RequestRepaint();
            }
            
            if (Event.current.rawType == EventType.MouseDown && Event.current.IsMouseOver(rect) && obj != null)
            {
                if (obj is Sprite && AssetDatabase.Contains(obj))
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    obj = AssetDatabase.LoadMainAssetAtPath(path) ?? obj;
                }

                if (Event.current.button == 0 || obj.GetType() == typeof(GameObject))
                {
                    GUIHelper.OpenInspectorWindow(obj);
                }
                else if (Event.current.button == 1)
                {
                    GUIHelper.OpenEditorInOdinDropDown(obj, btnRect);
                }

                GUIHelper.ExitGUI(true);
            }
        }

        internal static void EndDrawOpenInspector(Rect rect, UnityEngine.Object obj)
        {
            var prevEnabled = GUI.enabled;
            GUI.enabled = obj != null;
            rect.x -= 2;
            rect = rect.AlignRight(rect.height);
            EditorIcons.Pen.Draw(rect);
            GUI.enabled = prevEnabled;
        }

        private class VerticalMenuListInfo
        {
            public int SelectedItemIndex;
            public int MenuItemCount;
            public int CurrentIndex;
            public int? NextSelectedIndex = null;
            public float Height;
            public Vector2 ScrollPositiion;
            public bool ScrollToCurrent;
        }

        private struct ShakeGroup
        {
            public bool IsShaking;
            public float Duration;
            public float Time;

            public static ShakeGroup Create(float duration)
            {
                return new ShakeGroup()
                {
                    Duration = duration,
                };
            }

            public static ShakeGroup Create()
            {
                return new ShakeGroup()
                {
                    Duration = ShakingAnimationDuration,
                };
            }
        }

        private class BeginAutoScrollBoxInfo
        {
            public Rect TmpOuterRect;
            public Rect InnerRect;
            public bool IsActive;
            public Rect OuterRect;
            public Vector2 ScrollPosition;
        }

        public static Rect BeginVerticalWithoutUsingControlID(GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayoutGroup_Internal group = GUILayoutUtility_Internals.BeginLayoutGroup(style, options);
            group.IsVertical = true;
            
            if (Event.current.type == EventType.Repaint)
            {
                var controlId = 0;
                style.Draw(group.rect, GUIContent.none, controlId, false, group.rect.Contains(Event.current.mousePosition));
            }
            
            return group.rect;
        }

        public static Rect BeginHorizontalWithoutUsingControlID(GUIStyle style, params GUILayoutOption[] options)
        {
            GUILayoutGroup_Internal group = GUILayoutUtility_Internals.BeginLayoutGroup(style, options);
            group.IsVertical = false;

            if (Event.current.type == EventType.Repaint)
            {
                var controlId = 0;
                style.Draw(group.rect, GUIContent.none, controlId, false, group.rect.Contains(Event.current.mousePosition));
            }

            return group.rect;
        }
    }
}
#endif