using System;
using System.Reflection;
using HarmonyLib;
using LSCore.Extensions;
using UnityEditor;
using UnityEngine;
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
    
    public static class _EditorGUI
    {
        [HarmonyPatch(typeof(EditorGUI), "BeginPropertyInternal")]
        public static class BeginPropertyInternal
        {
            public static event Action<Rect, GUIContent, SerializedProperty> Calling;

            static void Prefix(
                Rect totalPosition,
                GUIContent label,
                SerializedProperty property)
            {
                Calling?.Invoke(totalPosition, label, property);
            }
        }
        
        [HarmonyPatch(typeof(EditorGUI), nameof(EditorGUI.EndProperty))]
        public static class EndProperty
        {
            public static event Action Called;

            static void Postfix()
            {
                Called?.Invoke();
            }
        }
    }

    public static class _Transform
    {
        [HarmonyPatch(typeof(Transform), "SetLocalEulerAngles")]
        public static class SetLocalEulerAngles
        {
            public static event Action<Vector3> Called;

            static void Postfix(Vector3 euler, object order)
            {
                Called?.Invoke(euler);
            }
        }
        
        [HarmonyPatch]
        public static class TransformTool
        {
            static MethodBase TargetMethod()
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditor.TransformTool");
                return type.GetMethod("ToolGUI", BindingFlags.Instance | BindingFlags.NonPublic);
            }
            
            public static event Action<Transform> Calling;
            public static event Action<Transform, bool> Called;

            static void Prefix()
            {
                EditorGUI.BeginChangeCheck();
                foreach (Transform transform in Selection.transforms)
                {
                    Calling?.Invoke(transform);
                }
            }
            
            static void Postfix()
            {
                var isChanged = EditorGUI.EndChangeCheck();
                foreach (Transform transform in Selection.transforms)
                {
                    Called?.Invoke(transform, isChanged);
                }
            }
        }
    }
}