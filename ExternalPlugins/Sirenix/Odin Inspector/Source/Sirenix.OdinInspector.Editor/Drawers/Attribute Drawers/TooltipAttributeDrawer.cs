//-----------------------------------------------------------------------
// <copyright file="TooltipAttributeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="TooltipAttribute"/>.
    /// </summary>
    /// <seealso cref="TooltipAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class TooltipAttributeDrawer : OdinAttributeDrawer<TooltipAttribute>
    {
        private ValueResolver<string> tooltipResolver;

        protected override void Initialize()
        {
            this.tooltipResolver = ValueResolver.GetForString(this.Property, this.Attribute.tooltip);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (label != null)
            {
                if (this.tooltipResolver.HasError)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.tooltipResolver.ErrorMessage);
                }

                label.tooltip = this.tooltipResolver.GetValue();
            }

            this.CallNextDrawer(label);
        }
    }

    /// <summary>
    /// Draws properties marked with <see cref="PropertyTooltipAttribute"/>.
    /// </summary>
    /// <seealso cref="PropertyTooltipAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class PropertyTooltipAttributeDrawer : OdinAttributeDrawer<PropertyTooltipAttribute>
    {
        private ValueResolver<string> tooltipResolver;

        protected override void Initialize()
        {
            this.tooltipResolver = ValueResolver.GetForString(this.Property, this.Attribute.Tooltip);
        }

        protected override bool CanDrawAttributeProperty(InspectorProperty property)
        {
            // Buttons have their own tooltip handling... for some reason
            return property.Info.PropertyType != PropertyType.Method;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (label != null)
            {
                if (this.tooltipResolver.HasError)
                {
                    SirenixEditorGUI.ErrorMessageBox(this.tooltipResolver.ErrorMessage);
                }

                label.tooltip = this.tooltipResolver.GetValue();
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif