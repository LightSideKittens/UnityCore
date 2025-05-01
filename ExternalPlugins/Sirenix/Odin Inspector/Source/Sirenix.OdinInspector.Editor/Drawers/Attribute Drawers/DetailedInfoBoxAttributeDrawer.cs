//-----------------------------------------------------------------------
// <copyright file="DetailedInfoBoxAttributeDrawer.cs" company="Sirenix ApS">
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
    using ValueResolvers;

    /// <summary>
    ///	Draws properties marked with <see cref="DetailedInfoBoxAttribute"/>.
    /// </summary>
    /// <seealso cref="DetailedInfoBoxAttribute"/>
    /// <seealso cref="InfoBoxAttribute"/>
    /// <seealso cref="RequiredAttribute"/>
    /// <seealso cref="OnInspectorGUIAttribute"/>
    [DrawerPriority(0, 100, 0)]
    public sealed class DetailedInfoBoxAttributeDrawer : OdinAttributeDrawer<DetailedInfoBoxAttribute>
    {
        private bool drawMessageBox;
        //private bool hideDetailedMessage;
        private UnityEditor.MessageType messageType;
        private bool valid;

        private ValueResolver<bool> visibleIfGetter;
        private ValueResolver<string> messageGetter;
        private ValueResolver<string> detailsGetter;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.visibleIfGetter = ValueResolver.Get<bool>(this.Property, this.Attribute.VisibleIf, true);
            this.messageGetter = ValueResolver.GetForString(this.Property, this.Attribute.Message);
            this.detailsGetter = ValueResolver.GetForString(this.Property, this.Attribute.Details);

            this.valid = !(this.visibleIfGetter.HasError || this.messageGetter.HasError || this.detailsGetter.HasError);

            this.Property.State.Create<bool>("ShowDetailedMessage", false, false);
            //this.hideDetailedMessage = true;

            switch (this.Attribute.InfoMessageType)
            {
                case InfoMessageType.None:
                    this.messageType = UnityEditor.MessageType.None;
                    break;

                case InfoMessageType.Info:
                    this.messageType = UnityEditor.MessageType.Info;
                    break;

                case InfoMessageType.Warning:
                    this.messageType = UnityEditor.MessageType.Warning;
                    break;

                case InfoMessageType.Error:
                    this.messageType = UnityEditor.MessageType.Error;
                    break;

                default:
                    Debug.LogError("Unknown InfoBoxType: " + this.Attribute.InfoMessageType.ToString());
                    break;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            ValueResolver.DrawErrors(this.visibleIfGetter, this.messageGetter, this.detailsGetter);

            if (valid)
            {
                if (Event.current.type == EventType.Layout)
                {
                    this.drawMessageBox = this.visibleIfGetter.GetValue();
                }

                if (this.drawMessageBox)
                {
                    this.Property.State.Set<bool>("ShowDetailedMessage", !SirenixEditorGUI.DetailedMessageBox(
                        message:                this.messageGetter.GetValue(),
                        detailedMessage:        this.detailsGetter.GetValue(),
                        messageType:            this.messageType,
                        hideDetailedMessage: !this.Property.State.Get<bool>("ShowDetailedMessage")));
                }
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif