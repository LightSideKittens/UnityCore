using System;
using LSCore;
using Sendbird.Chat;
using UnityEngine;

public static partial class BlaBla
{
    private static string tag = "[BlaBla]".ToTag(new Color(0.4f, 0.43f, 1f));
    public static event Action<object> ErrorGot;
    [ResetStatic] private static bool inited;
    
    public static void Init()
    {
        if (inited) return;
        inited = true;
        SbInitParams initParams = new SbInitParams(BlaBlaSettings.AppId, BlaBlaSettings.LogLevel, Application.version);
        SendbirdChat.Init(initParams);
        Events.Setup();
    }
    
    public static void Connect(Action<SbUser> callback)
    {
        Init();
        SendbirdChat.Connect(World.UserId, (user, error) =>
        {
            if(HandleError(error)) return;
            callback(user);
        });
    }

    
    internal static bool HandleError(SbError error)
    {
        if (error != null)
        {
            Burger.Error($"{tag} {error.ErrorCode}\n{error.ErrorMessage}");
            ErrorGot?.Invoke(error);
            return true;
        }
        
        return false;
    }
    
    internal static bool HandleError(AggregateException error)
    {
        if (error != null)
        {
            Burger.Error($"{tag} {error.Message}");
            ErrorGot?.Invoke(error);
            return true;
        }
        
        return false;
    }
}