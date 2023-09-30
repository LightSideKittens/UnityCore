using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule.Converters
{
    public class Vector3JsonConverter : JsonConverter<Vector3>
    {
        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            Vector3 pos = new Vector3();

            JObject data = JObject.Load(reader);
            pos.x = data["x"]!.ToObject<int>();
            pos.y = data["y"]!.ToObject<int>();
            pos.z = data["z"]!.ToObject<int>();

            return pos;
        }

        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            JObject pos = new JObject()
            {
                ["x"] = value.x,
                ["y"] = value.y,
                ["z"] = value.z
            };

            pos.WriteTo(writer);
        }
    }
}