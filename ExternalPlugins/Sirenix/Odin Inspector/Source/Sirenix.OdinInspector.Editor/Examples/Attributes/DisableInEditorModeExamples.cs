//-----------------------------------------------------------------------
// <copyright file="DisableInEditorModeExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(DisableInEditorModeAttribute))]
    internal class DisableInEditorModeExamples
    {
        [Title("Disabled in edit mode")]
        [DisableInEditorMode]
        public GameObject A;

        [DisableInEditorMode]
        public Material B;
    }
}
#endif