//-----------------------------------------------------------------------
// <copyright file="GUITabPage.cs" company="Sirenix ApS">
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
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// A tab page created by <see cref="GUITabGroup"/>.
    /// </summary>
    /// <seealso cref="GUITabGroup"/>
    public class GUITabPage
    {
        private static GUIStyle innerContainerStyle;
        private static GUIStyle InnerContainerStyle
        {
            get
            {
                if (innerContainerStyle == null)
                {
                    innerContainerStyle = new GUIStyle()
                    {
                        padding = new RectOffset(3, 3, 3, 3),
                    };
                }

                return innerContainerStyle;
            }
        }

        private GUITabGroup tabGroup;
        private Color prevColor;
        private static int pageIndexIncrementer = 0;
        private bool isSeen;
        private bool isMessured = false;

        public int Order = 0;
        public readonly string TabName;
        public string Title;
        public SdfIconType Icon;
        public Color? TextColor;
        public Rect Rect;
        public bool IsActive;
        public bool IsVisible;
        public string Tooltip;

        internal GUITabPage(GUITabGroup tabGroup, string title)
        {
            this.TabName = title;
            this.Title = title;
            this.tabGroup = tabGroup;
            this.IsActive = true;
        }

        internal void OnBeginGroup()
        {
            pageIndexIncrementer = 0;
            this.isSeen = false;
        }

        internal void OnEndGroup()
        {
            if (Event.current.type == EventType.Repaint)
            {
                this.IsActive = this.isSeen;
            }
        }

        /// <summary>
        /// Begins the page.
        /// </summary>
        public bool BeginPage()
        {
            if (this.tabGroup.FixedHeight && this.isMessured == false)
                this.IsVisible = true;

            this.isSeen = true;

            if (this.IsVisible)
            {
                var options = GUILayoutOptions.Width(this.tabGroup.InnerContainerWidth + 3 ).ExpandHeight(this.tabGroup.ExpandHeight);
                var rect = EditorGUILayout.BeginVertical(InnerContainerStyle, options);

                GUIHelper.PushHierarchyMode(false);
                GUIHelper.PushLabelWidth(this.tabGroup.LabelWidth - 4);

                if (Event.current.type == EventType.Repaint)
                {
                    this.Rect = rect;
                }
                if (this.tabGroup.IsAnimating)
                {
                    this.prevColor = GUI.color;
                    var col = this.prevColor;
                    col.a *= this.tabGroup.CurrentPage == this ? this.tabGroup.T : 1 - this.tabGroup.T;
                    GUI.color = col;
                }
            }
            return this.IsVisible;
        }

        /// <summary>
        /// Ends the page.
        /// </summary>
        public void EndPage()
        {
            if (this.IsVisible)
            {
                GUIHelper.PopLabelWidth();
                GUIHelper.PopHierarchyMode();

                if (this.tabGroup.IsAnimating)
                {
                    GUI.color = this.prevColor;
                }
                EditorGUILayout.EndVertical();
            }

            if (Event.current.type == EventType.Repaint)
            {
                this.isMessured = true;
                this.Order = pageIndexIncrementer++;
            }
        }
    }
}
#endif