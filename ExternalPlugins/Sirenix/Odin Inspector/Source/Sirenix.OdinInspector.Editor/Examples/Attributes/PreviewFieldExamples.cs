//-----------------------------------------------------------------------
// <copyright file="PreviewFieldExamples.cs" company="Sirenix ApS">
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

namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using UnityEngine;

    [AttributeExample(typeof(PreviewFieldAttribute))]
	[ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class PreviewFieldExamples
    {
        [PreviewField]
        public Object RegularPreviewField;

        [VerticalGroup("row1/left")]
        public string A, B, C;

        [HideLabel]
        [PreviewField(50, ObjectFieldAlignment.Right)]
        [HorizontalGroup("row1", 50), VerticalGroup("row1/right")]
        public Object D;

        [HideLabel]
        [PreviewField(50, ObjectFieldAlignment.Left)]
        [HorizontalGroup("row2", 50), VerticalGroup("row2/left")]
        public Object E;

        [VerticalGroup("row2/right"), LabelWidth(-54)]
        public string F, G, H;

        [PreviewField("preview", FilterMode.Bilinear)]
        public Object I;

        private Texture preview;

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            RegularPreviewField = ExampleHelper.GetTexture();
            D = ExampleHelper.GetTexture();
            E = ExampleHelper.GetTexture();
            I = ExampleHelper.GetMesh();
            preview = ExampleHelper.GetTexture();
        }

        [InfoBox(
            "These object fields can also be selectively enabled and customized globally " +
            "from the Odin preferences window.\n\n" +
            " - Hold Ctrl + Click = Delete Instance\n" +
            " - Drag and drop = Move / Swap.\n" +
            " - Ctrl + Drag = Replace.\n" +
            " - Ctrl + drag and drop = Move and override.")]
        [PropertyOrder(-1)]
        [Button(ButtonSizes.Large)]
        private void ConfigureGlobalPreviewFieldSettings()
        {
            Sirenix.OdinInspector.Editor.GeneralDrawerConfig.Instance.OpenInEditor();
        }
#endif
    }
}
#endif