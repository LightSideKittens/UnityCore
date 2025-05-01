//-----------------------------------------------------------------------
// <copyright file="DisableInInlineEditorExample.cs" company="Sirenix ApS">
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

    using Sirenix.OdinInspector.Editor.Examples.Internal;
    using UnityEngine;

    [AttributeExample(typeof(DisableInInlineEditorsAttribute))]
    [ExampleAsComponentData(Namespaces = new string[] { "Sirenix.OdinInspector.Editor.Examples" })]
    internal class DisableInInlineEditorExample
    {
#if UNITY_EDITOR // DisabledInInlineEditorScriptableObject is an example type and only exists in the editor
        [InfoBox("Click the pen icon to open a new inspector window for the InlineObject too see the difference this attribute makes.")]
        [InlineEditor(Expanded = true)]
        public DisabledInInlineEditorScriptableObject InlineObject;
#endif 

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorInit]
        private void CreateData()
        {
            InlineObject = ExampleHelper.GetScriptableObject<DisabledInInlineEditorScriptableObject>("Inline Object");
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