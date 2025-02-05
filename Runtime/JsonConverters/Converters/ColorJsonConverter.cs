using System;
using LSCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace LSCore.ConfigModule.Converters
{
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject data = JObject.Load(reader);
            return data.ToColor();
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            value.ToJObject().WriteTo(writer);
        }
    }
}