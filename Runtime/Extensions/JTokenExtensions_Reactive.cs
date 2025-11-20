using System;
using Newtonsoft.Json.Linq;

namespace LSCore.Extensions
{
    public static partial class JTokenExtensions
    {
        public static JTokenListener Listen(this JToken token) => JTokenListener.Get(token);
        
        public static JTokenListener ListenAndCall(this JToken token, string path, Action callback)
        {
            var listener = Get(token, path);
            listener.ListenAndCall(callback);
            return listener;
        }

        public static JTokenListener UnListen(this JToken token, string path, Action callback)
        {
            var listener = Get(token, path);
            listener.UnListen(callback);
            return listener;
        }

        private static JTokenListener Get(this JToken token, string path)
        {
            var listener = JTokenListener.Get(token);
            foreach (var key in path.Split('.'))
            {
                listener = listener[key];
            }
            return listener;
        }
    }
}