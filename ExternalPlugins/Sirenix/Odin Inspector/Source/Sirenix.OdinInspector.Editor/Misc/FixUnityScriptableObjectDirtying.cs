//-----------------------------------------------------------------------
// <copyright file="FixUnityScriptableObjectDirtying.cs" company="Sirenix ApS">
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
    using UnityEngine;

    /// <summary>
    /// This class fixes a bug where Unity's Undo.RecordObject does not mark ScriptableObjects dirty when
    /// a change is recorded for them. It does this by subscribing to the Undo.postprocessModifications
    /// event, and marking all modified ScriptableObjects dirty manually.
    /// </summary>
    [InitializeOnLoad]
    internal static class FixUnityScriptableObjectDirtying
    {
        static FixUnityScriptableObjectDirtying()
        {
            Undo.postprocessModifications += (UndoPropertyModification[] mods) =>
            {
                try
                {
                    for (int i = 0; i < mods.Length; i++)
                    {
                        var mod = mods[i];

                        if (mod.currentValue.target is ScriptableObject)
                        {
                            EditorUtility.SetDirty(mod.currentValue.target);
                        }
                    }
                }
                catch { } // We want to be sure not to mess up anything, here

                return mods;
            };
        }
    }
}
#endif