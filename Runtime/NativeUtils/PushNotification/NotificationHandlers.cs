using System;
using System.Collections.Generic;
using LSCore.Async;
using Newtonsoft.Json.Linq;
using OneSignalSDK;
using OneSignalSDK.Notifications;
using UnityEngine;

public static class NotificationHandlers
{
    private static Dictionary<string, Action<JToken>> notificationActions = new();
    public static string Id { get; private set; }
    public static string DeviceToken { get; private set; }
    
    public async static void Init()
    {
        OneSignal.Initialize("0790afb5-506d-4eba-9f27-a7e9db329238");
        await OneSignal.Notifications.RequestPermissionAsync(true);
        InitToken();
        OneSignal.Notifications.Clicked += OnClicked;

        void InitToken()
        {
            Wait.While(() => string.IsNullOrEmpty(OneSignal.User.PushSubscription.Id), () =>
            {
                Id = OneSignal.User.PushSubscription.Id;
                DeviceToken  = OneSignal.User.PushSubscription.Token;
                Burger.Log($"Push-notification Id: {Id}");
                Burger.Log($"Push-notification Token: {DeviceToken}");
            });
        }
    }

    public static void AddHandler(string actionId, Action<JToken> action)
    {
        notificationActions[actionId] = action;
    }

    private static void OnClicked(object sender, NotificationClickEventArgs args)
    {
        var data = args.Notification.AdditionalData;
        Handle(JToken.FromObject(data));
    }
    
    private static void Handle(JToken data)
    {
        Debug.Log(data.ToString());
    }
}