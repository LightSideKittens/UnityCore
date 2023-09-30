using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule.Converters
{
    public class Vector2JsonConverter : JsonConverter<Vector2>
    {
        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector2 pos = new Vector2();

            JObject data = JObject.Load(reader);
            pos.x = data["x"]!.ToObject<int>();
            pos.y = data["y"]!.ToObject<int>();

            return pos;
        }

        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            JObject pos = new JObject()
            {
                ["x"] = value.x,
                ["y"] = value.y
            };

            pos.WriteTo(writer);
        }
    }
}