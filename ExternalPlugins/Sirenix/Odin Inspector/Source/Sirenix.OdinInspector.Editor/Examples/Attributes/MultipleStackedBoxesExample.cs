//-----------------------------------------------------------------------
// <copyright file="MultipleStackedBoxesExample.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(BoxGroupAttribute), Order = 10)]
    [AttributeExample(typeof(HorizontalGroupAttribute), Order = 10)]
    [AttributeExample(typeof(TitleGroupAttribute), Order = 10)]
    [AttributeExample(typeof(VerticalGroupAttribute), Order = 10)]
    internal class MultipleStackedBoxesExample
    {
        [TitleGroup("Multiple Stacked Boxes")]
        [HorizontalGroup("Multiple Stacked Boxes/Split")]
        [VerticalGroup("Multiple Stacked Boxes/Split/Left")]
        [BoxGroup("Multiple Stacked Boxes/Split/Left/Box A")]
        public int BoxA;

        [BoxGroup("Multiple Stacked Boxes/Split/Left/Box B")]
        public int BoxB;

        [VerticalGroup("Multiple Stacked Boxes/Split/Right")]
        [BoxGroup("Multiple Stacked Boxes/Split/Right/Box C")]
        public int BoxC, BoxD, BoxE;
    }
}
#endif