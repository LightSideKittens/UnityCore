using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace LSCore.ConfigModule
{
    public static partial class Migrator
    {
        public static class Type<T>
        {
            private static readonly Dictionary<Type, List<Action<JToken>>> byKey = new();
        
            public static void Add(Action<JToken> migrator)
            {
                if (!byKey.TryGetValue(typeof(T), out var migrators))
                {
                    migrators = new List<Action<JToken>>();
                    byKey.Add(typeof(T), migrators);
                }
            
                migrators.Add(migrator);
            }

            internal static void Migrate(JToken token)
            {
                const string versionKey = "type_version";
                if (byKey.TryGetValue(typeof(T), out var migration))
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
        }
    }
}