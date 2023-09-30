using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace LSCore
{
    public static class Network
    {
        private static Action connected;
        private static bool isCompleted;

        public static string Country { get; private set; } = "World";
        private static Dictionary<string, object> ip;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
#if DEBUG
            Country = DebugData.Country ?? Country;
            OnComplete(null);
            return;
            #endif
            GetCountry();
        }

        private static void GetCountry()
        {
            var www = UnityWebRequest.Get("http://ip-api.com/json");
            var request = www.SendWebRequest();
            
            request.completed += _ =>
            {
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Burger.Error($"[{nameof(Network)}] Error while receiving: {www.downloadHandler.text}" + www.error);
                }
                else
                {
                    ip = JsonConvert.DeserializeObject<Dictionary<string, object>>(www.downloadHandler.text);
                    Country = (string)ip["country"];

                    Burger.Log($"[{nameof(Network)}] Country: " + Country);
                }
            };
            
            request.completed += OnComplete;
        }

        public static void OnConnected(Action callback)
        {
            if (isCompleted)
            {
                callback();
                return;
            }

            connected += Callback;

            void Callback()
            {
                connected -= Callback;
                callback();
            }
        }
        
        private static void OnComplete(AsyncOperation _)
        {
            if(isCompleted) return;

            isCompleted = true;
            
            connected?.Invoke();
            connected = null;
        }
    }
}