using Newtonsoft.Json;

namespace LSCore.ConfigModule
{
    public abstract class BaseConfigManager<T> where T : BaseConfig, new()
    {
        protected T cached;
        
        [JsonIgnore] protected virtual JsonSerializerSettings Settings { get; } = new()
        {
            ContractResolver = UnityJsonContractResolver.Instance,
            Error = (_, args) =>
            {
                args.ErrorContext.Handled = true;
            }
        };
        
        protected virtual void Deserialize(string json)
        {
            cached ??= new T();
            cached.OnDeserializing();
            JsonConvert.PopulateObject(json, cached, Settings);
            cached.OnDeserialized();
        }
        
        protected virtual string Serialize()
        {
            cached.OnSerializing();
            var json = JsonConvert.SerializeObject(cached, Settings);
            cached.OnSerialized();
            return json;
        }
    }
}