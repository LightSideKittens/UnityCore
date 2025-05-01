//-----------------------------------------------------------------------
// <copyright file="PropertyRangeExample.cs" company="Sirenix ApS">
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

    using UnityEngine;

    [AttributeExample(typeof(RangeAttribute))]
    [AttributeExample(typeof(PropertyRangeAttribute))]
    internal class PropertyRangeExample
    {
        [Range(0, 10)]
        public int Field = 2;

        [InfoBox("Odin's PropertyRange attribute is similar to Unity's Range attribute, but also works on properties.")]
        [ShowInInspector, PropertyRange(0, 10)]
        public int Property { get; set; }

        [InfoBox("You can also reference member for either or both min and max values.")]
        [PropertyRange(0, "Max"), PropertyOrder(3)]
        public int Dynamic = 6;

        [PropertyOrder(4)]
        public int Max = 100;
    }
}
#endif