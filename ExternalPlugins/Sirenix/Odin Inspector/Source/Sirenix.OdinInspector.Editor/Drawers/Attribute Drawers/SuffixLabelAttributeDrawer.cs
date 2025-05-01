//-----------------------------------------------------------------------
// <copyright file="SuffixLabelAttributeDrawer.cs" company="Sirenix ApS">
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

    using UnityEngine;
    using UnityEditor;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities.Editor;
    using Sirenix.Utilities;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="SuffixLabelAttribute"/>.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="PropertyTooltipAttribute"/>
    /// <seealso cref="InlineButtonAttribute"/>
    /// <seealso cref="CustomValueDrawerAttribute"/>
    [AllowGUIEnabledForReadonly]
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class SuffixLabelAttributeDrawer : OdinAttributeDrawer<SuffixLabelAttribute>
    {
        private ValueResolver<string> labelResolver;
        private ValueResolver<Color> iconColorResolver;

        protected override void Initialize()
        {
            this.labelResolver = ValueResolver.GetForString(this.Property, this.Attribute.Label);
            this.iconColorResolver = ValueResolver.Get(this.Property, this.Attribute.IconColor, SirenixGUIStyles.RightAlignedGreyMiniLabel.normal.textColor);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.labelResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.labelResolver.ErrorMessage);
            }

            if (this.iconColorResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.iconColorResolver.ErrorMessage);
            }

            if (this.Attribute.Overlay)
            {
                this.CallNextDrawer(label);
                GUIHelper.PushGUIEnabled(true);

                if (this.Attribute.HasDefinedIcon)
                {
                    var rect = GUILayoutUtility.GetLastRect().HorizontalPadding(0, 22);
                    GUI.Label(rect, this.labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                    SdfIcons.DrawIcon(rect.AlignRight(12f).AddX(14f), this.Attribute.Icon, this.iconColorResolver.GetValue());
                }
                else
                {
                    var rect = GUILayoutUtility.GetLastRect().HorizontalPadding(0, 8);
                    GUI.Label(rect, this.labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel);
                }

                GUIHelper.PopGUIEnabled();
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical();
                this.CallNextDrawer(label);
                GUILayout.EndVertical();
                GUIHelper.PushGUIEnabled(true);
                GUILayout.Label(this.labelResolver.GetValue(), SirenixGUIStyles.RightAlignedGreyMiniLabel, GUILayoutOptions.ExpandWidth(false));

                if (this.Attribute.HasDefinedIcon)
                {
                    var iconRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.Width(12f));
                    SdfIcons.DrawIcon(iconRect.AlignCenter(12f), this.Attribute.Icon, this.iconColorResolver.GetValue());
                }

                GUIHelper.PopGUIEnabled();
                GUILayout.EndHorizontal();
            }
        }
    }
}
#endif