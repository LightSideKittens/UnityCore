//-----------------------------------------------------------------------
// <copyright file="MinMaxValueValueExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(MinValueAttribute))]
    [AttributeExample(typeof(MaxValueAttribute))]
    internal class MinMaxValueValueExamples
    {
        // Ints
        [Title("Int")]
        [MinValue(0)]
        public int IntMinValue0;

        [MaxValue(0)]
        public int IntMaxValue0;

        // Floats
        [Title("Float")]
        [MinValue(0)]
        public float FloatMinValue0;

        [MaxValue(0)]
        public float FloatMaxValue0;

        // Vectors
        [Title("Vectors")]
        [MinValue(0)]
        public Vector3 Vector3MinValue0;

        [MaxValue(0)]
        public Vector3 Vector3MaxValue0;
    }
}
#endif