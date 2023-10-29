using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore
{
    internal class Currencies : BaseConfig<Currencies>
    {
        [JsonProperty] private Dictionary<string, int> currencies = new();
        internal static int GetValue(string name)
        {
            var currencies = Config.currencies;
            if (!currencies.TryGetValue(name, out var value))
            {
                currencies[name] = value;
            }
            return value;
        }

        internal static int SetValue(string name, int value) => Config.currencies[name] = value;

        internal static void Earn(string name, int value)
        {
            SetValue(name, value + GetValue(name));
        }

        internal static void Spend(string name, int value, Func<bool> confirmation)
        {
            var currentValue = GetValue(name);
            if (currentValue >= value)
            {
                if (confirmation())
                {
                    SetValue(name, currentValue - value);
                }
            }
        }
    }
}