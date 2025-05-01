//-----------------------------------------------------------------------
// <copyright file="ShowAndHideInInlineEditorExample.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
#pragma warning disable
namespace Sirenix.OdinInspector.Editor.Examples
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using UnityEngine;

    [AttributeExample(typeof(ShowInInlineEditorsAttribute))]
    [AttributeExample(typeof(HideInInlineEditorsAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class ShowAndHideInInlineEditorExample
    {
#if UNITY_EDITOR // MyInlineScriptableObject is an example type and only exists in the editor
        [InfoBox("Click the pen icon to open a new inspector window for the InlineObject too see the differences these attributes make.")]
        [InlineEditor(Expanded = true)]
        public MyInlineScriptableObject InlineObject;
#endif

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            InlineObject = ExampleHelper.GetScriptableObject<MyInlineScriptableObject>("Inline Object");
        }

        [OnInspectorDispose]
        private void CleanupData()
        {
            if (InlineObject != null) Object.DestroyImmediate(InlineObject);
        }
#endif
    }
}
#endif