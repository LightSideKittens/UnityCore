//-----------------------------------------------------------------------
// <copyright file="BoxGroupAttributeDrawer.cs" company="Sirenix ApS">
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

    /// <summary>
    /// Draws all properties grouped together with the <see cref="BoxGroupAttribute"/>
    /// </summary>
    /// <seealso cref="BoxGroupAttribute"/>
    public class BoxGroupAttributeDrawer : OdinGroupDrawer<BoxGroupAttribute>
    {
        private ValueResolver<string> labelGetter;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            this.labelGetter = ValueResolver.GetForString(this.Property, this.Attribute.LabelText ?? this.Attribute.GroupName);
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            this.labelGetter.DrawError();

            string headerLabel = null;
            if (this.Attribute.ShowLabel)
            {
                headerLabel = this.labelGetter.GetValue();
                if (string.IsNullOrEmpty(headerLabel))
                {
                    headerLabel = "Null"; // The user has asked for a header. So he gets a header.
                }
            }

            SirenixEditorGUI.BeginBox(headerLabel, this.Attribute.CenterLabel);

            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                var child = this.Property.Children[i];
                child.Draw(child.Label);
            }

            SirenixEditorGUI.EndBox();
        }
    }
}
#endif