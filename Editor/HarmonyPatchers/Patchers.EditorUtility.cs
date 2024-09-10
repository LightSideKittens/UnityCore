using System;
using HarmonyLib;
using UnityEditor;
using Object = UnityEngine.Object;

public static partial class Patchers
{
    public static class _EditorUtility
    {
        [HarmonyPatch(typeof(EditorUtility), nameof(EditorUtility.SetDirty))]
        public static class SetDirty
        {
            public static event Action<Object> Called;

            private static void Postfix(Object target) => Called?.Invoke(target);
        }
    }
}

public static partial class Patchers
{
    public static class _SerializedObject
    {
        [HarmonyPatch(typeof(SerializedObject), nameof(SerializedObject.ApplyModifiedProperties))]
        public static class ApplyModifiedProperties
        {
            public static event Action<SerializedObject, bool> Called;

            static void Postfix(UnityEditor.SerializedObject __instance, bool __result)
            {
                Called?.Invoke(__instance, __result);
            }
        }
    }
}