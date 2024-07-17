using System;
using HarmonyLib;
using UnityEditor;
using Object = UnityEngine.Object;

[HarmonyPatch(typeof(EditorUtility), nameof(EditorUtility.SetDirty))]
public static class LSEditorUtility
{
    public static event Action<Object> DirtySet;

    private static void Postfix(Object target) => DirtySet?.Invoke(target);
}