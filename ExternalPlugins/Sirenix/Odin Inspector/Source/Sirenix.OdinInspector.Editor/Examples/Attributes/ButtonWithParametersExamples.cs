//-----------------------------------------------------------------------
// <copyright file="ButtonWithParametersExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(ButtonAttribute),
        Name = "Parameters Examples",
        Description = "You can also use the Button attribute on any method with parameters.\n" +
        "This will draw a form in the inspector that lets you fill out the parameters that gets passed to the method when the button is pressed.")]
    internal class ButtonWithParametersExamples
    {
        [Button]
        private void Default(float a, float b, GameObject c)
        {
        }

        [Button]
        private void Default(float t, float b, float[] c)
        {
        }

        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        private int FoldoutButton(int a = 2, int b = 2)
        {
            return a + b;
        }

        [Button(ButtonSizes.Medium, ButtonStyle.FoldoutButton)]
        private void FoldoutButton(int a, int b, ref int result)
        {
            result = a + b;
        }

        [Button(ButtonStyle.Box)]
        private void Full(float a, float b, out float c)
        {
            c = a + b;
        }

        [Button(ButtonSizes.Large, ButtonStyle.Box)]
        private void Full(int a, float b, out float c)
        {
            c = a + b;
        }

        [Button(ButtonStyle.CompactBox, Expanded = true)]
        private void CompactExpanded(float a, float b, GameObject c)
        {
        }

        [Button(ButtonSizes.Medium, ButtonStyle.Box, Expanded = true)]
        private void FullExpanded(float a, float b)
        {
        }
    }
}
#endif