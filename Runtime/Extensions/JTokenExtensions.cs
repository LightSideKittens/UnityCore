using System;
using System.Collections.Generic;
using System.IO;
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
        public static JToken FindByPath(string json, string targetPath)
        {
            using var reader = new JsonTextReader(new StringReader(json));

            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if (!reader.Read()) return null;

                    if (string.Equals(reader.Path, targetPath, StringComparison.Ordinal))
                    {
                        return JToken.ReadFrom(reader);
                    }
                }
            }
            return null;
        }
        
        public static JToken[] FindByPath(string json, params string[] targetPaths)
        {
            using var reader = new JsonTextReader(new StringReader(json));
            int index = 0;
            int length = targetPaths.Length;
            var tokens = new JToken[length];
            
            while (index < length && reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    if (!reader.Read()) return null;

                    if (string.Equals(reader.Path, targetPaths[index], StringComparison.Ordinal))
                    {
                        tokens[index] = JToken.ReadFrom(reader);
                        index++;
                    }
                }
            }
            
            return tokens;
        }
        
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
        
        public static T AsJ<T>(this JArray array, int index) where T : JToken, new()
        {
            T result; 
            if (index <= array.Count)
            {
                result = new T();
                array.Add(result);
            }
            else
            {
                result = array[index] as T;
            }
            
            return result;
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
}