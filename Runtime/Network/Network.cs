using System;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LSCore
{
    public static class Network
    {
        private static Action connected;
        private static bool isCompleted;

        public static string Country { get; private set; } = "World";
        private static JToken ip;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
#if DEBUG
            Country = LSDebugData.Country ?? Country;
            OnComplete(null);
            return;
#endif
            GetCountry();
        }

        private static void GetCountry()
        {
            var www = UnityWebRequest.Get("https://geoip.maxmind.com/geoip/v2.1/city/me");
            www.SetRequestHeader("Origin", "https://www.maxmind.com");
            www.SetRequestHeader("Referer", "https://www.maxmind.com/");
            
            var request = www.SendWebRequest();
            
            request.completed += _ =>
            {
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Burger.Error($"[{nameof(Network)}] Error while receiving: {www.downloadHandler.text}" + www.error);
                }
                else
                {
                    ip = JToken.Parse(www.downloadHandler.text);
                    Country = (string)ip["country"]["names"]["en"];

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