using System;
using System.Reflection;
using HarmonyLib;
using UnityEditor;

public static partial class Patchers
{
    public static partial class AnimEditor
    {
        [HarmonyPatch]
        public static class OnSelectionChanged
        {
            public static event Action Called;

            static MethodBase TargetMethod()
            {
                var animEditorType = typeof(Editor).Assembly.GetType("UnityEditor.AnimEditor");
                var methodInfo = animEditorType.GetMethod("OnSelectionChanged", BindingFlags.Instance | BindingFlags.Public);
                return methodInfo;
            }

            static void Postfix()
            {
                Called?.Invoke();
            }
        }
        
        [HarmonyPatch]
        public static class previewing
        {
            public static event Action<bool> Called;

            static MethodBase TargetMethod()
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditorInternal.AnimationWindowState");
                return type.GetProperty("previewing").GetSetMethod(true);
            }

            static void Postfix(bool value)
            {
                Called?.Invoke(value);
            }
        }
    }
}