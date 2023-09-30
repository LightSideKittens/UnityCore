#if !UNITY_EDITOR
#define RUNTIME
#endif

using System;
using System.Diagnostics;
using UnityEngine;

namespace LSCore.Runtime
{
    public static partial class Clipboard
    {
        static Clipboard()
        {
            Android_Init();
        }

        [Conditional("RUNTIME")]
        static partial void Android_Init();
        [Conditional("RUNTIME")]
        static partial void Android_Copy(string text);
        [Conditional("RUNTIME")]
        static partial void Android_Paste(Action<string> action);
        [Conditional("RUNTIME")]
        static partial void IOS_Copy(string text);
        [Conditional("RUNTIME")]
        static partial void IOS_Paste(Action<string> action);

        [Conditional("UNITY_EDITOR")]
        private static void Editor_Copy(string text)
        {
            GUIUtility.systemCopyBuffer = text;
        }

        [Conditional("UNITY_EDITOR")]
        private static void Editor_Paste(Action<string> action)
        {
            action(GUIUtility.systemCopyBuffer);
        }

        public static void Copy(string text)
        {
            Android_Copy(text);
            IOS_Copy(text);
            Editor_Copy(text);
        }
        
        public static void Paste(Action<string> action)
        {
            Android_Paste(action);
            IOS_Paste(action);
            Editor_Paste(action);
        }
    }
}