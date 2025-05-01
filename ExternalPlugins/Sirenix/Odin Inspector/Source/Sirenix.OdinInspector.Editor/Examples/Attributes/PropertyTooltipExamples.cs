//-----------------------------------------------------------------------
// <copyright file="PropertyTooltipExamples.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(PropertyTooltipAttribute),
        "PropertyTooltip is used to add tooltips to properties in the inspector.\n\nPropertyTooltip can also be applied to properties and methods, unlike Unity's Tooltip attribute.")]
    internal class PropertyTooltipExamples
    {
        [PropertyTooltip("This is tooltip on an int property.")]
        public int MyInt;

        [InfoBox("Use $ to refer to a member string.")]
        [PropertyTooltip("$Tooltip")]
        public string Tooltip = "Dynamic tooltip.";

        [Button, PropertyTooltip("Button Tooltip")]
        private void ButtonWithTooltip()
        {
            // ...
        }
    }
}
#endif