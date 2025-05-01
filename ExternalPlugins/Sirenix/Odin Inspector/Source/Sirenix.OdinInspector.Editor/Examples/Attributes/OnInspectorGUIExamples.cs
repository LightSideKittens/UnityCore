//-----------------------------------------------------------------------
// <copyright file="OnInspectorGUIExamples.cs" company="Sirenix ApS">
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

    [AttributeExample(typeof(OnInspectorGUIAttribute))]
    internal class OnInspectorGUIExamples
    {
        [OnInspectorInit("@Texture = Sirenix.Utilities.Editor.EditorIcons.OdinInspectorLogo")]
        [OnInspectorGUI("DrawPreview", append: true)]
        public Texture2D Texture;

        private void DrawPreview()
        {
            if (this.Texture == null) return;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(this.Texture);
            GUILayout.EndVertical();
        }

#if UNITY_EDITOR // Editor-related code must be excluded from builds
        [OnInspectorGUI]
        private void OnInspectorGUI()
        {
            UnityEditor.EditorGUILayout.HelpBox("OnInspectorGUI can also be used on both methods and properties", UnityEditor.MessageType.Info);
        }
#endif
    }
}
#endif