//-----------------------------------------------------------------------
// <copyright file="OdinObjectSelector.cs" company="Sirenix ApS">
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
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.ValueResolvers;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using NamedValue = Sirenix.OdinInspector.Editor.ValueResolvers.NamedValue;
using SerializationUtility = Sirenix.Serialization.SerializationUtility; // Important for later Unity versions.

namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

	public static class OdinObjectSelector
	{
		private struct ReopenInfo
		{
			public static ReopenInfo None => new ReopenInfo(null, null, false);

			public bool IsNone => this.Type == null && this.BaseType == null;

			public Type Type;
			public Type BaseType;
			public bool AllowSceneObjects;

			public ReopenInfo(Type type, Type baseType, bool allowSceneObjects)
			{
				this.Type = type;
				this.BaseType = baseType;
				this.AllowSceneObjects = allowSceneObjects;
			}
		}

		/// <summary>
		/// The <see cref="InspectorProperty"/> that was used in the last 'Show' call.
		/// </summary>
		internal static InspectorProperty SelectorProperty { get; private set; }

		/// <summary>
		/// The key to identify who called the selector.
		/// </summary>
		public static object SelectorKey { get; private set; }

		/// <summary>
		/// The id to identify who called the selector.
		/// </summary>
		public static int SelectorId { get; private set; }

		/// <summary>
		/// The current selected object.
		/// </summary>
		public static object SelectorObject { get; private set; }

		/// <summary>
		/// True if <see cref="UnityEditor.ObjectSelector"/> is used; otherwise false.
		/// </summary>
		internal static bool IsUnityObjectSelector { get; private set; }

		public static bool IsOpen { get; private set; }
		public static bool GenericTypeBuilded { get; private set; }
		internal static bool IsSelectorReadyToClaim { get; private set; }

		// ====== Context required for handling Components in the Unity Object Selector.

		/// <summary>
		/// The type of the value used in the last 'Show' call.
		/// </summary>
		internal static Type SelectorValueType { get; private set; }

		/// <summary>
		/// The base type of the value used in the last 'Show' call.
		/// </summary>
		internal static Type SelectorBaseType { get; private set; }

		// Context required for handling Components in the Unity Object Selector. ======

		private static EventType showedInEvent = EventType.Ignore;

		private static bool wasObjectChanged;

		private static ReopenInfo reopenInfo;

		// TODO: weird name
		private static bool IsCurrentEventValid()
		{
			// In-case the selector was opened in Layout or !Layout event, we need to ensure the current event is the same,
			// since the id could have shifted between (e.g.) Layout-->Repaint.

			switch (showedInEvent)
			{
				case EventType.Ignore:
					return false;

				case EventType.Layout:
					return Event.current.type == EventType.Layout;

				default:
					return Event.current.type != EventType.Layout;
			}
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="position">The position where the selector should appear.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key"/>' or '<paramref name="id"/>' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		public static void Show(object key, int id, object value, Type baseType,
										bool allowSceneObjects = true, bool disallowNullValues = false, Rect position = default)
		{
			Type valueType;

			if (value is UnityEngine.Object unityObj && unityObj != null)
			{
				valueType = value.GetType();
			}
			else
			{
				valueType = value == null ? baseType : value.GetType();
			}

			Show(position, key, id, value, valueType, baseType, allowSceneObjects, disallowNullValues, null, false);
		}
		
		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="property">
		/// Used to provide values for all parameters in the full declaration of this method.
		/// It also allows for customization of various stages of the selector with attributes.
		/// This also sets the <see cref="SelectorProperty"/>.
		/// </param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key"/>' or '<paramref name="id"/>' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(object key, int id, InspectorProperty property, bool useUnitySelector = false)
		{
			IPropertyValueEntry entry = property.ValueEntry;

			Type valueType = entry.WeakSmartValue == null ? entry.BaseValueType : entry.TypeOfValue;

			Show(Rect.zero,
				  key,
				  id,
				  entry.WeakSmartValue,
				  valueType,
				  entry.BaseValueType,
				  property.GetAttribute<AssetsOnlyAttribute>() == null,
				  !entry.SerializationBackend.SupportsPolymorphism,
				  property,
				  useUnitySelector);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="position">The position where the selector should appear; can be <see cref="Rect.zero"/>.</param>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="property">
		/// Used to provide values for all parameters in the full declaration of this method.
		/// It also allows for customization of various stages of the selector with attributes.
		/// This also sets the <see cref="SelectorProperty"/>.
		/// </param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key"/>' or '<paramref name="id"/>' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(Rect position, object key, int id, InspectorProperty property, bool allowSceneObjects, bool useUnitySelector = false)
		{
			IPropertyValueEntry entry = property.ValueEntry;

			Type valueType = entry.WeakSmartValue == null ? entry.BaseValueType : entry.TypeOfValue;

			Show(position,
				  key,
				  id,
				  entry.WeakSmartValue,
				  valueType,
				  entry.BaseValueType,
				  allowSceneObjects,
				  !entry.SerializationBackend.SupportsPolymorphism,
				  property,
				  useUnitySelector);
		}
#if false
		/// <summary>
		/// Displays a selector for an object.
		/// </summary>
		/// <param name="key">The Key used to identify the object that opened the selector; this can be null.</param>
		/// <param name="id">The ID used to identify the object that opened the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="baseType">The base type of the value.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector attempts to open for a condition that has not been defined.</exception>
		/// <remarks>
		/// <para>This method cannot be called during <see cref="EventType.Layout"/>.</para>
		/// <para>Either <paramref name="key"/> or <paramref name="id"/> must be set, but both can also be set for a more accurate result.</para>
		/// </remarks>
		public static void Show(object key, int id, object value, Type baseType, bool allowSceneObjects)
		{
			Show(Rect.zero, key, id, value, value == null ? baseType : value.GetType(), baseType, allowSceneObjects, false, null);
		}
#endif

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="SelectorProperty"/>.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key"/>' or '<paramref name="id"/>' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(object key, int id, object value, Type baseType,
										  bool allowSceneObjects = true,
										  bool disallowNullValues = false,
										  InspectorProperty property = null,
										  bool useUnitySelector = false) 
		{
			Show(Rect.zero,
				  key,
				  id,
				  value,
				  value == null ? baseType : value.GetType(),
				  baseType,
				  allowSceneObjects,
				  disallowNullValues,
				  property,
				  useUnitySelector);
		}

		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="valueType">The type of the 'value'.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="SelectorProperty"/>.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key"/>' or '<paramref name="id"/>' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(object key, int id, object value, Type valueType, Type baseType,
										  bool allowSceneObjects = true,
										  bool disallowNullValues = false,
										  InspectorProperty property = null,
										  bool useUnitySelector = false)
		{
			Show(Rect.zero,
				  key,
				  id,
				  value,
				  valueType,
				  baseType,
				  allowSceneObjects,
				  disallowNullValues,
				  property,
				  useUnitySelector);
		}
		
		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="position">The position where the selector should appear; can be <see cref="Rect.zero"/>.</param>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="SelectorProperty"/>.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key"/>' or '<paramref name="id"/>' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(Rect position, object key, int id, object value, Type baseType,
										  bool allowSceneObjects = true,
										  bool disallowNullValues = false,
										  InspectorProperty property = null,
										  bool useUnitySelector = false) 
		{
			Show(position,
				  key,
				  id,
				  value,
				  value == null ? baseType : value.GetType(),
				  baseType,
				  allowSceneObjects,
				  disallowNullValues,
				  property,
				  useUnitySelector);
		}

