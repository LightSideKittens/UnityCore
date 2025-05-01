//-----------------------------------------------------------------------
// <copyright file="SearchField.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Reflection.Editor;
    using UnityEditor;
    using UnityEngine;

    public class SearchField
    {
        private bool wantsFocus;
        private int controlID;
        
        private static GUIStyle searchFieldStyle;
        private static GUIStyle SearchFieldStyle => searchFieldStyle = searchFieldStyle ?? new GUIStyle(EditorStyles.textField)
        {
            alignment = TextAnchor.MiddleLeft,
        };
        
        private static GUIStyle placeholderTextStyle;
        private static GUIStyle PlaceholderTextStyle => placeholderTextStyle = placeholderTextStyle ?? new GUIStyle(SearchFieldStyle)
        {
            normal = { textColor = Color.gray }
        };

        /// <summary>Initializes the <see cref="SearchField"/> and creates a permanent ID for the Control.</summary>
        /// <remarks>If you create this <see cref="SearchField"/> on a <see cref="ScriptableObject"/> such as <see cref="EditorWindow"/>,
        /// make sure to initialize this during OnEnable to ensure it gets initialized correctly.</remarks>
        public SearchField() => this.controlID = GUIUtility_Internals.GetPermanentControlID();

        public void Focus() => this.wantsFocus = true;
        public bool HasFocus() => GUIUtility.keyboardControl == this.controlID;
        
        public string Draw(string searchTerm, string placeholder = "")
        {
            return this.Draw(EditorGUILayout.GetControlRect(), searchTerm, placeholder);
        }

        public string Draw(Rect rect, string searchTerm, string placeholder = "")
        {
            var fontSize = Mathf.Min(Mathf.RoundToInt(rect.height * 0.65f), 12);
            SearchFieldStyle.fontSize = fontSize;
            PlaceholderTextStyle.fontSize = fontSize;
            
            var searchFieldPadding = new RectOffset(fontSize * 2, fontSize * 2, 0, 0);
            SearchFieldStyle.padding = searchFieldPadding;
            PlaceholderTextStyle.padding = searchFieldPadding;
            
            var buttonSize = fontSize * 2f;
            var iconPadding = fontSize * 0.5f;

            var searchButtonRect = rect.AlignLeft(buttonSize);
            var searchIconRect = searchButtonRect.HorizontalPadding(iconPadding);
            var clearButtonRect = rect.AlignRight(buttonSize);
            var clearIconRect = clearButtonRect.HorizontalPadding(iconPadding);
            
            EditorGUIUtility.AddCursorRect(searchButtonRect, MouseCursor.Arrow);
            
            var shouldDrawClearButton = !searchTerm.IsNullOrWhitespace();
            if (shouldDrawClearButton)
            {
                EditorGUIUtility.AddCursorRect(clearButtonRect, MouseCursor.Arrow);
            
                if (Event.current.OnMouseDown(clearButtonRect, 0))
                {
                    searchTerm = "";
                    GUIHelper.RemoveFocusControl();
                    GUI.changed = true;
                }                
            }

            searchTerm = EditorGUI_Internals.DoTextField(
                id: this.controlID,
                position: rect,
                text: searchTerm,
                style: SearchFieldStyle,
                allowedLetters: null,
                changed: out _, 
                reset: false, 
                multiline: false, 
                passwordField: false);

            if (this.wantsFocus && Event.current.type == EventType.Repaint)
            {
                GUIUtility.keyboardControl = this.controlID;
                EditorGUIUtility.editingTextField = true;
                this.wantsFocus = false;
            }

            var shouldDrawPlaceholderText = GUIUtility.keyboardControl != this.controlID && string.IsNullOrEmpty(searchTerm);
            if (shouldDrawPlaceholderText)
            {
                EditorGUI.LabelField(rect, placeholder, PlaceholderTextStyle);
            }

            if (shouldDrawClearButton)
            {
                SdfIcons.DrawIcon(clearIconRect, SdfIconType.X, GetIconColor(clearButtonRect));
            }

            SdfIcons.DrawIcon(searchIconRect, SdfIconType.Search, GetIconColor(searchButtonRect));

            return searchTerm;
        }
            
        private static Color GetIconColor(Rect iconRect)
        {
            return Event.current.IsMouseOver(iconRect)
                ? EditorGUIUtility.isProSkin ? Color.white : Color.black
                : EditorGUIUtility.isProSkin ? EditorStyles.label.normal.textColor : new Color(0.333f, 0.333f, 0.333f, 1f);
        }
    }
}
#endif