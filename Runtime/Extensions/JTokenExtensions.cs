using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LSCore.Extensions
{
    public static class JTokenExtensions
    {
        public static T As<T>(this JToken token, string key, T defaultValue = default)
        {
            if (token[key] != null)
            {
                return token[key].ToObject<T>();
            }
            
            token[key] = JToken.FromObject(defaultValue);
            return defaultValue;
        }
        
        public static T As<T>(this JToken token, T defaultValue = default)
        {
            return token != null ? token.ToObject<T>() : defaultValue;
        }
        
        public static void RemoveKey(this JToken target, object key)
        {
            target[key]?.Parent?.Remove();
        }
        
        public static int Increase(this JToken target, object key, int value)
        {
            var count = target[key]?.ToObject<int>() ?? 0;
            count += value;
            target[key] = count;
            return count;
        }
        
        public static bool CheckDiffAndSync<T>(this JToken lastData, object key, JToken currentValue, Action<(T lastValue, T currentValue)> onSync = null) 
        {
            var lastValue = lastData[key];
                
            if (lastValue == null || !JToken.DeepEquals(lastValue, currentValue))
            {
                var lastVal = lastValue != null ? lastValue.ToObject<T>() ?? default(T) : default;
                    
                onSync?.Invoke((lastVal, currentValue.ToObject<T>()));
                lastData[key] = currentValue;
                return true;
            }
                
            return false;
        }
    }
    
    public static class RJTokenExtensions
    {
        public static void Remove(this RJToken target, object key)
        {
            target[key]?.Parent?.Remove();
        }

        public static bool CheckDiffAndSync<T>(this RJToken lastData, object key, JToken currentValue, Action<(T lastValue, T currentValue)> onSync = null) 
        {
            var lastValue = lastData[key];
                
            if (lastValue == null || !JToken.DeepEquals(lastValue, currentValue))
            {
                var lastVal = lastValue != null ? lastValue.ToObject<T>() ?? default(T) : default;
                    
                onSync?.Invoke((lastVal, currentValue.ToObject<T>()));
                lastData[key] = currentValue;
                return true;
            }
                
            return false;
        }
    }
    
    public class RJToken
    {
        public Dictionary<object, Action<JToken>> setActions = new();
        
        private JToken token;
        public JToken Token => token;

        public RJToken(JToken token) => this.token = token;
        
        public virtual JToken? this[object key]
        {
            get => token[key];
            set
            {
                token[key] = value;

                if (setActions.Remove(key, out var action))
                {
                    action(value);
                }
            }
        }

        public void Replace(RJToken value)
        {
            token.Replace(value.token);
        }
    }
}