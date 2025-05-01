//-----------------------------------------------------------------------
// <copyright file="CustomEditorLoader.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using UnityEditor;
    using UnityEditor.Callbacks;

    internal static class CustomEditorLoader
    {
        [DidReloadScripts]
        private static void LoadCustomEditors()
        {
            InspectorConfig.Instance.UpdateOdinEditors();
        }
    }
}
#endif