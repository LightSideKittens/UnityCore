//-----------------------------------------------------------------------
// <copyright file="SpaceAttributeDrawer.cs" company="Sirenix ApS">
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

    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Draws properties marked with <see cref="SpaceAttribute"/>.
    /// </summary>
    /// <seealso cref="SpaceAttribute"/>
    [DrawerPriority(2, 0, 0)]
    public sealed class SpaceAttributeDrawer : OdinAttributeDrawer<SpaceAttribute>
    {
        private bool drawSpace;

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        protected override void Initialize()
        {
            if (this.Property.Parent == null)
            {
                this.drawSpace = true;
            }
            else if (this.Property.Parent.ChildResolver is ICollectionResolver)
            {
                this.drawSpace = false;
            }
            else
            {
                this.drawSpace = true;
            }
        }

        /// <summary>
        /// Draws the property.
        /// </summary>
        protected override void DrawPropertyLayout(GUIContent label)
        {
            if (this.drawSpace)
            {
                var attribute = this.Attribute;

                if (attribute.height == 0)
                {
                    EditorGUILayout.Space();
                }
                else
                {
                    GUILayout.Space(attribute.height);
                }
            }

            this.CallNextDrawer(label);
        }
    }
}
#endif