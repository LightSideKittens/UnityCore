using System;
using LSCore.Async;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace LSCore
{
    public static class Network
    {
        private static Action connected;
        private static bool isCompleted;
        private static LSTask<string> task = LSTask<string>.Create();
        private static JToken ip;
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
#if DEBUG
            task.Success(LSDebugData.Country);
            return;
#endif
            GetCountry();
        }

        public static LSTask<string> GetCountry()
        {
            if (task.IsSuccess)
            {
                return task;
            }
            
            
            var www = UnityWebRequest.Get("https://geoip.maxmind.com/geoip/v2.1/city/me");
            www.SetRequestHeader("Origin", "https://www.maxmind.com");
            www.SetRequestHeader("Referer", "https://www.maxmind.com/");
            
            var request = www.SendWebRequest();
            
            request.completed += _ =>
            {
                if (www.result != UnityWebRequest.Result.Success)
                {
                    var error = $"[{nameof(Network)}] Error while receiving: {www.downloadHandler.text} {www.error}";
                    task.Error(error);
                }
                else
                {
                    ip = JToken.Parse(www.downloadHandler.text);
                    var countryCode = (string)ip["country"]["iso_code"];
                    task.Success(countryCode);

                    Burger.Log($"[{nameof(Network)}] Country: {countryCode}");
                }
            };

            return task;
        }
    }
}