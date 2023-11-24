using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using LSCore.ConfigModule.Converters;
using Newtonsoft.Json;
using UnityEngine;

namespace LSCore.ConfigModule
{
	public class UnityJsonContractResolver : DefaultContractResolver
	{
		public static readonly UnityJsonContractResolver Instance = new UnityJsonContractResolver();

		private static readonly Dictionary<Type, JsonConverter> typeMap = new Dictionary<Type, JsonConverter>()
		{
			{typeof(Vector4), new Vector4JsonConverter()},
			{typeof(Vector3), new Vector3JsonConverter()},
			{typeof(Vector2), new Vector2JsonConverter()},
			{typeof(Color), new ColorJsonConverter()},
			{typeof(Bounds), new BoundsJsonConverter()},
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

		protected override JsonContract CreateContract(Type objectType)
		{
			JsonContract contract = base.CreateContract(objectType);

			if (typeMap.TryGetValue(objectType, out var converter))
			{
				contract.Converter = converter;
			}
			
			return contract;
		}
	}
}