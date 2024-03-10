using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LSCore.ConfigModule
{
    public class FirstTime : BaseSingleConfig<FirstTime>
    {
        [JsonProperty] private HashSet<string> states = new();

        public static bool IsNot(string name, out Action pass)
        {
            pass = () => Config.states.Add(name);
            return !Config.states.Contains(name);
        }
    }
}