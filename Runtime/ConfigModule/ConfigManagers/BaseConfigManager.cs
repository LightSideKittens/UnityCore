using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule
{
    public abstract class BaseConfigManager<T> where T : BaseConfig, new()
    {
        private static string GetLogTag(string tag) => $"[{tag}]".ToTag(new Color(1f, 0.8f, 0.05f));
        protected T cached;

        [JsonIgnore] protected virtual JsonSerializerSettings Settings { get; } = new()
        {
            ContractResolver = UnityJsonContractResolver.Instance,
            Error = (_, args) =>
            {
                args.ErrorContext.Handled = true;
            }
        };

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
            
            JsonConvert.PopulateObject(json, cached, Settings);
            
            Log("Deserialized");
            cached.OnDeserialized();
        }
        
        protected virtual void Deserialize(JObject token)
        {
            SetMeta(token.ToString());
            
            cached ??= new T();
            
            Log("Deserializing");
            cached.OnDeserializing();
            
            var serializer = JsonSerializer.Create(Settings);
            using (var reader = token.CreateReader())
            {
                serializer.Populate(reader, cached);
            }
            
            Log("Deserialized");
            cached.OnDeserialized();
        }
        
        protected virtual string Serialize()
        {
            Log("Serializing");
            cached.OnSerializing();
            
            var json = JsonConvert.SerializeObject(cached, Settings);
            SetMeta(json);
            
            cached.OnSerialized();
            Log("Serialized");

            return json;
        }
        
        protected virtual string Serialize(JObject token)
        {
            Log("Serializing");
            cached.OnSerializing();
            
            var serializer = JsonSerializer.Create(Settings);
            var updatedToken = JObject.FromObject(cached, serializer);
            
            token.Merge(updatedToken, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Replace
            });

            var json = token.ToString();
            SetMeta(json);
            
            cached.OnSerialized();
            Log("Serialized");

            return json;
        }
    }
}