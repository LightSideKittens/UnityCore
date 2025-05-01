//-----------------------------------------------------------------------
// <copyright file="LabelTextAttributeDrawer.cs" company="Sirenix ApS">
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
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using UnityEditor;

    /// <summary>
    /// Draws properties marked with <see cref="LabelTextAttribute"/>.
    /// Creates a new GUIContent, with the provided label text, before calling further down in the drawer chain.
    /// </summary>
    /// <seealso cref="LabelTextAttribute"/>
    /// <seealso cref="HideLabelAttribute"/>
    /// <seealso cref="TooltipAttribute"/>
    /// <seealso cref="LabelWidthAttribute"/>
    /// <seealso cref="TitleAttribute"/>
    /// <seealso cref="HeaderAttribute"/>
    /// <seealso cref="GUIColorAttribute"/>
    [DrawerPriority(DrawerPriorityLevel.SuperPriority)]
    public sealed class LabelTextAttributeDrawer : OdinAttributeDrawer<LabelTextAttribute>
    {
        //private static readonly IValueResolver<string> TextResolver = ValueResolverUtility.CreateResolver<string>()
        //    .TryMemberOrExpression();

        //private IValueProvider<string> textProvider;

        private ValueResolver<string> textProvider;
        private ValueResolver<Color> iconColorResolver;
        private GUIContent overrideLabel;

        protected override void Initialize()
        {
            //var context = ValueResolverUtility.CreateContext(this);
            //this.textProvider = TextResolver.Resolve(context, this.Attribute.Text, this.Attribute.Text);

            this.textProvider = ValueResolver.GetForString(this.Property, this.Attribute.Text);
            this.iconColorResolver = ValueResolver.Get(this.Property, this.Attribute.IconColor, EditorStyles.label.normal.textColor);
            this.overrideLabel = new GUIContent();
        }

        /// <summary>
        /// Draws the attribute.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.textProvider.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.textProvider.ErrorMessage);
                this.CallNextDrawer(label);
                return;
            }

            if (this.iconColorResolver.HasError)
            {
                SirenixEditorGUI.ErrorMessageBox(this.iconColorResolver.ErrorMessage);
                this.CallNextDrawer(label);
                return;
            }

            var str = this.textProvider.GetValue();
            GUIContent useLabel;

            if (str == null && this.Attribute.Icon == SdfIconType.None)
            {
                useLabel = label;
            }
            else
            {
                var lbl = str ?? label.text;

                if (this.Attribute.NicifyText)
                {
                    lbl = ObjectNames.NicifyVariableName(lbl);
                }

                this.overrideLabel.text = lbl;
                useLabel = this.overrideLabel;

                if (this.Attribute.Icon != SdfIconType.None)
                {
                    var iconColor = this.iconColorResolver.GetValue();
                    useLabel.image = SdfIcons.CreateTransparentIconTexture(this.Attribute.Icon, iconColor, 16, 16, 0);
                }
            }

            this.CallNextDrawer(useLabel);
        }
    }
}
#endif