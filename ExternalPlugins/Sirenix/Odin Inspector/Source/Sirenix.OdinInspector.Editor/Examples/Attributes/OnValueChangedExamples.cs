//-----------------------------------------------------------------------
// <copyright file="OnValueChangedExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(OnValueChangedAttribute), "OnValueChanged is used here to create a material for a shader, when the shader is changed.")]
    internal class OnValueChangedExamples
    {
        [OnValueChanged("CreateMaterial")]
        public Shader Shader;

        [ReadOnly, InlineEditor(InlineEditorModes.LargePreview)]
        public Material Material;

        private void CreateMaterial()
        {
            if (this.Material != null)
            {
                Material.DestroyImmediate(this.Material);
            }

            if (this.Shader != null)
            {
                this.Material = new Material(this.Shader);
            }
        }
    }
}
#endif