//-----------------------------------------------------------------------
// <copyright file="OnInspectorInitAndDisposeMethodDrawer.cs" company="Sirenix ApS">
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

    public class OnInspectorInitAndDisposeMethodDrawer : MethodDrawer
    {
        protected override bool CanDrawMethodProperty(InspectorProperty property)
        {
            var attrs = property.Attributes;

            return attrs.HasAttribute<OnInspectorDisposeAttribute>()
                || attrs.HasAttribute<OnInspectorInitAttribute>();
        }

        protected override void DrawPropertyLayout(GUIContent label)
        {
            // Draw nothing
        }
    }
}
#endif