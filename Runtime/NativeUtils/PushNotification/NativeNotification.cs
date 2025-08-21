using System;
using System.Threading;
using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public static class NativeNotification
{
    private static int mainThreadId;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        mainThreadId = Thread.CurrentThread.ManagedThreadId;
    }

    public static void RequestPermission()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (!Permission.HasUserAuthorizedPermission("android.permission.POST_NOTIFICATIONS"))
        {
            Permission.RequestUserPermission("android.permission.POST_NOTIFICATIONS");
        }
#endif
    }
    
    private const string EXTRA_NOTIFICATION_ID = "notification_id";

    public static void CloseNotification()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var intent = currentActivity.Call<AndroidJavaObject>("getIntent"))
                    {
                        int notificationId = intent.Call<int>("getIntExtra", EXTRA_NOTIFICATION_ID, -1);
                        Debug.Log("HandleLaunchFromNotification: notificationId=" + notificationId);

                        if (notificationId != -1)
                        {
                            CloseNotification(notificationId);
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in HandleLaunchFromNotification: {e.Message}\n{e.StackTrace}");
        }
#endif
    }
    
    private static void CloseNotification(int notificationId)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                using (var currentActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity"))
                {
                    using (var context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
                    {
                        using (var notificationManagerCompatClass = new AndroidJavaClass("androidx.core.app.NotificationManagerCompat"))
                        {
                            using (var notificationManager = notificationManagerCompatClass.CallStatic<AndroidJavaObject>("from", context))
                            {
                                notificationManager.Call("cancel", notificationId);
                            }
                        }
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"CloseNotificationAndShade error: {e.Message}\n{e.StackTrace}");
        }
#endif
    }
    
    public static void ClearJsonData()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                intent.Call("removeExtra", "json_data");
                activity.Call("setIntent", intent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error clearing JSON data from Intent: " + e);
        }
#endif
    }
    
    public static string GetJsonData()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject intent = activity.Call<AndroidJavaObject>("getIntent");
                string data = intent.Call<string>("getStringExtra", "json_data");
                return data;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to read jsonData from Intent: " + e);
            return null;
        }
#endif
        return null;
    }

    public static void Show(
        string title,
        string description,
        string jsonData,
        string bigImagePath,
        string smallImagePath,
        string closeButtonText,
        string playButtonText)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool needDetach = false;

        if (Thread.CurrentThread.ManagedThreadId != mainThreadId)
        {
            AndroidJNI.AttachCurrentThread();
            needDetach = true;
        }

        try
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaClass notificationHelper = new AndroidJavaClass("com.lscore.notification.NotificationHelper");
            AndroidJavaObject buttonList = new AndroidJavaObject("java.util.ArrayList");

            int dismissType = notificationHelper.GetStatic<int>("BUTTON_TYPE_DISMISS");
            int openAppType = notificationHelper.GetStatic<int>("BUTTON_TYPE_OPEN_APP");

            AndroidJavaObject dismissButton = new AndroidJavaObject("com.lscore.notification.NotificationButton", closeButtonText, dismissType);
            AndroidJavaObject playButton = new AndroidJavaObject("com.lscore.notification.NotificationButton", playButtonText, openAppType);

            buttonList.Call<bool>("add", dismissButton);
            buttonList.Call<bool>("add", playButton);

            int notificationId = (int)(DateTime.Now.Ticks & 0xFFFFFFFF);
            bool openAppOnNotificationClick = false;

            notificationHelper.CallStatic(
                "showNotification",
                currentActivity,
                notificationId,
                title,
                description,
                buttonList,
                jsonData,
                bigImagePath,
                smallImagePath,
                openAppOnNotificationClick
            );
        }
        catch (Exception ex)
        {
            Debug.LogError("NativeNotification.Show error: " + ex.Message);
        }
        finally
        {
            if (needDetach)
            {
                AndroidJNI.DetachCurrentThread();
            }
        }
#endif
    }
}
