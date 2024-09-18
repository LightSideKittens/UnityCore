using System;
using System.Reflection;
using HarmonyLib;
using UnityEditor;

public static partial class Patchers
{
    public static class AnimationWindowControl
    {
        [HarmonyPatch]
        public static class time
        {
            public static event Action<float> Called;

            static MethodBase TargetMethod()
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditorInternal.AnimationWindowControl");
                return type.GetProperty("time").GetSetMethod(false);
                
            }

            static void Postfix(float value)
            {
                Called?.Invoke(value);
            }
        }
        
        [HarmonyPatch]
        public static class PlaybackUpdate
        {
            public static event Action Called;

            static MethodBase TargetMethod()
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditorInternal.AnimationWindowControl");
                return type.GetMethod("PlaybackUpdate", BindingFlags.Instance | BindingFlags.Public);
                
            }

            static void Postfix()
            {
                Called?.Invoke();
            }
        }
    }
    
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