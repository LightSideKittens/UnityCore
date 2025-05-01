//-----------------------------------------------------------------------
// <copyright file="HideLabelExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(HideLabelAttribute))]
    internal class HideLabelExamples
    {
        [Title("Wide Colors")]
        [HideLabel]
        [ColorPalette("Fall")]
        public Color WideColor1;

        [HideLabel]
        [ColorPalette("Fall")]
        public Color WideColor2;

        [Title("Wide Vector")]
        [HideLabel]
        public Vector3 WideVector1;

        [HideLabel]
        public Vector4 WideVector2;

        [Title("Wide String")]
        [HideLabel]
        public string WideString;

        [Title("Wide Multiline Text Field")]
        [HideLabel]
        [MultiLineProperty]
        public string WideMultilineTextField = "";
    }
}
#endif