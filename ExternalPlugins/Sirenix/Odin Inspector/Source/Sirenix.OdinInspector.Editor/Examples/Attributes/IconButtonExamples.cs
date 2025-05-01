//-----------------------------------------------------------------------
// <copyright file="IconButtonExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(ButtonAttribute))]
    internal class IconButtonExamples
    {
        [Button(SdfIconType.Dice1Fill, IconAlignment.LeftOfText)]
        private void IconButtonLeftOfText() {}

        [Button(SdfIconType.Dice2Fill, IconAlignment.RightOfText)]
        private void IconButtonRightOfText() {}

        [Button(SdfIconType.Dice3Fill, IconAlignment.LeftEdge)]
        private void IconButtonLeftEdge() {}

        [Button(SdfIconType.Dice4Fill, IconAlignment.RightEdge)]
        private void IconButtonRightEdge() {}

        [Button(SdfIconType.Dice5Fill, IconAlignment.RightEdge, Stretch = false)]
        private void DontStretch() {}

        [Button(SdfIconType.Dice5Fill, IconAlignment.RightEdge, Stretch = false, ButtonAlignment = 1f)]
        private void DontStretchAndAlign() {}
    }
}
#endif