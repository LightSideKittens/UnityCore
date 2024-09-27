using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public static partial class Migrator
    {
        private static readonly Dictionary<object, List<Action<JToken>>> byKey = new();
        private const string versionKey = "version";
        
        public static void Add(object key, Action<JToken> migrator)
        {
            if (!byKey.TryGetValue(key, out var migrators))
            {
                migrators = new List<Action<JToken>>();
                byKey.Add(key, migrators);
            }
            
            migrators.Add(migrator);
        }
        
        internal static void PopulateMeta(JToken src, JToken dst)
        {
            var token = src[versionKey];
            if (token != null)
            {
                dst[versionKey] = token;
            }
        }

        internal static bool Migrate(object key, JToken token)
        {
            if (byKey.TryGetValue(key, out var migration))
            {
                if (migration.Count > 0)
                {
                    int version = GetVersion(token, versionKey);
                    
                    int i;
                    for (i = version; i < migration.Count; i++)
                    {
                        migration[i](token);
                    }

                    token[versionKey] = i;
                    return i < migration.Count;
                }
            }

            return false;
        }

        private static int GetVersion(JToken token, string key)
        {
            int version;
            if (token[key] == null)
            {
                token[key] = 0;
                version = 0;
            }
            else
            {
                version = token[key].ToObject<int>();
            }

            return version;
        }
    }
}