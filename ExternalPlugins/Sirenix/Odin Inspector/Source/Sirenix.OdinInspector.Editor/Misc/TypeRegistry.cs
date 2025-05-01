//-----------------------------------------------------------------------
// <copyright file="TypeRegistry.cs" company="Sirenix ApS">
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
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Sirenix.Config;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

	public static class TypeRegistry
	{
		internal static HashSet<Type> ValidTypes;

		internal static HashSet<Type> HiddenTypes = new HashSet<Type>();

		internal static readonly Dictionary<AssemblyCategory, string> CategoryStringMap = new Dictionary<AssemblyCategory, string>();

		internal static readonly Dictionary<Type, TypeRegistryItemAttribute> ItemSettings = new Dictionary<Type, TypeRegistryItemAttribute>();

		internal static Dictionary<string, string> NamespacePaths = new Dictionary<string, string>(64);

		internal static Type[] SystemTypes =
		{
			// default types
			typeof(String),
			typeof(Boolean),
			typeof(SByte),
			typeof(Int16),
			typeof(Int32),
			typeof(Int64),
			typeof(Byte),
			typeof(UInt16),
			typeof(UInt32),
			typeof(UInt64),
			typeof(Single),
			typeof(Double),

			// generic collections
			typeof(Array),
			typeof(List<>),
			typeof(Dictionary<,>),
			typeof(HashSet<>),
			typeof(Stack<>),
			typeof(Queue<>),
			typeof(Hashtable),
			typeof(LinkedList<>),
			typeof(LinkedListNode<>),
			typeof(SortedDictionary<,>),
			typeof(SortedList<,>),
			typeof(SortedSet<>),
			typeof(SortedList),
			typeof(ConcurrentBag<>),
			typeof(ConcurrentDictionary<,>),
			typeof(ConcurrentQueue<>),
			typeof(ConcurrentStack<>)
		};

		private static readonly Type TypeRegistryItemAttributeType = typeof(TypeRegistryItemAttribute);

		static TypeRegistry()
		{
			ValidTypes = new HashSet<Type>();

			foreach (Type type in AssemblyUtilities.GetTypes(AssemblyCategory.All))
			{
				if (IsGeneratedType(type))
				{
					continue;
				}

				ValidTypes.Add(type);
			}

			TypeCache.TypeCollection typesWithSettings = TypeCache.GetTypesWithAttribute(TypeRegistryItemAttributeType);

			for (var i = 0; i < typesWithSettings.Count; i++)
			{
				Type type = typesWithSettings[i];

				ItemSettings[type] = type.GetAttribute<TypeRegistryItemAttribute>(false);
			}

			ValidTypes.AddRange(SystemTypes);
		}

		internal static string GetName(Type type)
		{
			TypeSettings userSettings = TypeRegistryUserConfig.Instance.TryGetSettings(type);

			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				return userSettings.Name;
			}

			if (ItemSettings.TryGetValue(type, out TypeRegistryItemAttribute settings))
			{
				return !string.IsNullOrEmpty(settings.Name) ? settings.Name : type.Name;
			}

			return type.Name;
		}

		internal static bool HasCustomName(Type type)
		{
			TypeSettings userSettings = TypeRegistryUserConfig.Instance.TryGetSettings(type);

			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				return true;
			}

			if (ItemSettings.TryGetValue(type, out TypeRegistryItemAttribute settings))
			{
				return !string.IsNullOrEmpty(settings.Name);
			}

			return false;
		}

		internal static string GetNiceName(Type type)
		{
			TypeSettings userSettings = TypeRegistryUserConfig.Instance.TryGetSettings(type);

			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				return userSettings.Name;
			}

			if (ItemSettings.TryGetValue(type, out TypeRegistryItemAttribute settings))
			{
				return !string.IsNullOrEmpty(settings.Name) ? settings.Name : type.GetNiceName();
			}
			
			return type.GetNiceName();
		}

		internal static bool TryGetCustomName(Type type, out string customName)
		{
			TypeSettings userSettings = TypeRegistryUserConfig.Instance.TryGetSettings(type);

			customName = null;

			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Name))
			{
				customName = userSettings.Name;
				return true;
			}

			if (!ItemSettings.TryGetValue(type, out TypeRegistryItemAttribute settings))
			{
				return false;
			}

			if (string.IsNullOrEmpty(settings.Name))
			{
				return false;
			}

			customName = settings.Name;

			return true;
		}

		internal static int GetPriority(Type type)
		{
			int userPriority = TypeRegistryUserConfig.Instance.GetPriority(type);

			if (userPriority != 0)
			{
				return userPriority;
			}

			var priority = 0;

			if (ItemSettings.TryGetValue(type, out TypeRegistryItemAttribute itemSettings))
			{
				priority = itemSettings.Priority;
			}

			return priority;
		}

		internal static bool TryGetIcon(Type type, out SdfIconType icon, out Color? iconColor)
		{
			TypeSettings userSettings = TypeRegistryUserConfig.Instance.TryGetSettings(type);

			icon = SdfIconType.None;
			iconColor = null;

			if (userSettings != null)
			{
				icon = userSettings.Icon;

				if (EditorGUIUtility.isProSkin)
				{
					iconColor = userSettings.DarkIconColor;
				}
				else
				{
					iconColor = userSettings.LightIconColor;
				}
			}

			if (ItemSettings.TryGetValue(type, out TypeRegistryItemAttribute settings))
			{
				if (icon == SdfIconType.None)
				{
					icon = settings.Icon;
				}

				if (iconColor == null)
				{
					if (EditorGUIUtility.isProSkin)
					{
						iconColor = settings.DarkIconColor;
					}
					else
					{
						iconColor = settings.LightIconColor;
					}
				}
			}

			return icon != SdfIconType.None;
		}

		internal static string GetNamespacePath(Type type)
		{
			string ns = type.Namespace;

			if (string.IsNullOrEmpty(ns))
			{
				return ns;
			}

			if (NamespacePaths.TryGetValue(ns, out string path))
			{
				return path;
			}

			return NamespacePaths[ns] = ns.Replace('.', '/');
		}

		internal static string GetCategoryPath(Type type, bool preferNamespaceOverAssemblyCategory)
		{
			TypeSettings userSettings = TypeRegistryUserConfig.Instance.TryGetSettings(type);

			if (userSettings != null && !string.IsNullOrEmpty(userSettings.Category))
			{
				return userSettings.Category;
			}

			if (ItemSettings.TryGetValue(type, out TypeRegistryItemAttribute itemSettings))
			{
				if (!string.IsNullOrEmpty(itemSettings.CategoryPath))
				{
					return itemSettings.CategoryPath;
				}
			}

			if (preferNamespaceOverAssemblyCategory)
			{
				return GetNamespacePath(type);
			}

			AssemblyCategory assemblyCategory = AssemblyUtilities.GetAssemblyCategory(type.Assembly);

			if (assemblyCategory == AssemblyCategory.None)
			{
				return string.Empty;
			}

			if (!CategoryStringMap.ContainsKey(assemblyCategory))
			{
				CategoryStringMap[assemblyCategory] = assemblyCategory.ToString();
			}

			return CategoryStringMap[assemblyCategory];
		}

		internal static bool IsModifiableType(Type type)
		{
			if (type.IsArray)
			{
				return false;
			}
			
			if (IsGeneratedType(type))
			{
				return false;
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(type)) //&& type.GetInterfaces().Length == 0)
			{
				return false;
			}

			if (type.IsGenericType)
			{
				return false;
			}


			return true;
		}

		public static List<Type> GetValidTypesInCategory(AssemblyCategory category)
		{
			var items = new List<Type>(64);

			foreach (Type type in AssemblyUtilities.GetTypes(category))
			{
				if (ValidTypes.Contains(type))
				{
					items.Add(type);
				}
			}

			return items;
		}

		public static List<Type> GetInheritors(Type type)
		{
			TypeCache.TypeCollection potentialInheritors;

			bool isClosedGeneric = type.IsGenericType && !type.IsGenericTypeDefinition;

			if (isClosedGeneric)
			{
				potentialInheritors = TypeCache.GetTypesDerivedFrom(type.GetGenericTypeDefinition());
			}
			else
			{
				potentialInheritors = TypeCache.GetTypesDerivedFrom(type);
			}

			var result = new HashSet<Type>();

			if (isClosedGeneric)
			{
				Type[] typeGenericArgs = type.GetGenericArguments();
				
				foreach (Type systemTypes in SystemTypes)
				{
					if (IsGeneratedType(systemTypes))
					{
						continue;
					}
					
					if (TryGetCompatibleClosedGenericType(type, typeGenericArgs, systemTypes, out Type inheritor))
					{
						result.Add(inheritor);
					}
				}

				foreach (Type potentialInheritor in potentialInheritors)
				{
					if (IsGeneratedType(potentialInheritor))
					{
						continue;
					}
					
					if (TryGetCompatibleClosedGenericType(type, typeGenericArgs, potentialInheritor, out Type inheritor))
					{
						result.Add(inheritor);
					}
				}
			}
			else
			{
				foreach (Type systemType in SystemTypes)
				{
					if (systemType.IsGenericType)
					{
						if (systemType.ImplementsOpenGenericType(type))
						{
							result.Add(systemType);
						}

						continue;
					}
					
					if (type.IsAssignableFrom(systemType))
					{
						result.Add(systemType);
					}
				}

				foreach (Type potentialInheritor in potentialInheritors)
				{
					if (!IsGeneratedType(potentialInheritor))
					{
						result.Add(potentialInheritor);
					}
				}
			}

			if (!IsGeneratedType(type))
			{
				result.Add(type);
			}

			return result.ToList();
		}

		public static List<Type> GetInstantiableInheritors(Type type, bool includeUnityTypes)
		{
			return GetInstantiableInheritors(type, includeUnityTypes, false);
		}
		
		/// <summary>
		/// 
		/// </summary>
		/// <param name="type"></param>
		/// <param name="includeUnityTypes"></param>
		/// <param name="excludeTypesWithoutDefaultConstructor">This is checked using <see cref="TypeExtensions.HasDefaultConstructor"/>.</param>
		/// <returns></returns>
		public static List<Type> GetInstantiableInheritors(Type type, bool includeUnityTypes, bool excludeTypesWithoutDefaultConstructor)
		{
			// Ignore Open Generics.
			if (type.IsGenericType && type.IsGenericTypeDefinition)
			{
				return new List<Type>();
			}
			
			List<Type> result = GetInheritors(type);
			
			for (int i = result.Count - 1; i >= 0; i--)
			{
				Type currentInheritor = result[i];

				bool isExcluded = currentInheritor.IsAbstract || 
										currentInheritor.IsInterface || 
										currentInheritor.IsGenericTypeDefinition;

				isExcluded = isExcluded || (!includeUnityTypes && typeof(UnityEngine.Object).IsAssignableFrom(currentInheritor));

				isExcluded = isExcluded || (excludeTypesWithoutDefaultConstructor && !currentInheritor.HasDefaultConstructor());

				if (isExcluded)
				{
					result.RemoveAt(i);
				}
			}

			return result;
		}

		private static readonly Type CompilerGeneratedAttributeType = typeof(CompilerGeneratedAttribute);

		internal static bool IsGeneratedType(Type type)
		{
			if (type == null)
			{
				return false;
			}

			return type.IsDefined(CompilerGeneratedAttributeType, false) ||
					 type.Name.FastContains('<') ||
					 IsPrivateImplementationDetails(type) ||
					 type.Assembly.IsDynamic();
		}

		internal static bool IsPrivateImplementationDetails(Type type)
		{
			return type.DeclaringType != null && type.DeclaringType.Name.FastStartsWith("<PrivateImplementationDetails>");
		}

		internal static bool TryGetCompatibleClosedGenericType(Type closedGenericType, Type[] closedGenericArgs, Type type, out Type result)
		{
			result = null;

			if (!type.IsGenericType || !type.IsGenericTypeDefinition)
			{
				if (closedGenericType.IsAssignableFrom(type))
				{
					result = type;
					return true;
				}

				return false;
			}

			if (type.TryInferGenericParameters(out Type[] inferredParams, closedGenericArgs))
			{
				Type inferredInheritor = type.MakeGenericType(inferredParams);

				if (closedGenericType.IsAssignableFrom(inferredInheritor))
				{
					result = inferredInheritor;
					return true;
				}
			}

			return false;
		}

		internal enum CheckedInstanceResult
		{
			Success = 0,
			NoTypeDefined = -1,
			NoDefaultCtorFound = -2,
			CreateInstanceFuncFailed = -3
		}

		internal readonly struct CheckedInstance
		{
			public readonly object Instance;
			public readonly CheckedInstanceResult Result;

			public CheckedInstance(object instance)
			{
				this.Instance = instance;
				this.Result = CheckedInstanceResult.Success;
			}

			public CheckedInstance(CheckedInstanceResult result)
			{
				this.Instance = null;
				this.Result = result;
			}
		}

		internal static CheckedInstance CreateCheckedInstance(Type type, InspectorProperty property = null)
		{
			if (type == null)
			{
				Debug.LogError("Attempted to create an instance of a NULL type.");
				return new CheckedInstance(CheckedInstanceResult.NoTypeDefined);
			}

			if (type == typeof(TypeSelectorV2.TypeSelectorNoneValue))
			{
				return new CheckedInstance(null);
			}

			NonDefaultConstructorPreference handleNonDefaultCtors;

			if (property != null)
			{
				var settings = property.GetAttribute<PolymorphicDrawerSettingsAttribute>();

				if (settings != null)
				{
					if (!string.IsNullOrEmpty(settings.CreateInstanceFunction))
					{
						const string NAMED_VALUE = "type";

						ValueResolver<object> createInstanceFunc = ValueResolver.Get<object>(property,
																													settings.CreateInstanceFunction,
																													new NamedValue(NAMED_VALUE, typeof(Type), null));

						if (createInstanceFunc.HasError)
						{
							Debug.LogError(createInstanceFunc.ErrorMessage);
							return new CheckedInstance(CheckedInstanceResult.CreateInstanceFuncFailed);
						}

						createInstanceFunc.Context.NamedValues.Set(NAMED_VALUE, type);

						return new CheckedInstance(createInstanceFunc.GetValue());
					}

					handleNonDefaultCtors = settings.NonDefaultConstructorPreferenceIsSet
														? settings.NonDefaultConstructorPreference
														: GeneralDrawerConfig.Instance.nonDefaultConstructorPreference;
				}
				else
				{
					handleNonDefaultCtors = GeneralDrawerConfig.Instance.nonDefaultConstructorPreference;
				}
			}
			else
			{
				handleNonDefaultCtors = GeneralDrawerConfig.Instance.nonDefaultConstructorPreference;
			}

			bool hasDefaultCtor = type.HasDefaultConstructor();

			if (hasDefaultCtor || handleNonDefaultCtors != NonDefaultConstructorPreference.LogWarning)
			{
				return new CheckedInstance(type.InstantiateDefault(handleNonDefaultCtors == NonDefaultConstructorPreference.PreferUninitialized));
			}

			Debug.LogWarning($"Failed to instantiate {type.Name}, since it has no default constructor.");

			return new CheckedInstance(CheckedInstanceResult.NoDefaultCtorFound);
		}

		private static bool FastStartsWith(this string str, string startsWith)
		{
			if (str.Length < startsWith.Length)
			{
				return false;
			}

			for (var i = 0; i < startsWith.Length; i++)
			{
				if (str[i] != startsWith[i])
				{
					return false;
				}
			}

			return true;
		}

		private static bool FastContains(this string str, char c)
		{
			for (var i = 0; i < str.Length; i++)
			{
				if (str[i] == c)
				{
					return true;
				}
			}

			return false;
		}
	}
}
#endif