//-----------------------------------------------------------------------
// <copyright file="HorizontalGroupAttributeExamples.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities.Editor;
    using System;
    using UnityEngine;

    [AttributeExample(typeof(HorizontalGroupAttribute))]
    internal class HorizontalGroupAttributeExamples
    {
        // The width can either be specified as percentage or pixels.
        // All values between 0 and 1 will be treated as a percentage.
        // If the width is 0 the column will be automatically sized.

        // Auto width
        [HorizontalGroup]
        public SomeFieldType Left1;

        [HorizontalGroup]
        public SomeFieldType Right1;

        // Margins
        [HorizontalGroup("row2", MarginRight = 0.4f)]
        public SomeFieldType Left2;

        [HorizontalGroup("row2")]
        public SomeFieldType Right2;


        // Custom width:
        [HorizontalGroup("row1", Width = 0.25f)] // 25 %
        public SomeFieldType Left3;

        [HorizontalGroup("row1", Width = 150)] // 150 px
        public SomeFieldType Center3;

        [HorizontalGroup("row1")] // Auto / expand
        public SomeFieldType Right3;


        // Gap Size
        [HorizontalGroup("row3", Gap = 3)]
        public SomeFieldType Left4;

        [HorizontalGroup("row3")]
        public SomeFieldType Center4;

        [HorizontalGroup("row3")] // Auto / expand
        public SomeFieldType Right4;


        // Title
        [HorizontalGroup("row4", Title = "Horizontal Group Title")]
        public SomeFieldType Left5;

        [HorizontalGroup("row4")]
        public SomeFieldType Center5;

        [HorizontalGroup("row4")]
        public SomeFieldType Right5;

        [HideLabel, Serializable]
        public struct SomeFieldType
        {
            [LabelText("@$property.Parent.NiceName")]
            [ListDrawerSettings(ShowIndexLabels = true)]
            public float[] x;
        }
    }
}
#endif