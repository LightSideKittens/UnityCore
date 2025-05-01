//-----------------------------------------------------------------------
// <copyright file="ButtonsInBoxesExample.cs" company="Sirenix ApS">
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
    [AttributeExample(typeof(FoldoutGroupAttribute), Order = 10)]
    [AttributeExample(typeof(HorizontalGroupAttribute), Order = 10)]
    internal class ButtonsInBoxesExample
    {
        [Button(ButtonSizes.Large)]
        [FoldoutGroup("Buttons in Boxes")]
        [HorizontalGroup("Buttons in Boxes/Horizontal")]
        [BoxGroup("Buttons in Boxes/Horizontal/One")]
        public void Button1() { }

        [Button(ButtonSizes.Large)]
        [BoxGroup("Buttons in Boxes/Horizontal/Two")]
        public void Button2() { }

        [Button]
        [HorizontalGroup("Buttons in Boxes/Horizontal", Width = 60)]
        [BoxGroup("Buttons in Boxes/Horizontal/Double")]
        public void Accept() { }

        [Button]
        [BoxGroup("Buttons in Boxes/Horizontal/Double")]
        public void Cancel() { }
    }
}
#endif