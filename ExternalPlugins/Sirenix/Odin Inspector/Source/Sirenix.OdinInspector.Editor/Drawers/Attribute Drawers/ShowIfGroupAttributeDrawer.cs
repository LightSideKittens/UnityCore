//-----------------------------------------------------------------------
// <copyright file="ShowIfGroupAttributeDrawer.cs" company="Sirenix ApS">
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

    public class ShowIfGroupAttributeDrawer : OdinGroupDrawer<ShowIfGroupAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            for (int i = 0; i < this.Property.Children.Count; i++)
            {
                this.Property.Children[i].Draw();
            }
        }
    }
}
#endif