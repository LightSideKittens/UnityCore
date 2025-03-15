using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace Telegram.Bot.Serialization
{
    internal class InputFileConverter : JsonConverter<InputFile?>
    {
        internal static readonly AsyncLocal<List<InputFileStream>?> Attachments = new();

        public override InputFile ReadJson(JsonReader reader, Type objectType, InputFile existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;
            
            var value = reader.Value?.ToString();
            if (value is null)
                return null;

            if (value.StartsWith("attach://", StringComparison.OrdinalIgnoreCase))
                return new InputFileStream(Stream.Null, value.Substring("attach://".Length));

            return Uri.TryCreate(value, UriKind.Absolute, out var url)
                ? new InputFileUrl(url)
                : new InputFileId(value);
        }

        public override void WriteJson(JsonWriter writer, InputFile? value, JsonSerializer serializer)
        {
            switch (value)
            {
                case InputFileId file:
                    writer.WriteValue(file.Id);
                    break;
                case InputFileUrl file:
                    writer.WriteValue(file.Url.ToString());
                    break;
                case InputFileStream file:
                    var attachments = Attachments.Value ??= new List<InputFileStream>();
                    writer.WriteValue($"attach://{attachments.Count}");
                    attachments.Add(file);
                    break;
                case null:
                    writer.WriteNull();
                    break;
                default:
                    throw new JsonSerializationException("File Type not supported");
            }
        }
    }
}
