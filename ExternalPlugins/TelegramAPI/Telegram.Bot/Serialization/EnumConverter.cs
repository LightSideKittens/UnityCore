using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Telegram.Bot.Serialization
{
    public class EnumConverter<TEnum> : JsonConverter where TEnum : struct, Enum
    {
        private static readonly Dictionary<string, TEnum> mapRead = new();
        private static readonly Dictionary<TEnum, string> mapWrite = new();
        private static readonly SnakeCaseNamingStrategy namingStrategy = new SnakeCaseNamingStrategy();

        static EnumConverter()
        {
            var enumType = typeof(TEnum);
            var names = Enum.GetNames(enumType);
            var values = (TEnum[])Enum.GetValues(enumType);

            for (int i = 0; i < names.Length; i++)
            {
                var snakeName = namingStrategy.GetPropertyName(names[i], false);
                mapRead[snakeName] = values[i];
                mapWrite[values[i]] = snakeName;
            }
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(TEnum) || objectType == typeof(TEnum?);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (Nullable.GetUnderlyingType(objectType) != null)
                    return null;

                throw new JsonSerializationException($"Cannot convert null value to {objectType}.");
            }

            var str = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(str))
                return default(TEnum);

            if (mapRead.TryGetValue(str, out TEnum value))
                return value;

            throw new JsonSerializationException($"Unexpected value '{str}' for enum {typeof(TEnum).Name}.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is TEnum enumValue)
            {
                if (mapWrite.TryGetValue(enumValue, out var str))
                {
                    writer.WriteValue(str);
                }
                else
                {
                    throw new JsonSerializationException($"Can't serialize value {value} for enum {typeof(TEnum).Name}.");
                }
            }
            else
            {
                writer.WriteNull();
            }
        }
    }
}
