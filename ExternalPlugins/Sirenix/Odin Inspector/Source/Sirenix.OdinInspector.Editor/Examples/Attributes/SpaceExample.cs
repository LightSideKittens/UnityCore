//-----------------------------------------------------------------------
// <copyright file="SpaceExample.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(SpaceAttribute))]
    [AttributeExample(typeof(PropertySpaceAttribute))]
    internal class SpaceExample
    {
        // PropertySpace and Space attributes are virtually identical.
        [Space]
        [BoxGroup("Space", ShowLabel = false)]
        public int Space;

        // You can also control spacing both before and after the PropertySpace attribute.
        [PropertySpace(SpaceBefore = 30, SpaceAfter = 60)]
        [BoxGroup("BeforeAndAfter", ShowLabel = false)]
        public int BeforeAndAfter;

        // The PropertySpace attribute can, as the name suggests, also be applied to properties.
        [PropertySpace]
        [ShowInInspector, BoxGroup("Property", ShowLabel = false)]
        public string Property { get; set; }
    }
}
#endif