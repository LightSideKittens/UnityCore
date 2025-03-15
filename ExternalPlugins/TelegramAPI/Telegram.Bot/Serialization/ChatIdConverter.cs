using System;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Telegram.Bot.Serialization
{
    internal class ChatIdConverter : JsonConverter<ChatId?>
    {
        public override ChatId ReadJson(JsonReader reader, Type objectType, ChatId existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            
            var valueStr = reader.Value?.ToString();
            if (string.IsNullOrWhiteSpace(valueStr))
                return null;
            
            return new ChatId(valueStr);
        }

        public override void WriteJson(JsonWriter writer, ChatId? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            
            if (!string.IsNullOrEmpty(value.Username))
            {
                writer.WriteValue(value.Username);
            }
            
            else if (value.Identifier != null)
            {
                writer.WriteValue(value.Identifier);
            }
            else
            {
                throw new JsonSerializationException("Chat ID value is incorrect");
            }
        }
    }
}