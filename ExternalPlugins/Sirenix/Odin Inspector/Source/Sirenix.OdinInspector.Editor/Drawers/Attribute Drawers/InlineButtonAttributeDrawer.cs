//-----------------------------------------------------------------------
// <copyright file="InlineButtonAttributeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor;
    using Sirenix.OdinInspector.Editor.ActionResolvers;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="InlineButtonAttribute"/>
    /// </summary>
    [DrawerPriority(DrawerPriorityLevel.WrapperPriority)]
    public sealed class InlineButtonAttributeDrawer<T> : OdinAttributeDrawer<InlineButtonAttribute, T>
    {
        private ValueResolver<string> labelGetter;
        private ActionResolver clickAction;
        private ValueResolver<bool> showIfGetter;
        private ValueResolver<Color> buttonColorGetter;
        private ValueResolver<Color> textColorGetter;

        private bool show = true;
        private string tooltip;

        protected override void Initialize()
        {
            if (this.Attribute.Label != null)
            {
                this.labelGetter = ValueResolver.GetForString(this.Property, this.Attribute.Label);
            }
            else
            {
                this.labelGetter = ValueResolver.Get<string>(this.Property, null, Attribute.Action.SplitPascalCase());
            }

            this.clickAction = ActionResolver.Get(this.Property, this.Attribute.Action);

            this.showIfGetter = ValueResolver.Get(this.Property, this.Attribute.ShowIf, true);
            this.buttonColorGetter = ValueResolver.Get<Color>(this.Property, this.Attribute.ButtonColor);
            this.textColorGetter = ValueResolver.Get<Color>(this.Property, this.Attribute.TextColor, SirenixGUIStyles.Button.normal.textColor);

            this.show = this.showIfGetter.GetValue();

            this.tooltip = this.Property.GetAttribute<PropertyTooltipAttribute>()?.Tooltip
                           ?? this.Property.GetAttribute<TooltipAttribute>()?.tooltip;
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.labelGetter.HasError 
                || this.clickAction.HasError 
                || this.showIfGetter.HasError 
                || this.buttonColorGetter.HasError 
                || this.textColorGetter.HasError)
            {
                this.labelGetter.DrawError();
                this.clickAction.DrawError();
                this.buttonColorGetter.DrawError();
                this.textColorGetter.DrawError();
                this.CallNextDrawer(label);
                return;
            }
            
            if (Event.current.type == EventType.Layout)
            {
                this.show = this.showIfGetter.GetValue();
            }

            if (this.show)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.BeginVertical();
                this.CallNextDrawer(label);
                EditorGUILayout.EndVertical();

                var buttonLabel = new GUIContent(this.labelGetter.GetValue(), this.tooltip);
                SirenixEditorGUI.CalculateMinimumSDFIconButtonWidth(buttonLabel.text, null, this.Attribute.Icon != SdfIconType.None, EditorGUIUtility.singleLineHeight, out _, out _, out _, out var btnWidth);

                var btnRect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight, GUILayout.MaxWidth(btnWidth));
                var btnColor = this.buttonColorGetter.GetValue();
                var btnTextColor = this.textColorGetter.GetValue();
                if (SirenixEditorGUI.SDFIconButton(btnRect, buttonLabel, btnColor, btnTextColor, this.Attribute.Icon, this.Attribute.IconAlignment))
                {
                    InvokeButton(buttonLabel);
                }

                EditorGUILayout.EndHorizontal();
            }
            else
            {
                this.CallNextDrawer(label);
            }
        }

        private void InvokeButton(GUIContent buttonLabel)
        {
            this.Property.RecordForUndo("Click " + buttonLabel);
            this.clickAction.DoActionForAllSelectionIndices();
        }
    }
}
#endif