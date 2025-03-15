using System;
using Newtonsoft.Json;

namespace Telegram.Bot.Serialization
{
    internal class UnixDateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                throw new JsonSerializationException("Cannot convert null value to DateTime.");

            long unixTime;
            switch (reader.TokenType)
            {
                case JsonToken.Integer:
                    unixTime = System.Convert.ToInt64(reader.Value);
                    break;
                case JsonToken.String:
                    if (!long.TryParse(reader.Value?.ToString(), out unixTime))
                        throw new JsonSerializationException("Invalid Unix timestamp value.");
                    break;
                default:
                    throw new JsonSerializationException($"Unexpected token parsing DateTime. Expected Integer or String, got {reader.TokenType}.");
            }
            
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).UtcDateTime;
        }

        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            long unixTime = new DateTimeOffset(value).ToUnixTimeSeconds();
            writer.WriteValue(unixTime);
        }
    }
}