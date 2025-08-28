using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LSCore.ConfigModule
{
    public class FirstTime : GameSingleConfig<FirstTime>
    {
        [JsonProperty] private HashSet<string> states = new();

        public static bool IsNot(string name, out Action pass)
        {
            if (Config.states.Contains(name))
            {
                pass = null;
                return false;
            }

            pass = () => Config.states.Add(name);
            return true;
        }

        public static bool IsNot(string name) => Config.states.Contains(name);
        public static bool Pass(string name) => Config.states.Add(name);
    }
}