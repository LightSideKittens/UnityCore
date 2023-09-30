using System.Collections.Generic;

namespace LSCore.Firebase
{
    public static class Data
    {
        public static Dictionary<string, object> Create(string key, object value)
        {
            return new Dictionary<string, object>() { {key, value} };
        }
        
        public static Dictionary<string, object> Add(this Dictionary<string, object> data, string key, object value)
        {
            data.Add(key, value);
            return data;
        }
    }
}