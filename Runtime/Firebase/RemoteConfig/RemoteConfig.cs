using System;
using System.Collections.Generic;
using Core.Server;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;

namespace LSCore.Firebase
{
    public partial class RemoteConfig
    {
        private static int FetchIntervalMinutes
        {
            get
            {
#if DEBUG
                return 0;
#endif
                return 30;
            }
        }
        
        private static readonly Dictionary<string, object> defaults = new();
        private static FirebaseRemoteConfig remoteConfig;
        private static readonly ConfigSettings fetchSettings = new()
        {
            MinimumFetchInternalInMilliseconds = (ulong)TimeSpan.FromMinutes(FetchIntervalMinutes).Milliseconds
        };

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void StaticConstructor()
        {
            remoteConfig = FirebaseRemoteConfig.DefaultInstance;
        }

        private RemoteConfig(string name, object value)
        {
            defaults.Clear();
            defaults.Add(name, value);
        }

        public static RemoteConfig CreateDefault(string name, object value) => new(name, value);

        public RemoteConfig AddDefault(string name, object value)
        {
            defaults.Add(name, value);
            return this;
        }

        public static void FetchAndActivate(Action onComplete, Action onError = null)
        {
            remoteConfig.SetConfigSettingsAsync(fetchSettings).ContinueWithOnMainThread(_ =>
            {
                remoteConfig.FetchAndActivateAsync().ContinueWithOnMainThread(task =>
                {
                    if (!task.IsCompleted)
                    {
                        Burger.Error($"[{nameof(RemoteConfig)}] Retrieval hasn't finished.");
                        onError.SafeInvoke();
                        return;
                    }

                    var info = remoteConfig.Info;
                    if(info.LastFetchStatus != LastFetchStatus.Success) 
                    {
                        Burger.Error($"[{nameof(RemoteConfig)}] was unsuccessful\n{nameof(info.LastFetchStatus)}: {info.LastFetchStatus}");
                        onError.SafeInvoke();
                        return;
                    }

                    onComplete.SafeInvoke();
                });
            });
        }

        public void Activate(Action onComplete, Action onError = null)
        {
            remoteConfig.SetDefaultsAsync(defaults).ContinueWithOnMainThread(_ =>
            {
                defaults.Clear();
                FetchAndActivate(onComplete, onError);
            });
        }
    }
}