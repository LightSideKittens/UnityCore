using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public static partial class Migrator
    {
        private static readonly Dictionary<object, List<Action<JToken>>> byKey = new();
        
        public static void Add(object key, Action<JToken> migrator)
        {
            if (!byKey.TryGetValue(key, out var migrators))
            {
                migrators = new List<Action<JToken>>();
                byKey.Add(key, migrators);
            }
            
            migrators.Add(migrator);
        }

        internal static void Migrate(object key, JToken token)
        {
            const string versionKey = "version";
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
                }
            }
        }

        private static int GetVersion(JToken token, string versionKey)
        {
            int version;
            if (token[versionKey] == null)
            {
                token[versionKey] = 0;
                version = 0;
            }
            else
            {
                version = token[versionKey].ToObject<int>();
            }

            return version;
        }
    }
}