//-----------------------------------------------------------------------
// <copyright file="InfoBoxAttributeDrawer.cs" company="Sirenix ApS">
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
    using UnityEditor;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    /// <summary>
    /// Draws properties marked with <see cref="InfoBoxAttribute"/>.
    /// Draws an info box above the property. Error and warning info boxes can be tracked by Odin Scene Validator.
    /// </summary>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="DetailedInfoBoxAttribute"/>
    /// <seealso cref="RequiredAttribute"/>
    /// <seealso cref="ValidateInputAttribute"/>
    [DrawerPriority(0, 10001, 0)]
    public sealed class InfoBoxAttributeDrawer : OdinAttributeDrawer<InfoBoxAttribute>
    {
        private bool drawMessageBox;
        private ValueResolver<bool> visibleIfResolver;
        private ValueResolver<string> messageResolver;
        private ValueResolver<Color> iconColorResolver;
        private MessageType messageType;

        protected override void Initialize()
        {
            this.visibleIfResolver = ValueResolver.Get(this.Property, this.Attribute.VisibleIf, true);
            this.messageResolver = ValueResolver.GetForString(this.Property, this.Attribute.Message);
            this.iconColorResolver = ValueResolver.Get(this.Property, this.Attribute.IconColor, EditorStyles.label.normal.textColor);

            this.drawMessageBox = this.visibleIfResolver.GetValue();

            switch (this.Attribute.InfoMessageType)
            {
                default:
                case InfoMessageType.None:
                    this.messageType = MessageType.None;
                    break;
                case InfoMessageType.Info:
                    this.messageType = MessageType.Info;
                    break;
                case InfoMessageType.Warning:
                    this.messageType = MessageType.Warning;
                    break;
                case InfoMessageType.Error:
                    this.messageType = MessageType.Error;
                    break;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            bool valid = true;

            if (this.visibleIfResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.visibleIfResolver.ErrorMessage);
                valid = false;
            }

            if (this.messageResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.messageResolver.ErrorMessage);
                valid = false;
            }

            if (this.iconColorResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.iconColorResolver.ErrorMessage);
                valid = false;
            }

            if (!valid)
            {
                this.CallNextDrawer(label);
                return;
            }

            if (this.Attribute.GUIAlwaysEnabled)
            {
                GUIHelper.PushGUIEnabled(true);
            }

            if (Event.current.type == EventType.Layout)
            {
                this.drawMessageBox = this.visibleIfResolver.GetValue();
            }

            if (this.drawMessageBox)
            {
                var message = this.messageResolver.GetValue();

                if (this.Attribute.HasDefinedIcon)
                {
                    SirenixEditorGUI.IconMessageBox(message, this.Attribute.Icon, this.iconColorResolver.GetValue());
                }
                else
                {
                    SirenixEditorGUI.MessageBox(message, this.messageType);
                }
            }

            if (this.Attribute.GUIAlwaysEnabled)
            {
                GUIHelper.PopGUIEnabled();
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif