using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore
{
    internal class Currencies : BaseConfig<Currencies>
    {
        [JsonProperty] private Dictionary<string, int> currencies = new();
        internal static int GetValue<T>() where T : BaseCurrency<T>, new()
        {
            var currencies = Config.currencies;
            var name = typeof(T).Name;
            if (!currencies.TryGetValue(name, out var value))
            {
                currencies[name] = value;
            }
            return value;
        }

        internal static int SetValue<T>(int value) where T : BaseCurrency<T>, new() => Config.currencies[typeof(T).Name] = value;
    }
}