//-----------------------------------------------------------------------
// <copyright file="InlineButtonExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(InlineButtonAttribute))]
    internal class InlineButtonExamples
    {
        // Inline Buttons:
        [InlineButton("A")]
        public int InlineButton;

        [InlineButton("A")]
        [InlineButton("B", "Custom Button Name")]
        public int ChainedButtons;

        [InlineButton("C", SdfIconType.Dice6Fill, "Random")]
        public int IconButton;

        private void A()
        {
            Debug.Log("A");
        }

        private void B()
        {
            Debug.Log("B");
        }

        private void C()
        {
            Debug.Log("C");
        }
    }
}
#endif