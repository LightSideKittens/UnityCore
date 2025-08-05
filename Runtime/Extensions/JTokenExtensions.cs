using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LSCore.Extensions
{
    public class JHashSet<T>
    {
        private HashSet<T> set;
        private JArray array;

        public JHashSet(JArray array)
        {
            this.array = array;
            set = array.ToObject<HashSet<T>>();
        }

        public bool Add(T value)
        {
            if (set.Add(value))
            {
                array.Add(value);
                return true;
            }
            return false;
        }

        public bool Remove(T value)
        {
            if (set.Remove(value))
            {
                array.Remove(JToken.FromObject(value));
                return true;
            }
            return false;
        }

        public bool Contains(T value)
        {
            return set.Contains(value);
        }
    }
    
    public static partial class JTokenExtensions
    {
        public static JObject FromObject(this object o, JsonSerializer serializer)
        {
            var tokenWriter = new JTokenWriter();
            serializer.Serialize(tokenWriter, o, typeof(object));
            return (JObject)tokenWriter.Token;
        }
        
        public static bool TryGetValue(this JToken token, object key, out JToken value)
        {
            value = token[key];
            return value != null;
        }
        
        public static bool TryGetValue<T>(this JToken token, object key, out T value) where T : JToken
        {
            value = token[key] as T;
            return value != null;
        }
        
        public static T As<T>(this JToken token, object key, T defaultValue = default)
        {
            if (token[key] != null)
            {
                return token[key].ToObject<T>();
            }
            
            token[key] = JToken.FromObject(defaultValue);
            return defaultValue;
        }
        
        public static T AsJ<T>(this JToken token, object key, T defaultValue) where T : JToken
        {
            if (token[key] != null)
            {
                return token[key] as T;
            }

            token[key] = defaultValue;
            return defaultValue;
        }

        public static T AsJ<T>(this JToken token, object key) where T : JToken, new()
        {
            return (T)(token[key] ??= new T());
        }
        
        public static T As<T>(this JToken token, T defaultValue = default)
        {
            return token != null ? token.ToObject<T>() : defaultValue;
        }
        
        public static void RemoveKey(this JToken target, object key)
        {
            target[key]?.Parent?.Remove();
        }
        
        
        public static string ToStr(this JToken token)
        {
            return Convert.ToString(((JValue)token).Value, invariantCulture);
        }
        
        public static JValue ToJValue(this string value)
        {
            return new JValue(value);
        }
        
        public static int Increase(this JToken target, object key, int value)
        {
            var count = target[key]?.ToInt() ?? 0;
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
        public static void Remove(this IRJToken target, object key)
        {
            target[key]?.Parent?.Remove();
        }

        public static bool CheckDiffAndSync<T>(this IRJToken lastData, object key, JToken currentValue, Action<(T lastValue, T currentValue)> onSync = null) 
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
        
        public static T As<T>(this IRJToken token, object key, T defaultValue = default)
        {
            if (token[key] != null)
            {
                return token[key].ToObject<T>();
            }
            
            token[key] = JToken.FromObject(defaultValue);
            return defaultValue;
        }
        
        public static T AsJ<T>(this IRJToken token, object key) where T : JToken, new()
        {
            return (T)(token[key] ??= new T());
        }
    }
        
    public struct Wrapper : IRJToken
    {
        public static Wrapper wrapper = new();
        public JToken token;

        public JToken this[object key]
        {
            get => token[key];
            set => token[key] = value;
        }
    }

    public interface IRJToken
    {
        public JToken? this[object key] { get; set; }
    }
    
    public class BaseRJToken<T> : IRJToken where T : JToken
    {
        private Dictionary<object, Action<JToken>> tempSetActions = new();
        private Dictionary<object, Action<JToken>> setActions = new();
        
        public void Listen(object key, Action<JToken> action) => setActions[key] = action;
        public void ListenTemp(object key, Action<JToken> action) => tempSetActions[key] = action;
        public bool UnListenTemp(object key, Action<JToken> action) => tempSetActions.Remove(key);
        
        private T token;
        public T Token => token;

        public BaseRJToken(T token) => this.token = token;
        
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

                if (tempSetActions.TryGetValue(key, out action))
                {
                    action(value);
                }
            }
        }

        public void Replace(BaseRJToken<T> value)
        {
            token.Replace(value.token);
        }
        
        public int Increase(object key, int value)
        {
            var count = this[key]?.ToInt() ?? 0;
            count += value;
            this[key] = count;
            return count;
        }
    }
    
    public class RJToken : BaseRJToken<JToken>
    {
        public RJToken(JToken token) : base(token)
        {
        }
    }
    
    public class RJObject : BaseRJToken<JObject>
    {
        public RJObject(JObject token) : base(token)
        {
        }
        
        public static implicit operator RJObject(JObject token) => new(token);

        public bool ContainsKey(string key) => Token.ContainsKey(key);
    }
}