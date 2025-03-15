using System;
using Newtonsoft.Json;

namespace Telegram.Bot.Serialization
{
    internal class BanTimeConverter : JsonConverter<DateTime?>
    {
        public override DateTime? ReadJson(JsonReader reader, Type objectType, DateTime? existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (Nullable.GetUnderlyingType(objectType) == null)
                    throw new JsonSerializationException($"Cannot convert null value to {objectType}.");
                return null;
            }
            
            long value = System.Convert.ToInt64(reader.Value);
            if (value == 0L)
                return null;
            
            return DateTimeOffset.FromUnixTimeSeconds(value).UtcDateTime;
        }

        public override void WriteJson(JsonWriter writer, DateTime? value, JsonSerializer serializer)
        {
            if (value is null || value.Value == default)
            {
                writer.WriteValue(0);
            }
            else
            {
                long unixTime = new DateTimeOffset(value.Value).ToUnixTimeSeconds();
                writer.WriteValue(unixTime);
            }
        }
    }
}