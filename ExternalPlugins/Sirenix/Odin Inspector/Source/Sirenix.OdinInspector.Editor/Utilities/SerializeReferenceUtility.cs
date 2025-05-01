//-----------------------------------------------------------------------
// <copyright file="SerializeReferenceUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System;
using System.Collections.Generic;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	public enum SerializeReferenceValidityResult
	{
		Valid,
		Error,
		Warning
	}
	
	public static class SerializeReferenceUtility
	{
		private static readonly string MessagePrefix = $"[{nameof(SerializeReference)}] ";

		public static SerializeReferenceValidityResult ValidateBaseType(Type baseType, out string errorMessage)
		{
			if (baseType.IsValueType)
			{
				errorMessage =
					$"{MessagePrefix}'{baseType.GetNiceName()}' is a value type, and value types are not supported by [{nameof(SerializeReference)}]. Use [{nameof(SerializeField)}] instead.";

				return SerializeReferenceValidityResult.Warning;
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(baseType))
			{
				errorMessage = $"{MessagePrefix}'{baseType.GetNiceName()}' derives from '{typeof(UnityEngine.Object).FullName}'.";

				return SerializeReferenceValidityResult.Error;
			}

			if (baseType.IsGenericType)
			{
				if (!UnityVersion.IsVersionOrGreater(2023, 1) && !IsSubclassOfList(baseType))
				{
					errorMessage = $"{MessagePrefix}does not support the generic '{baseType.GetNiceName()}'.";

					return SerializeReferenceValidityResult.Error;
				}
			}

			errorMessage = string.Empty;

			return SerializeReferenceValidityResult.Valid;
		}

		public static SerializeReferenceValidityResult ValidateInstanceType(Type baseType, Type instanceType, out string errorMessage)
		{
			if (instanceType == null)
			{
				errorMessage = string.Empty;

				return SerializeReferenceValidityResult.Valid;
			}

			if (instanceType.IsValueType)
			{
				errorMessage =
					$"{MessagePrefix}'{instanceType.GetNiceName()}' is a value type, and value types are not supported by [{nameof(SerializeReference)}]. Use [{nameof(SerializeField)}] instead.";

				return SerializeReferenceValidityResult.Warning;
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(instanceType))
			{
				errorMessage = $"{MessagePrefix}'{instanceType.GetNiceName()}' derives from '{typeof(UnityEngine.Object).FullName}'.";

				return SerializeReferenceValidityResult.Error;
			}

			if (instanceType.IsArray)
			{
				if (baseType == typeof(object))
				{
					errorMessage = $"{MessagePrefix}'{instanceType.GetNiceName()}' cannot be assigned to '{typeof(object).FullName}'.";

					return SerializeReferenceValidityResult.Error;
				}

				return ValidateElementType(instanceType.GetElementType(), out errorMessage);
			}

			if (IsSubclassOfList(instanceType))
			{
				if (baseType == typeof(object))
				{
					errorMessage = $"{MessagePrefix}'{instanceType.GetNiceName()}' cannot be assigned to '{typeof(object).FullName}'.";

					return SerializeReferenceValidityResult.Error;
				}

				return ValidateElementType(GetListElementType(instanceType), out errorMessage);
			}

			errorMessage = string.Empty;

			return SerializeReferenceValidityResult.Valid;
		}

		private static SerializeReferenceValidityResult ValidateElementType(Type elementType, out string errorMessage)
		{
			if (elementType.IsValueType)
			{
				errorMessage =
					$"{MessagePrefix}the element type '{elementType.GetNiceName()}' is a value type, and value types are not supported by [{nameof(SerializeReference)}]. Use [{nameof(SerializeField)}] instead.";

				return SerializeReferenceValidityResult.Warning;
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(elementType))
			{
				errorMessage = $"{MessagePrefix}the element type '{elementType.GetNiceName()}' derives from '{typeof(UnityEngine.Object).FullName}'.";

				return SerializeReferenceValidityResult.Error;
			}

			errorMessage = string.Empty;

			return SerializeReferenceValidityResult.Valid;
		}
		
		private static bool IsSubclassOfList(Type type) => type.ImplementsOpenGenericClass(typeof(List<>));

		private static Type GetListElementType(Type type)
		{
			while (true)
			{
				Type baseType = type.BaseType;

				if (type.IsGenericType)
				{
					Type genericDef = type.GetGenericTypeDefinition();

					if (genericDef == typeof(List<>))
					{
						return type.GenericTypeArguments[0];
					}
				}

				if (baseType is null)
				{
					return null;
				}

				type = baseType;
			}
		}
	}
}
#endif