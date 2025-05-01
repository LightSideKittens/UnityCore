//-----------------------------------------------------------------------
// <copyright file="ButtonGroupAttributeDrawer.cs" company="Sirenix ApS">
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
    /// Draws all properties grouped together with the <see cref="ButtonGroupAttribute"/>
    /// </summary>
    /// <seealso cref="ButtonGroupAttribute"/>

    public class ButtonGroupAttributeDrawer : OdinGroupDrawer<ButtonGroupAttribute>
    {
        private int buttonHeight;
        private float[] buttonAlignment;
        private IconAlignment[] buttonIconAlignment;
        private bool[] stretch;

        protected override void Initialize()
        {
            var childCount = this.Property.Children.Count;
            this.buttonHeight = this.Attribute.ButtonHeight;
            this.buttonAlignment = new float[childCount];
            this.buttonIconAlignment = new IconAlignment[childCount];
            this.stretch = new bool[childCount];

            for (var i = 0; i < childCount; i++)
            {
                var button = this.Property.Children[i].GetAttribute<ButtonAttribute>();
                var hasButton = button != null;

                this.buttonHeight = Mathf.Max(this.buttonHeight, hasButton ? button.ButtonHeight : 0);

                this.buttonAlignment[i] = hasButton && button.HasDefinedButtonAlignment
                    ? button.ButtonAlignment
                    : this.Attribute.HasDefinedButtonAlignment
                        ? this.Attribute.ButtonAlignment
                        : GeneralDrawerConfig.Instance.ButtonAlignment;

                this.buttonIconAlignment[i] = hasButton && button.HasDefinedButtonIconAlignment
                    ? button.IconAlignment
                    : this.Attribute.HasDefinedButtonIconAlignment
                        ? this.Attribute.IconAlignment
                        : GeneralDrawerConfig.Instance.ButtonIconAlignment;

                this.stretch[i] = hasButton && button.HasDefinedStretch
                    ? button.Stretch
                    : this.Attribute.HasDefinedStretch
                        ? this.Attribute.Stretch
                        : GeneralDrawerConfig.Instance.StretchButtons;
            }

            if (this.buttonHeight == 0)
            {
                this.buttonHeight = GeneralDrawerConfig.Instance.ButtonHeight;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;

            SirenixEditorGUI.BeginIndentedHorizontal();

            for (var i = 0; i < property.Children.Count; i++)
            {
                var style = (GUIStyle)null;

                if (property.Children.Count != 1)
                {
                    if (i == 0)
                    {
                        style = SirenixGUIStyles.ButtonLeft;
                    }
                    else if (i == property.Children.Count - 1)
                    {
                        style = SirenixGUIStyles.ButtonRight;
                    }
                    else
                    {
                        style = SirenixGUIStyles.ButtonMid;
                    }
                }

                var child = property.Children[i];

                if (style != null)
                    child.Context.GetGlobal("ButtonStyle", style).Value = style;

                child.Context.GetGlobal("ButtonHeight", this.buttonHeight).Value = this.buttonHeight;
                child.Context.GetGlobal("ButtonAlignment", this.buttonAlignment[i]).Value = this.buttonAlignment[i];
                child.Context.GetGlobal("IconAlignment", this.buttonIconAlignment[i]).Value = this.buttonIconAlignment[i];
                child.Context.GetGlobal("StretchButton", this.stretch[i]).Value = this.stretch[i];
                child.Context.GetGlobal("DrawnByGroup", false).Value = true;

                DefaultMethodDrawer.DontDrawMethodParameters = true;
                child.Draw(child.Label);
                DefaultMethodDrawer.DontDrawMethodParameters = false;
            }

            SirenixEditorGUI.EndIndentedHorizontal();
        }
    }
}
#endif