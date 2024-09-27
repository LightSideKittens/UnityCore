using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule
{
    public class ConfigSerializationSettings
    {
        public JsonSerializerSettings settings;
        public JsonSerializer serializer;

        public ConfigSerializationSettings(JsonSerializerSettings settings)
        {
            this.settings = settings;
            serializer = JsonSerializer.Create(settings);
        }
        
        public ConfigSerializationSettings()
        {
            settings = new()
            {
                ContractResolver = UnityJsonContractResolver.Instance,
                Error = (_, args) =>
                {
                    args.ErrorContext.Handled = true;
                }
            };

            serializer = JsonSerializer.Create(settings);
        }
    }
    
    public abstract class BaseConfigManager<T> where T : BaseConfig, new()
    {
        private static string GetLogTag(string tag) => $"[{tag}]".ToTag(new Color(1f, 0.8f, 0.05f));
        protected T cached;

        [JsonIgnore] protected virtual ConfigSerializationSettings Settings { get; } = new();

        protected virtual string Tag => typeof(T).Name;

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
            Burger.Log($"{GetLogTag(Tag)} {message} {meta}");
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