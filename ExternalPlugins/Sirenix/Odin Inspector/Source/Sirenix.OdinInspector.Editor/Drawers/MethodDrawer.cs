//-----------------------------------------------------------------------
// <copyright file="MethodDrawer.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    /// <summary>
    /// Base drawer to inherit from to draw methods.
    /// </summary>
    public abstract class MethodDrawer : OdinDrawer
    {
        public sealed override bool CanDrawProperty(InspectorProperty property)
        {
            return property.Info.PropertyType == PropertyType.Method && this.CanDrawMethodProperty(property);
        }

        protected virtual bool CanDrawMethodProperty(InspectorProperty property)
        {
            return true;
        }
    }
}
#endif