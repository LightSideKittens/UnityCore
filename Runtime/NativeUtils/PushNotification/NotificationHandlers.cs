using System;
using System.Collections.Generic;
using Firebase.Messaging;
using LSCore;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class NotificationHandlers
{
    private static Dictionary<string, Action<JToken>> notificationActions = new();
    public static string DeviceToken { get; private set; }
    
    public static void Init()
    {
        InitToken();
        NativeNotification.RequestPermission();
        World.ApplicationResumed += Handle;
        Handle();

        async void InitToken()
        {
            DeviceToken = await FirebaseMessaging.GetTokenAsync();
            Burger.Log($"Push-notification Token: {DeviceToken}");
        }
    }

    public static void AddHandler(string actionId, Action<JToken> action)
    {
        notificationActions[actionId] = action;
    }

    private static void Handle()
    {
        string jsonData = NativeNotification.GetJsonData();
        Debug.Log($"[NotificationHandlerSystem] Handle {jsonData}");
        
        if (!string.IsNullOrEmpty(jsonData))
        {
            NativeNotification.CloseNotification();
            JObject json = JObject.Parse(jsonData);
            
            string actionId = json["actionId"]!.ToString();
            notificationActions[actionId](json);
        }
    }
}