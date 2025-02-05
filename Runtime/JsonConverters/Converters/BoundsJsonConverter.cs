using System;
using LSCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule.Converters
{
    public class BoundsJsonConverter : JsonConverter<Bounds>
    {
        public override Bounds ReadJson(JsonReader reader, Type objectType, Bounds existingValue,
            bool hasExistingValue, JsonSerializer serializer)
        {
            Bounds col = new Bounds();

            JObject data = JObject.Load(reader);
            col.center = data["center"]!.ToVector3();
            col.size = data["size"]!.ToVector3();

            return col;
        }

        public override void WriteJson(JsonWriter writer, Bounds value, JsonSerializer serializer)
        {
            JObject col = new JObject()
            {
                ["center"] = value.center.ToJObject(),
                ["size"] = value.size.ToJObject(),
            };

            col.WriteTo(writer);
        }
    }
}