#if false
		/// <summary>
		/// Displays a selector for an object.
		/// </summary>
		/// <param name="position">The position where the selector should appear.</param>
		/// <param name="key">The Key used to identify the object that opened the selector; this can be null.</param>
		/// <param name="id">The ID used to identify the object that opened the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="type">The base type of the value.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="SelectorProperty"/>.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector attempts to open for a condition that has not been defined.</exception>
		/// <remarks>
		/// <para>This method cannot be called during <see cref="EventType.Layout"/>.</para>
		/// <para>Either <paramref name="key"/> or <paramref name="id"/> must be set, but both can also be set for a more accurate result.</para>
		/// </remarks>
		public static void Show(Rect position, object key, int id, object value, Type type, InspectorProperty property) // <
		{
			Show(position, key, id, value, type, type,
				  property.GetAttribute<AssetsOnlyAttribute>() == null,
				  !property.ValueEntry.SerializationBackend.SupportsPolymorphism,
				  property);
		}
#endif
		
		[Serializable]
		public class GenericTypeBuilder
		{
			[Serializable]
			public struct TypeData
			{
				[OdinSerialize]
				[TypeDrawerSettings(FilterFunc = "Filter")]
				public Type type;
				
				[NonSerialized] public Type arg;

				public bool Filter(Type candidate)
				{
					var special = arg.GenericParameterAttributes &
					              GenericParameterAttributes.SpecialConstraintMask;
					if (special.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) &&
					    candidate.IsValueType)
						return false;
					if (special.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) &&
					    !candidate.IsValueType)
						return false;
					if (special.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint)
					    && candidate.GetConstructor(Type.EmptyTypes) == null)
						return false;
							
					bool violatesTypeConstraint = false;
					foreach (var ctr in arg.GetGenericParameterConstraints())
					{
						if (!ctr.IsAssignableFrom(candidate))
						{
							violatesTypeConstraint = true;
							break;
						}
					}

					if (violatesTypeConstraint)
					{
						return false;
					}
					
					return true;
				}
			}

			private Type baseType;
			[OdinSerialize] 
			[NonSerialized]
			public TypeData[] types;
			public event Action<Type> Builded;
			public event Action NeedClose;
			
			public GenericTypeBuilder(Type baseType)
			{
				this.baseType = baseType;
				var args = baseType.GetGenericArguments();
				types = new TypeData[args.Length];
				for (var i = 0; i < args.Length; i++)
				{
					var typeData = new TypeData();
					typeData.arg = args[i];
					types[i] = typeData;
				}
			}

			[Button]
			public void Build()
			{
				NeedClose();
				Builded(baseType.MakeGenericType(types.Select(x => x.type).ToArray()));
			}
		}

		public class GenericTypeBuilderPopup : PopupWindowContent
		{
			public GenericTypeBuilder builder;
			private PropertyTree tree;
			private Rect position;
			
			public GenericTypeBuilderPopup(Rect position, GenericTypeBuilder builder)
			{
				builder.NeedClose += () =>
				{
					editorWindow.Close();
				};
				this.position = position;
				this.builder = builder;
				tree = PropertyTree.Create(builder, SerializationBackend.Odin);
			}

			public override Vector2 GetWindowSize()
			{
				var size = position.size;
				size.y = 500;
				return size;
			}

			public override void OnGUI(Rect rect)
			{
				tree.Draw(false);
			}

			public override void OnClose()
			{
				tree.Dispose();
				base.OnClose();
			}
		}
		
		/// <summary>
		/// Shows a selector.
		/// </summary>
		/// <param name="position">The position where the selector should appear; can be <see cref="Rect.zero"/>.</param>
		/// <param name="key">The key used to identify who called the selector; this can be null.</param>
		/// <param name="id">The ID used to identify who called the selector; this can be 0.</param>
		/// <param name="value">The current value selected.</param>
		/// <param name="valueType">The type of the 'value'.</param>
		/// <param name="baseType">The base type of the 'value'.</param>
		/// <param name="allowSceneObjects">Determines if scene objects are allowed.</param>
		/// <param name="disallowNullValues">Determines if null values are allowed.</param>
		/// <param name="property">Used for various stages of the selector that can be customized with attributes. This also sets the <see cref="SelectorProperty"/>.</param>
		/// <param name="useUnitySelector">Determines if the default Unity Object Selector should be the one to appear.</param>
		/// <exception cref="NotImplementedException">Thrown if the selector was opened for a case that it didn't expect.</exception>
		/// <remarks>
		/// <para>Either '<paramref name="key"/>' or '<paramref name="id"/>' must be set, but both can also be set for more consistent results.</para>
		/// </remarks>
		internal static void Show(Rect position, object key, int id, object value, Type valueType, Type baseType,
										  bool allowSceneObjects = true,
										  bool disallowNullValues = false,
										  InspectorProperty property = null,
										  bool useUnitySelector = false)
		{
#if SIRENIX_INTERNAL
			Debug.Assert(id != 0 || key != null);
#endif

			IsSelectorReadyToClaim = false;
			SelectorProperty = property;
			SelectorKey = key;
			SelectorId = id;
			SelectorObject = value;
			IsOpen = true;
			IsUnityObjectSelector = false;
			reopenInfo = ReopenInfo.None;
			SelectorValueType = valueType;
			SelectorBaseType = baseType;
			wasObjectChanged = false;
			showedInEvent = Event.current.type;

			if (valueType == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes))
			{
				string searchFilter = SirenixObjectPickerUtilities.GetSearchFilterForPolymorphicType(baseType);

				ShowUnityObjectSelector(value, typeof(UnityEngine.Object), allowSceneObjects, searchFilter, SelectorId, property);

				SirenixObjectPickerUtilities.MoveCaretToEndOfSearchFilter();
				
				return;
			}

			if (typeof(UnityEngine.Object).IsAssignableFrom(baseType) || useUnitySelector)
			{
				if (Event.current.modifiers == EventModifiers.Control)
				{
					SelectorObject = null;
					IsSelectorReadyToClaim = true;

					return;
				}

				ShowUnityObjectSelector(value, baseType, allowSceneObjects, string.Empty, SelectorId, property);

				return;
			}

			if (baseType == typeof(string))
			{
				SelectorObject = "";
				IsSelectorReadyToClaim = true;
				return;
			}

			if (baseType.IsValueType)
			{
				if (disallowNullValues)
				{
					SelectorObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(baseType);
				}
				else
				{
					if (baseType.IsValueType && !baseType.IsPrimitive)
					{
						SelectorObject = Activator.CreateInstance(baseType);
					}
					else
					{
						SelectorObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(baseType);
					}
				}

				IsSelectorReadyToClaim = true;

				return;
			}

			if (typeof(Delegate).IsAssignableFrom(baseType))
			{
				SelectorObject = null;
				IsSelectorReadyToClaim = true;
				return;
			}

			if (baseType.IsClass || baseType.IsInterface)
			{
				if (disallowNullValues)
				{
					if (baseType.IsInterface)
					{
						ResetState();
						Debug.LogError("Property is serialized by Unity, where interfaces are not supported.");
						return;
					}

					if (baseType.IsAbstract)
					{
						ResetState();
						Debug.LogError("Property is serialized by Unity, where abstract classes are not supported.");
						return;
					}

					SelectorObject = UnitySerializationUtility.CreateDefaultUnityInitializedObject(baseType);
					IsSelectorReadyToClaim = true;

					return;
				}

				if (!GeneralDrawerConfig.Instance.useNewObjectSelector)
				{
					if (position.width > 0 && position.height > 0)
					{
						InstanceCreator.Show(baseType, SelectorId, position);
					}
					else
					{
						InstanceCreator.Show(baseType, SelectorId);
					}
				}
				else
				{
					if (Event.current != null && Event.current.modifiers == EventModifiers.Control)
					{
						Type selectedType = GetFirstValidType(baseType, property);

						if (selectedType == null)
						{
							SelectorObject = null;
						}
						else if (typeof(UnityEngine.Object).IsAssignableFrom(selectedType))
						{
							SelectorObject = null;
						}
						else
						{
							TypeRegistry.CheckedInstance checkedInstance = TypeRegistry.CreateCheckedInstance(selectedType, property);

							SelectorObject = checkedInstance.Result == TypeRegistry.CheckedInstanceResult.Success ? checkedInstance.Instance : null;
						}

						IsSelectorReadyToClaim = true;

						Event.current.Use();

						return;
					}

					bool includeUnityTypes = property?.GetAttribute<SerializeReference>() == null;

					List<Type> inheritors = GetInheritors(baseType, includeUnityTypes, property);

					var showDefaultCtorInfo = true;

					if (property != null)
					{
						var settings = property.GetAttribute<PolymorphicDrawerSettingsAttribute>();

						if (settings != null && !string.IsNullOrEmpty(settings.CreateInstanceFunction))
						{
							showDefaultCtorInfo = false;
						}
					}

					var selector = new TypeSelectorV2(inheritors, false, value == null ? null : valueType, null, false, null, null, property)
					{
						useSingleClick = true,
						HideNonDefaultCtorInfo = !showDefaultCtorInfo,
					};

					var hasUnityInheritors = false;

					foreach (Type inheritor in inheritors)
					{
						if (typeof(UnityEngine.Object).IsAssignableFrom(inheritor))
						{
							hasUnityInheritors = true;
							break;
						}
					}

					selector.CategorizeUnityObjects = hasUnityInheritors;
					
					selector.SelectionCancelled += ResetState;

					selector.SelectionConfirmed += typesEnumerable =>
					{
						Type[] types = typesEnumerable.ToArray();

						if (types.Length < 1)
						{
							ResetState();

							return;
						}

						for (var i = 0; i < types.Length; i++)
						{
							if (types[i].IsGenericTypeDefinition)
							{
								var scrollPos = selector.SelectionTree.ScrollView.CurrentPosition;
								var builder = new GenericTypeBuilder(types[i]);
		
								builder.Builded += type =>
								{
									TypeRegistry.CheckedInstance instanceResult = TypeRegistry.CreateCheckedInstance(type, property);

									if (instanceResult.Result == TypeRegistry.CheckedInstanceResult.Success)
									{
										GenericTypeBuilded = true;
										SelectorObject = instanceResult.Instance;
										IsSelectorReadyToClaim = true;
									}
								};
								
								var popup = new GenericTypeBuilderPopup(position, builder);
								var p = position.position;
								p.y = scrollPos.y;
								position.position = p;
								PopupWindow.Show(position, popup);
								
								
								return;
							}
							
							if (types[i] == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes) || typeof(UnityEngine.Object).IsAssignableFrom(types[i]))
							{
								reopenInfo = new ReopenInfo(types[i], baseType, allowSceneObjects);
								return;
							}
						}

						TypeRegistry.CheckedInstance instanceResult = TypeRegistry.CreateCheckedInstance(types[0], property);

						if (instanceResult.Result == TypeRegistry.CheckedInstanceResult.Success)
						{
							SelectorObject = instanceResult.Instance;
							IsSelectorReadyToClaim = true;
						}
					};

					if (position.width > 0 && position.height > 0)
					{
						selector.ShowInPopup(position);
					}
					else
					{
						selector.ShowInAux();
					}
				}

				return;
			}

			ResetState();
			throw new NotImplementedException();
		}

		/// <summary>
		/// If the object was changed since the last time this method was called, it returns the changed object (<see cref="SelectorObject"/>); otherwise, it returns the provided value.
		/// </summary>
		/// <param name="value">The current value; the value to return if no change has occurred.</param>
		/// <param name="key">The key to identify who showed the current selector.</param>
		/// <param name="id">The ID to identify who showed the current selector.</param>
		/// <typeparam name="T">The type of object to expect as a return value.</typeparam>
		/// <returns><see cref="SelectorObject"/> if it was marked as changed, otherwise <paramref name="value"/>.</returns>
		public static T GetChangedObject<T>(T value, object key, int id)
		{
			if (!IsOpen)
			{
				return value;
			}

			if (!wasObjectChanged)
			{
				return value;
			}

			if (id != SelectorId || key != SelectorKey)
			{
				return value;
			}

			if (!IsCurrentEventValid())
			{
				return value;
			}

			wasObjectChanged = false;

			GUI.changed = true;
				
			return (T) SelectorObject;
		}

		/// <summary>
		/// Checks if the selector's object is ready to be claimed.
		/// </summary>
		/// <param name="key">The key to identify who showed the current selector.</param>
		/// <param name="id">The ID to identify who showed the current selector.</param>
		/// <returns><c>true</c> if the selector's object (<see cref="SelectorObject"/>) is ready to be claimed; otherwise, <c>false</c>.</returns>
		public static bool IsReadyToClaim(object key, int id)
		{
			if (GenericTypeBuilded)
			{
				return true;
			}
			
			if (!IsOpen)
			{
				return false;
			}

			if (id != SelectorId || key != SelectorKey)
			{
				return false;
			}

			if (!IsCurrentEventValid())
			{
				return false;
			}

			if (!GeneralDrawerConfig.Instance.useNewObjectSelector)
			{
				if (InstanceCreator.ControlID == id && InstanceCreator.HasCreatedInstance)
				{
					object val = InstanceCreator.GetCreatedInstance();

					SelectorObject = val;

					IsSelectorReadyToClaim = true;

					return IsSelectorReadyToClaim;
				}
			}

			if (!IsUnityObjectSelector)
			{
				if (!reopenInfo.IsNone)
				{
					bool isAllUnityInheritors = reopenInfo.Type == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes);

					Type baseTypeToReopen = isAllUnityInheritors ? reopenInfo.BaseType : reopenInfo.Type;

					Show(Rect.zero, SelectorKey, SelectorId, SelectorObject, reopenInfo.Type, baseTypeToReopen, reopenInfo.AllowSceneObjects, false,
						  SelectorProperty);
				}

				return IsSelectorReadyToClaim;
			}

			if (id != EditorGUIUtility.GetObjectPickerControlID())
			{
				return false;
			}

			if (Event.current.type == EventType.ExecuteCommand)
			{
				switch (Event.current.commandName)
				{
					case ObjectSelector_Internal.OBJECT_SELECTOR_CANCELED_COMMAND:
						ResetState();
						Event.current.Use();
						break;

					case ObjectSelector_Internal.OBJECT_SELECTOR_UPDATED_COMMAND:
						UnityEngine.Object currentObject = EditorGUIUtility.GetObjectPickerObject();

						UnityEngine.Object nextObject = currentObject;

						if (currentObject is GameObject gameObject)
						{
							if (SelectorValueType == typeof(UnityEngine.Object) || 
								 SelectorBaseType == typeof(System.Object) || 
								 typeof(GameObject).IsAssignableFrom(SelectorValueType))
							{
								nextObject = currentObject;
							}
							else if (SelectorBaseType == typeof(Component) || SelectorBaseType == typeof(MonoBehaviour))
							{
								Component[] components = gameObject.GetComponents(SelectorBaseType);
								
								if (components.Length > 0)
								{
									nextObject = components[0];
								}
								else
								{
									nextObject = null;
								}
							}
							else if (SelectorValueType == typeof(TypeSelectorV2.TypeSelectorAllUnityTypes))
							{
								nextObject = null;

								Component[] allComponents = gameObject.GetComponents(typeof(Component));

								List<Type> inheritors = GetInheritors(SelectorBaseType, true, SelectorProperty);

								for (var i = 0; i < allComponents.Length; i++)
								{
									for (var j = 0; j < inheritors.Count; j++)
									{
										if (inheritors[j].IsInstanceOfType(allComponents[i]))
										{
											nextObject = allComponents[i];
											break;
										}
									}

									if (nextObject != null)
									{
										break;
									}
								}
							}
							else if (SelectorValueType.IsSubclassOf(typeof(Component)))
							{
								nextObject = gameObject.GetComponent(SelectorValueType);
							}
						}
						else
						{
							nextObject = currentObject;
						}


						if (nextObject == null || SelectorBaseType.IsInstanceOfType(nextObject))
						{
							SelectorObject = nextObject;
							wasObjectChanged = true;
						}

						Event.current.Use();
						break;
					
					case ObjectSelector_Internal.OBJECT_SELECTOR_CLOSED_COMMAND:
						const string OBJECT_SELECTOR_TYPE = "UnityEditor.ObjectSelector, UnityEditor.CoreModule";

						Type selectorType = TwoWaySerializationBinder.Default.BindToType(OBJECT_SELECTOR_TYPE);

						if (selectorType != null)
						{
							const string SELECTION_CANCELED_METHOD = "SelectionCanceled";

							MethodInfo selectionCancelledMethod = selectorType.GetMethod(SELECTION_CANCELED_METHOD, BindingFlags.Static | BindingFlags.Public);

							if (selectionCancelledMethod != null)
							{
								var isSelectionCancelled = (bool) selectionCancelledMethod.Invoke(null, Array.Empty<object>());

								if (isSelectionCancelled)
								{
									ResetState();
								}
								else
								{
									if (IsOpen)
									{
										IsSelectorReadyToClaim = true;
									}
								}

								Event.current.Use();
								break;
							}
#if SIRENIX_INTERNAL
							else if (UnityVersion.IsVersionOrGreater(2021, 3)) // Was technically added in 2021.2.18f1
							{
								Debug.LogError($"SIRENIX: Failed to find method '{selectorType.Name}.{SELECTION_CANCELED_METHOD}'.");
							}
#endif
						}
#if SIRENIX_INTERNAL
						else
						{
							Debug.LogError($"SIRENIX: Failed to find a valid type for '{OBJECT_SELECTOR_TYPE}'.");
						}
#endif

						if (IsOpen)
						{
							IsSelectorReadyToClaim = true;
						}

						Event.current.Use();
						break;
				}
			}

			return IsSelectorReadyToClaim;
		}

		/// <summary>
		/// Determines whether the specified <paramref name="key"/> and <paramref name="id"/> combination match the currently open selectors.
		/// </summary>
		/// <param name="key">The key to match.</param>
		/// <param name="id">The ID to match.</param>
		/// <returns><c>true</c> if the combination is a match; otherwise, <c>false</c>.</returns>
		public static bool IsCurrentSelector(object key, int id) => IsOpen && SelectorKey == key && SelectorId == id;

		/// <summary>
		/// Claims the current <see cref="SelectorObject"/>.
		/// </summary>
		/// <returns>The <see cref="SelectorObject"/> if it is ready to be claimed; otherwise, NULL with an accompanying error message.</returns>
		public static object Claim()
		{
			if (!IsSelectorReadyToClaim)
			{
				Debug.LogError($"Attempted to claim an object, when no object is ready to be claimed (ID: {SelectorId}, Key: {SelectorKey}, Current Object: {SelectorObject}).");

				ResetState();

				return null;
			}

			object result = SelectorObject;

			ResetState();

			return result;
		}

		/// <summary>
		/// Claims the current <see cref="SelectorObject"/> and copies a specified amount (<paramref name="amount"/>) of instances of the <see cref="SelectorObject"/>.
		/// </summary>
		/// <param name="amount">The number of instances to copy.</param>
		/// <returns>
		/// An array of <see cref="SelectorObject"/> copies, where element 0 is always the original value, 
		/// if the <see cref="SelectorObject"/> is ready to be claimed; otherwise, NULL with an accompanying error message.
		/// </returns>
		public static object[] ClaimMultiple(int amount)
		{
			if (!IsSelectorReadyToClaim)
			{
				Debug.LogError($"Attempted to claim an object, when no object is ready to be claimed (ID: {SelectorId}, Key: {SelectorKey}, Current Object: {SelectorObject}).");

				ResetState();

				return null;
			}

			if (amount < 1)
			{
				Debug.LogError($"Attempted to claim {amount} objects.");
				return Array.Empty<object>();
			}

			object pickedObj = SelectorObject;

			ResetState();

			if (amount <= 1)
			{
				return new[] {pickedObj};
			}

			var result = new object[amount];

			result[0] = pickedObj;

			for (var i = 1; i < result.Length; i++)
			{
				result[i] = FastDeepCopier.DeepCopy(pickedObj);
			}

			return result;
		}

		/// <summary>
		/// Claims the current <see cref="SelectorObject"/> and, if ready, assigns it to the specified <see cref="InspectorProperty"/>.
		/// If the <see cref="SelectorObject"/> is not ready to be claimed, an error message will be logged.
		/// </summary>
		/// <param name="property">The <see cref="InspectorProperty"/> to assign the <see cref="SelectorObject"/> to.</param>
		internal static object ClaimAndAssign(InspectorProperty property)
		{
			if (property == null)
			{
				Debug.LogError("Attempted to claim and assign an object to a NULL property.");

				ResetState();

				return null;
			}

			if (!IsSelectorReadyToClaim)
			{
				Debug.LogError($"Attempted to claim an object, when no object is ready to be claimed (ID: {SelectorId}, Key: {SelectorKey}, Current Object: {SelectorObject}).");

				ResetState();

				return null;
			}

			object currentObject = SelectorObject;

			property.Tree.DelayActionUntilRepaint(() =>
			{
				if (property.ValueEntry.WeakValues.Count <= 1)
				{
					property.ValueEntry.WeakSmartValue = currentObject;
				}
				else
				{
					property.ValueEntry.WeakValues[0] = currentObject;

					for (var i = 1; i < property.ValueEntry.WeakValues.Count; i++)
					{
						property.ValueEntry.WeakValues[i] = FastDeepCopier.DeepCopy(currentObject);
					}
				}

				GUIHelper.RequestRepaint();
			});

			GUIHelper.RequestRepaint();
			
			ResetState();

			return currentObject;
		}

		private static void ShowUnityObjectSelector(object obj, Type objType, bool allowSceneObjects, string searchFilter, int controlID, InspectorProperty property)
		{
			IsUnityObjectSelector = true;

			if (!typeof(UnityEngine.Object).IsAssignableFrom(objType))
			{
				if (string.IsNullOrEmpty(searchFilter))
				{
					searchFilter = SirenixObjectPickerUtilities.GetSearchFilterForPolymorphicType(objType);
				}

				objType = typeof(UnityEngine.Object);
			}

			UnityEngine.Object objectBeingEdited = null;

			if (property != null)
			{
				objectBeingEdited = property.Tree.RootProperty.ValueEntry.WeakSmartValue as UnityEngine.Object;
			}

			if (obj is UnityEngine.Object unityObj)
			{
				ObjectSelector_Internal.ShowObjectSelector(unityObj, objType, objectBeingEdited, allowSceneObjects, searchFilter, controlID);
			}
			else
			{
				ObjectSelector_Internal.ShowObjectSelector(null, objType, objectBeingEdited, allowSceneObjects, searchFilter, controlID);
			}

			// NOTE: Don't shift the caret unless we have to.
			if (!string.IsNullOrEmpty(searchFilter))
			{
				SirenixObjectPickerUtilities.MoveCaretToEndOfSearchFilter();
			}
		}

		private static Type GetFirstValidType(Type type, InspectorProperty property)
		{
			// TODO: could be relevant to sort these based on constructors how the user prefers how non default constructors are handled.
			const string NAMED_VALUE = TypeSelectorSettingsAttribute.FILTER_TYPES_FUNCTION_NAMED_VALUE;

			ValueResolver<bool> typeFilterFunction = null;

			if (property != null)
			{
				var settings = property.GetAttribute<TypeSelectorSettingsAttribute>();

				if (settings != null && !string.IsNullOrEmpty(settings.FilterTypesFunction))
				{
					typeFilterFunction = ValueResolver.Get<bool>(property, settings.FilterTypesFunction, new NamedValue(NAMED_VALUE, typeof(Type), null));

					if (typeFilterFunction.HasError)
					{
						Debug.LogWarning(typeFilterFunction.ErrorMessage);
					}
				}
			}

			bool includeUnityTypes = property?.GetAttribute<SerializeReference>() == null;

			List<Type> inheritors = GetInheritors(type, includeUnityTypes, property);
			
			if (inheritors.Count <= 0)
			{
				if (!type.IsAbstract && !type.IsInterface)
				{
					if (typeFilterFunction != null)
					{
						typeFilterFunction.Context.NamedValues.Set(NAMED_VALUE, type);

						if (typeFilterFunction.GetValue())
						{
							return type;
						}
					}
					else
					{
						return type;
					}
				}
			
				return null;
			}

			inheritors.Sort(CompareTypes);

			if (typeFilterFunction == null)
			{
				return inheritors[0];
			}

			for (var i = 0; i < inheritors.Count; i++)
			{
				typeFilterFunction.Context.NamedValues.Set(NAMED_VALUE, inheritors[i]);

				if (typeFilterFunction.GetValue())
				{
					return inheritors[i];
				}
			}

			return null;
		}

		private static List<Type> GetInheritors(Type type, bool includeUnityTypes, InspectorProperty property)
		{
			if (property == null)
			{
				return TypeRegistry.GetInstantiableInheritors(type, includeUnityTypes);
			}

			var settings = property.GetAttribute<PolymorphicDrawerSettingsAttribute>();

			bool excludeTypesWithoutDefaultConstructor;

			if (settings != null )
			{
				if (settings.NonDefaultConstructorPreferenceIsSet)
				{
					excludeTypesWithoutDefaultConstructor = settings.NonDefaultConstructorPreference == NonDefaultConstructorPreference.Exclude;
				}
				else
				{
					excludeTypesWithoutDefaultConstructor = GeneralDrawerConfig.Instance.nonDefaultConstructorPreference == NonDefaultConstructorPreference.Exclude;
				}

				if (!string.IsNullOrEmpty(settings.CreateInstanceFunction))
				{
					excludeTypesWithoutDefaultConstructor = false;
				}
			}
			else
			{
				excludeTypesWithoutDefaultConstructor = GeneralDrawerConfig.Instance.nonDefaultConstructorPreference == NonDefaultConstructorPreference.Exclude;
			}
			
			

			return TypeRegistry.GetInstantiableInheritors(type, 
																		 includeUnityTypes, 
																		 excludeTypesWithoutDefaultConstructor);
		}

		private static int CompareTypes(Type self, Type other)
		{
			int priorityCmp = TypeRegistry.GetPriority(other).CompareTo(TypeRegistry.GetPriority(self));

			if (priorityCmp != 0)
			{
				return priorityCmp;
			}

			bool isSelfUnity = typeof(UnityEngine.Object).IsAssignableFrom(self);
			bool isOtherUnity = typeof(UnityEngine.Object).IsAssignableFrom(other);

			return isSelfUnity.CompareTo(isOtherUnity);
		}

		private static void ResetState()
		{
			SelectorProperty = null;
			SelectorKey = null;
			SelectorId = 0;
			SelectorObject = null;
			IsOpen = false;
			GenericTypeBuilded = false;
			IsSelectorReadyToClaim = false;
			IsUnityObjectSelector = false;
			reopenInfo = ReopenInfo.None;
			SelectorValueType = null;
			SelectorBaseType = null;
			wasObjectChanged = false;
			showedInEvent = EventType.Ignore;
		}
	}
}
#endif