using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using LSCore.ConfigModule.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace LSCore
{
	public class SerializationSettings
	{
		public static SerializationSettings Default { get; } = new();
		public JsonSerializerSettings settings;
		public JsonSerializer serializer;

		public SerializationSettings(JsonSerializerSettings settings)
		{
			this.settings = settings;
			serializer = JsonSerializer.Create(settings);
		}
        
		public SerializationSettings()
		{
			settings = new()
			{
				ContractResolver = UnityJsonContractResolver.Instance,
				Error = (_, args) =>
				{
					args.ErrorContext.Handled = true;
				}
			};

			serializer = JsonSerializer.Create(settings);
		}
	}
	
	public class UnityJsonContractResolver : DefaultContractResolver
	{
		public static UnityJsonContractResolver Instance { get; } = new();

		public static Dictionary<Type, JsonConverter> TypeMap { get; } = new()
		{
			{ typeof(Vector4), new Vector4JsonConverter() },
			{ typeof(Vector3), new Vector3JsonConverter() },
			{ typeof(Vector2), new Vector2JsonConverter() },
			{ typeof(Color), new ColorJsonConverter() },
			{ typeof(Bounds), new BoundsJsonConverter() },
		};

		protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
		{
			JsonProperty property = base.CreateProperty(member, memberSerialization);
			
			if (typeof(UnityEngine.Object).IsAssignableFrom(property.PropertyType))
			{
				property.Ignored = true;
			}

			return property;
		}
		
		public static JsonContract GetContract(Type type) => Instance.CreateContract(type);
		
		protected override JsonContract CreateContract(Type objectType)
		{
			JsonContract contract = base.CreateContract(objectType);

			if (TypeMap.TryGetValue(objectType, out var converter))
			{
				contract.Converter = converter;
			}
			
			return contract;
		}
	}
}