//-----------------------------------------------------------------------
// <copyright file="ButtonGroupExamples.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    [AttributeExample(typeof(ButtonGroupAttribute))]
    [AttributeExample(typeof(ButtonAttribute), Order = 10)]
    internal class ButtonGroupExamples
    {
        public IconButtonGroupExamples iconButtonGroupExamples;

        [ButtonGroup]
        private void A()
        {
        }

        [ButtonGroup]
        private void B()
        {
        }

        [ButtonGroup]
        private void C()
        {
        }

        [ButtonGroup]
        private void D()
        {
        }

        [Button(ButtonSizes.Large)]
        [ButtonGroup("My Button Group")]
        private void E()
        {
        }

        [GUIColor(0, 1, 0)]
        [ButtonGroup("My Button Group")]
        private void F()
        {
        }

        [Serializable, HideLabel]
        public struct IconButtonGroupExamples
        {
            [ButtonGroup(ButtonHeight = 25), Button(SdfIconType.ArrowsMove, "")]
            void ArrowsMove() { }

            [ButtonGroup, Button(SdfIconType.Crop, "")]
            void Crop() { }

            [ButtonGroup, Button(SdfIconType.TextLeft, "")]
            void TextLeft() { }

            [ButtonGroup, Button(SdfIconType.TextRight, "")]
            void TextRight() { }

            [ButtonGroup, Button(SdfIconType.TextParagraph, "")]
            void TextParagraph() { }

            [ButtonGroup, Button(SdfIconType.Textarea, "")]
            void Textarea() { }
        }
    }
}
#endif