using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json;

namespace LSCore
{
    internal class Currencies : GameSingleConfig<Currencies>
    {
        [JsonProperty] private Dictionary<string, int> currencies = new();
        internal static readonly Dictionary<string, Action<(int last, int current)>> onChangedActions = new();

#if UNITY_EDITOR
        static Currencies()
        {
            World.Destroyed += onChangedActions.Clear;
        }
#endif
        
        internal static int GetAmount(string name)
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
            var c = Config;
            var last = c.currencies[name];
            c.currencies[name] = value;
            TryInvokeOnChanged(name, (last, value));
        }

        internal static void Earn(string name, int value)
        {
            SetValue(name, value + GetAmount(name));
        }

        internal static bool Spend(string name, int value, out Action spend)
        {
            var currentValue = GetAmount(name);
            if (currentValue >= value)
            {
                spend = () => SetValue(name, currentValue - value);
                return true;
            }

            spend = null;
            return false;
        }
        
        internal static void ForceSpend(string name, int value)
        {
            var currentValue = GetAmount(name);
            SetValue(name, currentValue - value);
        }

        internal static void Remove(string name)
        {
            Config.currencies.Remove(name);
        }

        private static void TryInvokeOnChanged(string name, (int last, int current) data)
        {
            if (onChangedActions.TryGetValue(name, out var action))
            {
                action(data);
            }
        }
    }
}