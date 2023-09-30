using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule.Converters
{
    public class Vector4JsonConverter : JsonConverter<Vector4>
    {
        public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector4 pos = new Vector4();

            JObject data = JObject.Load(reader);
            pos.x = data["x"]!.ToObject<int>();
            pos.y = data["y"]!.ToObject<int>();
            pos.z = data["z"]!.ToObject<int>();
            pos.w = data["w"]!.ToObject<int>();

            return pos;
        }

        public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
        {
            JObject pos = new JObject()
            {
                ["x"] = value.x,
                ["y"] = value.y,
                ["z"] = value.z,
                ["w"] = value.w,
            };

            pos.WriteTo(writer);
        }
    }
}