using System;
using System.Collections.Generic;
using LSCore.ConfigModule;
using Newtonsoft.Json.Linq;

namespace LSCore.LevelSystem
{
    public partial class EntiProps
    {
        static partial void RegisterMigrations()
        {
            var migrations = new List<Action<JToken>>();
            migrations.Add(To1);
            migrations.Add(To2);
            
            Migration.Register<EntiProps>(migrations);
        }

        private static void To1(JToken token)
        {
            if (token["props"] is JObject props)
            {
                props["Arcane2"] = props["Arcane"];
                props.Remove("Arcane");
            }
        }
        
        private static void To2(JToken token)
        {
            if (token["props"] is JObject props)
            {
                props["Arcane3"] = props["Arcane2"];
                props.Remove("Arcane2");
            }
        }
    }
}