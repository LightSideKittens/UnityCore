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
            public static event Action Changed;

            static MethodBase TargetMethod()
            {
                var animEditorType = typeof(Editor).Assembly.GetType("UnityEditor.AnimEditor");
                var methodInfo = animEditorType.GetMethod("OnSelectionChanged", BindingFlags.Instance | BindingFlags.Public);
                return methodInfo;
            }

            static void Postfix()
            {
                Changed?.Invoke();
            }
        }
    }
}