#if UNITY_ANDROID

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
        private static AndroidJavaObject androidClipboardManager;
        private static AndroidJavaClass toastClass;
        private static AndroidJavaObject currentActivity;

        [Conditional("RUNTIME")]
        static partial void Android_Init()
        {
            var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            var staticContext = new AndroidJavaClass("android.content.Context");
            var service = staticContext.GetStatic<AndroidJavaObject>("CLIPBOARD_SERVICE"); 
            androidClipboardManager = activity.Call<AndroidJavaObject>("getSystemService", service);
            toastClass = new AndroidJavaClass("android.widget.Toast");
            currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }

        [Conditional("RUNTIME")]
        static partial void Android_Copy(string text)
        {
            androidClipboardManager.Call("setText", text);
            Toast();
        }
        
        [Conditional("RUNTIME")]
        static partial void Android_Paste(Action<string> action)
        {
            action(androidClipboardManager.Call<string>("getText"));
        }
        
        private static void Toast()
        {
            currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
            {
                AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", currentActivity, "Copy!", 0);
                toast.Call("show");
            }));
        }
    }
}
#endif