﻿using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule
{
    public class ConfigSerializationSettings
    {
        public static ConfigSerializationSettings Default { get; } = new();
        public JsonSerializerSettings settings;
        public JsonSerializer serializer;

        public ConfigSerializationSettings(JsonSerializerSettings settings)
        {
            this.settings = settings;
            serializer = JsonSerializer.Create(settings);
        }
        
        public ConfigSerializationSettings()
        {
            settings = CreateDefault();
            serializer = JsonSerializer.Create(settings);
        }

        public static JsonSerializerSettings CreateDefault()
        {
            return new()
            {
                ContractResolver = UnityJsonContractResolver.Instance,
                Error = (_, args) =>
                {
                    args.ErrorContext.Handled = true;
                }
            };
        }
    }
    
    public abstract class BaseConfigManager<T> where T : BaseConfig, new()
    {
        private static string GetLogTag(string tag) => $"{BaseConfig.ConfigTag} {tag}";
        protected T cached;

        [JsonIgnore] protected virtual ConfigSerializationSettings Settings => ConfigSerializationSettings.Default;

        protected virtual string Tag => $"[{typeof(T).Name}]".ToTag(new Color(0.15f, 0.82f, 0.42f));
        public bool isLogEnabled;

        private string meta;
        
        [Conditional("DEBUG")]
        protected void SetMeta(string meta)
        {
            if (string.IsNullOrEmpty(meta))
            {
                this.meta = string.Empty;
                return;
            }
            this.meta = $"\nMeta:\n{meta}";
        }
        
        [Conditional("DEBUG")]
        protected void Log(string message)
        {
            if (isLogEnabled)
            {
                Burger.Log($"{GetLogTag(Tag)} {message} {meta}");
            }
        }
        
        protected virtual void Deserialize(string json)
        {
            SetMeta(json);
            
            cached ??= new T();
            
            Log("Deserializing");
            cached.OnDeserializing();
            
            JsonConvert.PopulateObject(json, cached, Settings.settings);
            
            Log("Deserialized");
            cached.OnDeserialized();
        }
        
        protected virtual void Deserialize(JObject token)
        {
            SetMeta(token.ToString());
            
            cached ??= new T();
            
            Log("Deserializing");
            cached.OnDeserializing();
            
            using (var reader = token.CreateReader())
            {
                Settings.serializer.Populate(reader, cached);
            }
            
            Log("Deserialized");
            cached.OnDeserialized();
        }
        
        protected virtual string Serialize()
        {
            Log("Serializing");
            cached.OnSerializing();
            
            var json = JsonConvert.SerializeObject(cached, Settings.settings);
            SetMeta(json);
            
            cached.OnSerialized();
            Log("Serialized");

            return json;
        }
        
        protected virtual JToken SerializeAsToken()
        {
            Log("Serializing");
            cached.OnSerializing();
            
            var token = JObject.FromObject(cached, Settings.serializer);
            SetMeta(token.ToString());
            
            cached.OnSerialized();
            Log("Serialized");

            return token;
        }
    }
}