using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public static class Migration
    {
        private static Dictionary<Type, (int version, List<Action<JToken>> migrations)> migratorsByType = new();

        public static void Register<T>(List<Action<JToken>> migrators) where T : BaseConfig<T>, new()
        {
            migratorsByType.Add(typeof(T), (migrators.Count, migrators));
        }

        public static void Migrate<T>(JToken token) where T : BaseConfig<T>, new()
        {
            if (migratorsByType.TryGetValue(typeof(T), out var data))
            {
                var version = token["version"].ToObject<int>();
                int i;
                for (i = version; i < data.version; i++)
                {
                    data.migrations[i](token);
                }

                token["version"] = i;
            }
        }
    }
}