//-----------------------------------------------------------------------
// <copyright file="ShowPropertyResolverAttributeDrawer.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using UnityEngine;

    /// <summary>
    /// Drawer for the ShowPropertyResolver attribute.
    /// </summary>
    /// <seealso cref="ShowPropertyResolverAttribute" />
    [DrawerPriority(10000, 0, 0)]
    public class ShowPropertyResolverAttributeDrawer : OdinAttributeDrawer<ShowPropertyResolverAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var property = this.Property;
            var name = property.ChildResolver != null ? property.ChildResolver.GetType().GetNiceName() : "None";

            SirenixEditorGUI.BeginToolbarBox(name);
            this.CallNextDrawer(label);
            SirenixEditorGUI.EndToolbarBox();
        }
    }
}
#endif