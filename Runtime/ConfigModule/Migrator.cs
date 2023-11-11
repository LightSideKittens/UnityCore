using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public static class Migrator
    {
        private static readonly Dictionary<string, List<Action<JToken>>> byKey = new();
        
        public static void Add(string key, Action<JToken> migrator)
        {
            if (!byKey.TryGetValue(key, out var migrators))
            {
                migrators = new List<Action<JToken>>();
                byKey.Add(key, migrators);
            }
            
            migrators.Add(migrator);
        }

        internal static void Migrate(string key, JToken token)
        {
            if (byKey.TryGetValue(key, out var migration))
            {
                if (migration.Count > 0)
                {
                    int version;
                    if (token["version"] == null)
                    {
                        token["version"] = 0;
                        version = 0;
                    }
                    else
                    {
                        version = token["version"].ToObject<int>();
                    }
                    
                    int i;
                    for (i = version; i < migration.Count; i++)
                    {
                        migration[i](token);
                    }

                    token["version"] = i;
                }
            }
        }
    }
}