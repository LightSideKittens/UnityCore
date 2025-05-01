//-----------------------------------------------------------------------
// <copyright file="LabelWidthExample.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(LabelWidthAttribute), "Change the width of the label for your property.")]
    internal class LabelWidthExample
    {
        public int DefaultWidth;

        [LabelWidth(50)]
        public int Thin;

        [LabelWidth(250)]
        public int Wide;
    }
}
#endif