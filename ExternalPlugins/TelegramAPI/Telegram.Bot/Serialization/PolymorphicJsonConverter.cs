using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Telegram.Bot.Serialization
{
    internal sealed class PolymorphicJsonConverter<T> : JsonConverter
    {
        private readonly string _discriminatorPropName;
        private readonly Dictionary<string, Type> _discriminatorToSubtype = new();

        public PolymorphicJsonConverter()
        {
            var attr = typeof(T).GetCustomAttribute<CustomJsonPolymorphicAttribute>() 
                ?? throw new InvalidOperationException($"{typeof(T).Name} doesn't mark by CustomJsonPolymorphicAttribute");
            
            var namingStrategy = new SnakeCaseNamingStrategy();
            _discriminatorPropName = namingStrategy.GetPropertyName(attr.TypeDiscriminatorPropertyName ?? "$type", false);
            
            foreach (var subtype in typeof(T).GetCustomAttributes<CustomJsonDerivedTypeAttribute>())
            {
                if (subtype.Discriminator is not null)
                {
                    _discriminatorToSubtype.Add(subtype.Discriminator, subtype.Subtype);
                }
            }
        }

        public override bool CanConvert(Type objectType) => objectType == typeof(T);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var typeField = token[_discriminatorPropName];
            if (typeField == null || typeField.Type != JTokenType.String)
            {
                throw new JsonSerializationException($"Cannot find the property {_discriminatorPropName} at deserialization {typeof(T).Name}");
            }
            var typeName = typeField.Value<string>();
            if (!_discriminatorToSubtype.TryGetValue(typeName, out var subtype))
            {
                throw new JsonSerializationException($"Unknown type: {typeName}");
            }
            return token.ToObject(subtype, serializer)!;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is null)
            {
                writer.WriteNull();
                return;
            }
            serializer.Serialize(writer, value, value.GetType());
        }
    }
}
