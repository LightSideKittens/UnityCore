using Newtonsoft.Json.Linq;

namespace LSCore.LevelSystem
{
    public partial class EntiProps
    {
        static partial void RegisterMigrations()
        {
            Migration.Create(To1)
                .Add(To2)
                ;
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