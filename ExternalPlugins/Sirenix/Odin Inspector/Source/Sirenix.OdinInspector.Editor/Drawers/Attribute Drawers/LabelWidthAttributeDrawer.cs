//-----------------------------------------------------------------------
// <copyright file="LabelWidthAttributeDrawer.cs" company="Sirenix ApS">
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

    using Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with the <see cref="LabelWidthAttribute"/>.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="GUIColorAttribute"/>

    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class LabelWidthAttributeDrawer : OdinAttributeDrawer<LabelWidthAttribute>
    {
        /// <summary>
        /// Draws the attribute.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var attribute = this.Attribute;

            if (attribute.Width < 0)
            {
                var labelWidth = GUIHelper.BetterLabelWidth + attribute.Width;
                GUIHelper.PushLabelWidth(labelWidth);
                HorizontalGroupAttributeDrawer.PushLabelWidthDefault(labelWidth);
            }
            else
            {
                GUIHelper.PushLabelWidth(attribute.Width);
                HorizontalGroupAttributeDrawer.PushLabelWidthDefault(attribute.Width);
            }

            this.CallNextDrawer(label);
            GUIHelper.PopLabelWidth();
            HorizontalGroupAttributeDrawer.PopLabelWidthDefault();
        }
    }
}
#endif