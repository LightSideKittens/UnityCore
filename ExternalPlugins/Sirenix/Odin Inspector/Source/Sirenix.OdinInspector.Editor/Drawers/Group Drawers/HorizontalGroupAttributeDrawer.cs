//-----------------------------------------------------------------------
// <copyright file="HorizontalGroupAttributeDrawer.cs" company="Sirenix ApS">
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

    using Utilities;
    using Utilities.Editor;
    using UnityEngine;
    using System.Linq;
    using UnityEditor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Reflection.Editor;

    /// <summary>
    /// Drawer for the <see cref="HorizontalGroupAttribute"/>
    /// </summary>
    /// <seealso cref="HorizontalGroupAttribute"/>
    public class HorizontalGroupAttributeDrawer : OdinGroupDrawer<HorizontalGroupAttribute>
    {
        private static readonly GUIScopeStack<float> labelWidthHorizontalGroupStack = new GUIScopeStack<float>();

        public static void PushLabelWidthDefault(float labelWidth)
        {
            labelWidthHorizontalGroupStack.Push(labelWidth);
        }

        public static void PopLabelWidthDefault()
        {
            labelWidthHorizontalGroupStack.Pop();
        }

        private ValueResolver<string> titleGetter;
        private float totalWidth;
        private HGroup[] groups;
        private bool enableAutomaticLabelWidth;

        private struct HGroup
        {
            public LayoutSize Width;
            public LayoutSize MinWidth;
            public LayoutSize maxWidth;
            public LayoutSize MarginLeft;
            public LayoutSize PaddingLeft;
            public LayoutSize MarginRight;
            public LayoutSize PaddingRight;
            public float labelWidth;
            internal bool hasCustomLabelWidth;
        }

        protected override void Initialize()
        {
            if (this.Attribute.Title != null)
            {
                this.titleGetter = ValueResolver.GetForString(this.Property, this.Attribute.Title);
            }

            this.enableAutomaticLabelWidth = !this.Attribute.DisableAutomaticLabelWidth;

            var fallbackLabelWidth = this.Attribute.LabelWidth;

            this.groups = new HGroup[this.Property.Children.Count];
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                var child = this.Property.Children[i];
                var attr = child.Children.Recurse()
                    .AppendWith(child)
                    .SelectMany(a => a.GetAttributes<HorizontalGroupAttribute>())
                    .FirstOrDefault(x => x.GroupID == Attribute.GroupID);

                HGroup group = default;
                if (attr == null)
                {
                    group.Width = LayoutSize.Auto;
                    group.MinWidth = default;
                    group.maxWidth = LayoutSize.Percentage(1);
                    group.labelWidth = fallbackLabelWidth;
                }
                else
                {
                    group.labelWidth = attr.LabelWidth > 0 ? attr.LabelWidth : fallbackLabelWidth;
                    group.MinWidth.Type = attr.MinWidth > 0 && attr.MinWidth < 1 ? SizeMode.Percentage : SizeMode.Pixels;
                    group.MinWidth.Value = attr.MinWidth == 0 ? 0 : attr.MinWidth;
                    group.maxWidth.Type = attr.MaxWidth >= 0 && attr.MaxWidth < 1 ? SizeMode.Percentage : SizeMode.Pixels;
                    group.maxWidth.Value = attr.MaxWidth == 0 ? 1 : attr.MaxWidth;

                    if (attr.Width > 0)
                    {
                        group.Width.Value = attr.Width;
                        group.Width.Type = attr.Width < 1 ? SizeMode.Percentage : SizeMode.Pixels;
                    }

                    if (attr.MarginLeft > 0)
                    {
                        group.MarginLeft.Type = attr.MarginLeft < 1 ? SizeMode.Percentage : SizeMode.Pixels;
                        group.MarginLeft.Value = attr.MarginLeft;
                    }

                    if (attr.MarginRight > 0)
                    {
                        group.MarginRight.Type = attr.MarginRight < 1 ? SizeMode.Percentage : SizeMode.Pixels;
                        group.MarginRight.Value = attr.MarginRight;
                    }

                    if (attr.PaddingLeft > 0)
                    {
                        group.PaddingLeft.Type = attr.PaddingLeft < 1 ? SizeMode.Percentage : SizeMode.Pixels;
                        group.PaddingLeft.Value = attr.PaddingLeft;
                    }

                    if (attr.PaddingRight > 0)
                    {
                        group.PaddingRight.Type = attr.PaddingRight < 1 ? SizeMode.Percentage : SizeMode.Pixels;
                        group.PaddingRight.Value = attr.PaddingRight;
                    }
                }
                group.hasCustomLabelWidth = group.labelWidth > 0;
                this.groups[i] = group;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.titleGetter != null)
            {
                if (this.titleGetter.HasError)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.titleGetter.ErrorMessage);
                }
                else
                {
                    SirenixEditorGUI.Title(this.titleGetter.GetValue(), null, TextAlignment.Left, false);
                }
            }

            var hasOverrideDefaultLabelWidth = labelWidthHorizontalGroupStack.Count > 0;
            var overrideLabelWidth = 0f;

            if (hasOverrideDefaultLabelWidth)
                overrideLabelWidth = labelWidthHorizontalGroupStack.Peek();

            SirenixEditorGUI.BeginIndentedVertical();
            {
                var prevFieldWidth = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 40;

                var rowRect = GUILayout_Internal.BeginRow();
                if (Event.current.type == EventType.Repaint)
                {
                    if (this.totalWidth != rowRect.width)
                        GUIHelper.RequestRepaint();
                    this.totalWidth = rowRect.width;
                }

                var pCount = Property.Children.Count;
                var gap = this.Attribute.Gap;
                var needsGap = false;

                for (int i = 0; i < pCount; i++)
                {
                    var child = Property.Children[i];

                    if (child.State.Visible)
                    {
                        if (needsGap && gap > 0) GUILayout_Internal.ColumnSpace(gap < 1 ? LayoutSize.Percentage(gap) : LayoutSize.Pixels(gap));

                        ref var group = ref this.groups[i];

                        if (group.PaddingLeft.Type != SizeMode.Auto) GUILayout_Internal.ColumnSpace(group.PaddingLeft);
                        if (group.MarginLeft.Type != SizeMode.Auto) GUILayout_Internal.ColumnSpace(group.MarginLeft);

                        var rect = GUILayout_Internal.BeginColumn(group.Width, group.MinWidth, group.maxWidth);

                        if (!group.hasCustomLabelWidth && Event.current.type == EventType.Repaint && this.enableAutomaticLabelWidth)
                        {
                            group.labelWidth = Mathf.Max(rect.width * 0.45f - 40f, 60f);
                        }

                        if (group.labelWidth != 0) GUIHelper.PushLabelWidth(hasOverrideDefaultLabelWidth ? overrideLabelWidth : group.labelWidth);
                        GUILayout.Space(0); // Prevents margins from being collapsed, or something...
                        child.Draw(child.Label);
                        GUILayout.Space(0);
                        GUILayout_Internal.EndColumn();
                        if (group.labelWidth != 0) GUIHelper.PopLabelWidth();

                        if (group.PaddingRight.Type != SizeMode.Auto) GUILayout_Internal.ColumnSpace(group.PaddingRight);
                        if (group.MarginRight.Type != SizeMode.Auto) GUILayout_Internal.ColumnSpace(group.MarginRight);

                        needsGap = true;
                    }
                }
                GUILayout_Internal.EndRow();

                EditorGUIUtility.fieldWidth = prevFieldWidth;
            }
            SirenixEditorGUI.EndIndentedVertical();
        }
    }
}
#endif