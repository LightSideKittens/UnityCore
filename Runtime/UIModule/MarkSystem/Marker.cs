using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;

namespace LSCore
{
    public class Marker : JTokenGameConfig
    {
        private static JToken Config => Get(nameof(Marker));
        
        protected static JToken GetTarget(string group)
        {
            var target = Config[group];
            if (target != null) return target;
                    
            target = new JObject();
            Config[group] = target;

            return target;
        }
            
        public static void Mark(string group, string id, object value) => GetTarget(group)[id] = JToken.FromObject(value);
        public static void UnMark(string group, string id) => GetTarget(group).RemoveKey(id);

        public static bool TryGet<T>(string group, string id, out T value)
        {
            var token = GetTarget(group)[id];
            if (token == null)
            {
                value = default;
                return false;
            }

            value = token.ToObject<T>();
            return true;
        }
    }
}