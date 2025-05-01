//-----------------------------------------------------------------------
// <copyright file="VerticalGroupAttributeDrawer.cs" company="Sirenix ApS">
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

    /// <summary>
    /// Drawer for the <see cref="VerticalGroupAttribute"/>
    /// </summary>
    /// <seealso cref="VerticalGroupAttribute"/>

    public class VerticalGroupAttributeDrawer : OdinGroupDrawer<VerticalGroupAttribute>
    {
        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var attribute = this.Attribute;

            GUILayout.BeginVertical();

            if (attribute.PaddingTop != 0)
            {
                GUILayout.Space(attribute.PaddingTop);
            }

            for (int i = 0; i < property.Children.Count; i++)
            {
                var child = property.Children[i];
                child.Draw(child.Label);
            }

            if (attribute.PaddingBottom != 0)
            {
                GUILayout.Space(attribute.PaddingBottom);
            }

            GUILayout.EndVertical();
        }
    }
}
#endif