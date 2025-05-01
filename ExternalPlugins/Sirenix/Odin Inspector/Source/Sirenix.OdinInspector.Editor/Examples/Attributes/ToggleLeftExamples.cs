//-----------------------------------------------------------------------
// <copyright file="ToggleLeftExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(ToggleLeftAttribute))]
    internal class ToggleLeftExamples
    {
        [InfoBox("Draws the toggle button before the label for a bool property.")]
        [ToggleLeft]
        public bool LeftToggled;

        [EnableIf("LeftToggled")]
        public int A;

        [EnableIf("LeftToggled")]
        public bool B;

        [EnableIf("LeftToggled")]
        public bool C;
    }
}
#endif