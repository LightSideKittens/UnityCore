using System;
using Newtonsoft.Json;

namespace Telegram.Bot.Serialization
{
    internal static class UnixDateTimeConverterUtil
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static DateTime Read(JsonReader reader, Type typeToConvert)
        {
            long seconds;

            if (reader.TokenType == JsonToken.Null)
                return default;

            // Обработка числового значения или строки
            if (reader.TokenType == JsonToken.Integer)
            {
                seconds = System.Convert.ToInt64(reader.Value);
            }
            else if (reader.TokenType == JsonToken.String)
            {
                if (!long.TryParse(reader.Value?.ToString(), out seconds))
                    throw new JsonSerializationException("Invalid Unix timestamp value.");
            }
            else
            {
                throw new JsonSerializationException($"Unexpected token type {reader.TokenType} when parsing Unix timestamp.");
            }

            if (seconds > 0)
                return UnixEpoch.AddSeconds(seconds);
            if (seconds == 0)
                return default; // удобнее тестировать, чем 1/1/1970
            throw new JsonSerializationException(
                $"Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {typeToConvert}.");
        }

        internal static void Write(JsonWriter writer, DateTime value)
        {
            if (value == default)
            {
                writer.WriteValue(0L);
            }
            else
            {
                long seconds = (long)(value.ToUniversalTime() - UnixEpoch).TotalSeconds;
                if (seconds >= 0)
                    writer.WriteValue(seconds);
                else
                    throw new JsonSerializationException(
                        "Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
            }
        }
    }
}
