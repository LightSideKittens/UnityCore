using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule.Converters
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            Color col = new Color();

            JObject data = JObject.Load(reader);
            col.r = data["r"]!.ToObject<float>();
            col.g = data["g"]!.ToObject<float>();
            col.b = data["b"]!.ToObject<float>();
            col.a = data["a"]!.ToObject<float>();

            return col;
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            JObject col = new JObject()
            {
                ["r"] = value.r,
                ["g"] = value.g,
                ["b"] = value.b,
                ["a"] = value.a
            };

            col.WriteTo(writer);
        }
    }
}