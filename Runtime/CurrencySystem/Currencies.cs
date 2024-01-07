using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore
{
    public class Currencies : BaseConfig<Currencies>
    {
        [JsonProperty] private Dictionary<string, int> currencies = new();
        internal static readonly Dictionary<string, Action<int>> onChangedActions = new();
        
        internal static int GetValue(string name)
        {
            var currencies = Config.currencies;
            if (!currencies.TryGetValue(name, out var value))
            {
                currencies[name] = value;
            }
            return value;
        }

        internal static void SetValue(string name, int value)
        { 
            Config.currencies[name] = value;
            TryInvokeOnChanged(name, value);
        }

        internal static void Earn(string name, int value)
        {
            SetValue(name, value + GetValue(name));
        }

        internal static bool Spend(string name, int value, out Action spend)
        {
            var currentValue = GetValue(name);
            if (currentValue >= value)
            {
                spend = () => SetValue(name, currentValue - value);
                return true;
            }

            spend = null;
            return false;
        }
        
        private static void TryInvokeOnChanged(string name, int value)
        {
            if (onChangedActions.TryGetValue(name, out var action))
            {
                action(value);
            }
        }

        public static void Clear(string name)
        {
            Config.currencies.Remove(name);
            onChangedActions.Remove(name);
        }
    }
}