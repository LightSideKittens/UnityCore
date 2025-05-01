//-----------------------------------------------------------------------
// <copyright file="TypeRegistryUserConfig.cs" company="Sirenix ApS">
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
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

// TODO: Currently we don't handle naming collisions,
// This could probably just be handled on the Type Selector by doing stuff such as: A, A (1), A (2), A (3) etc.
namespace Sirenix.Config
{
#pragma warning disable

	public class TypeSettings
	{
		public string Name;
		public string Category;
		public SdfIconType Icon;
		public Color? LightIconColor;
		public Color? DarkIconColor;

		public bool IsDefault() =>
			string.IsNullOrEmpty(this.Name) && string.IsNullOrEmpty(this.Category) && this.Icon == SdfIconType.None &&
			(!this.LightIconColor.HasValue || this.LightIconColor.Value.a == 0.0f) &&
			(!this.DarkIconColor.HasValue || this.DarkIconColor.Value.a == 0.0f);
	}

	[Serializable]
	[HideLabel]
	[InlineProperty]
	[HideReferenceObjectPicker]
	public abstract class StringSerializedCollection<T, TElement> where T : ICollection<TElement>, new()
	{
		[ShowInInspector]
		[NonSerialized]
		[LabelText("@$property.Parent.NiceName")]
		public T Collection = new T();

		[HideInInspector]
		[SerializeField]
		public List<string> serializedCollection = new List<string>();
	}

	[Serializable]
	[HideLabel]
	[InlineProperty]
	[HideReferenceObjectPicker]
	public class SerializedTypeSettingsDictionary : ISerializationCallbackReceiver
	{
		[Serializable]
		public class SerializedData
		{
			public string typeBound;
			public string displayName;
			public string category;
			public SdfIconType sdfIconType;
			public Color lightColor;
			public Color darkColor;
		}

		[ShowInInspector]
		[NonSerialized]
		[LabelText("@$property.Parent.NiceName")]
		public Dictionary<Type, TypeSettings> Dictionary = new Dictionary<Type, TypeSettings>();

		[HideInInspector]
		[SerializeField]
		public List<SerializedData> serializedDictionary = new List<SerializedData>();

		public void OnBeforeSerialize()
		{
			this.serializedDictionary.Clear();

			foreach (KeyValuePair<Type, TypeSettings> kvp in this.Dictionary)
			{
				TypeSettings data = kvp.Value;

				var serializedData = new SerializedData
				{
					typeBound = TwoWaySerializationBinder.Default.BindToName(kvp.Key),
					displayName = data.Name,
					category = data.Category,
					sdfIconType = data.Icon,
					lightColor = data.LightIconColor ?? Color.clear,
					darkColor = data.DarkIconColor ?? Color.clear
				};

				this.serializedDictionary.Add(serializedData);
			}
		}

		public void OnAfterDeserialize()
		{
			if (this.Dictionary.Count > 0)
			{
				return;
			}

			foreach (SerializedData serializedData in this.serializedDictionary)
			{
				Type type = TwoWaySerializationBinder.Default.BindToType(serializedData.typeBound);

				var data = new TypeSettings
				{
					Name = serializedData.displayName,
					Category = serializedData.category,
					Icon = serializedData.sdfIconType,
					LightIconColor = serializedData.lightColor != Color.clear ? serializedData.lightColor : (Color?) null,
					DarkIconColor = serializedData.darkColor != Color.clear ? serializedData.darkColor : (Color?) null,
				};

				this.Dictionary[type] = data;
			}
		}
	}

	[Serializable]
	[HideLabel]
	[InlineProperty]
	[HideReferenceObjectPicker]
	public class SerializedTypePriorityDictionary : ISerializationCallbackReceiver
	{
		[Serializable]
		public class SerializedData
		{
			public string typeName;
			public int priority;
		}

		[ShowInInspector]
		[NonSerialized]
		[LabelText("@$property.Parent.NiceName")]
		public Dictionary<Type, int> Dictionary = new Dictionary<Type, int>();

		[HideInInspector]
		[SerializeField]
		public List<SerializedData> serializedDictionary = new List<SerializedData>();

		public void OnBeforeSerialize()
		{
			this.serializedDictionary.Clear();

			foreach (KeyValuePair<Type, int> kvp in this.Dictionary)
			{
				var serializedData = new SerializedData
				{
					typeName = TwoWaySerializationBinder.Default.BindToName(kvp.Key),
					priority = kvp.Value
				};

				this.serializedDictionary.Add(serializedData);
			}
		}

		public void OnAfterDeserialize()
		{
			if (this.Dictionary.Count > 0)
			{
				return;
			}

			foreach (SerializedData serializedData in this.serializedDictionary)
			{
				Type type = TwoWaySerializationBinder.Default.BindToType(serializedData.typeName);

				int priority = serializedData.priority;

				this.Dictionary[type] = priority;
			}
		}
	}

	[Serializable]
	public abstract class SerializedTypeCollection<T> : StringSerializedCollection<T, Type>, ISerializationCallbackReceiver where T : ICollection<Type>, new()
	{
		public void OnBeforeSerialize()
		{
			this.serializedCollection.Clear();

			foreach (Type type in this.Collection)
			{
				this.serializedCollection.Add(TwoWaySerializationBinder.Default.BindToName(type));
			}
		}

		public void OnAfterDeserialize()
		{
			if (this.Collection.Count > 0)
			{
				return;
			}

			foreach (string s in this.serializedCollection)
			{
				this.Collection.Add(TwoWaySerializationBinder.Default.BindToType(s));
			}
		}
	}

	[Serializable]
	public class SerializedStringHashSet : StringSerializedCollection<HashSet<string>, string>, ISerializationCallbackReceiver
	{
		public void OnBeforeSerialize()
		{
			this.serializedCollection.Clear();

			foreach (string s in this.Collection)
			{
				this.serializedCollection.Add(s);
			}
		}

		public void OnAfterDeserialize()
		{
			if (this.Collection.Count > 0)
			{
				return;
			}

			foreach (string s in this.serializedCollection)
			{
				this.Collection.Add(s);
			}
		}
	}

	[Serializable]
	public class SerializedTypeHashSet : SerializedTypeCollection<HashSet<Type>> { }

	[Serializable]
	public class SerializedTypeList : SerializedTypeCollection<List<Type>> { }
	
	[SirenixEditorConfig]
	public class TypeRegistryUserConfig : GlobalConfig<TypeRegistryUserConfig>
	{
		public HashSet<Type> IllegalTypes => this.addedIllegalTypes.Collection;

		[SerializeField]
		public SerializedTypeHashSet shownTypes = new SerializedTypeHashSet();

		[SerializeField]
		public SerializedTypeHashSet hiddenTypes = new SerializedTypeHashSet();

		[SerializeField]
		public SerializedTypeHashSet addedIllegalTypes = new SerializedTypeHashSet();

		[SerializeField]
		public SerializedTypeSettingsDictionary typeSettings = new SerializedTypeSettingsDictionary();

		[SerializeField]
		public SerializedTypePriorityDictionary typePriorities = new SerializedTypePriorityDictionary();

		public void OpenEditor() => EditorWindow.GetWindow<TypeRegistryUserConfigWindow>();

		public void SetVisibility(Type type, bool isVisible)
		{
			bool isDefaultHidden = TypeRegistry.HiddenTypes.Contains(type);

			if (isDefaultHidden)
			{
				if (isVisible)
				{
					this.shownTypes.Collection.Add(type);
				}
				else
				{
					this.shownTypes.Collection.Remove(type);
				}
			}
			else
			{
				if (isVisible)
				{
					this.hiddenTypes.Collection.Remove(type);
				}
				else
				{
					this.hiddenTypes.Collection.Add(type);
				}
			}

			EditorUtility.SetDirty(this);
		}

		public bool IsVisible(Type type)
		{
			if (this.shownTypes.Collection.Contains(type))
			{
				return true;
			}

			return !TypeRegistry.HiddenTypes.Contains(type) && !this.hiddenTypes.Collection.Contains(type);
		}

		public bool IsIllegal(Type type) => this.addedIllegalTypes.Collection.Contains(type);

		public void SetIllegal(Type type, bool value)
		{
			if (value)
			{
				this.addedIllegalTypes.Collection.Add(type);
			}
			else
			{
				this.addedIllegalTypes.Collection.Remove(type);
			}

			EditorUtility.SetDirty(this);
		}

		public TypeSettings TryGetSettings(Type type) => this.typeSettings.Dictionary.ContainsKey(type) ? this.typeSettings.Dictionary[type] : null;

		public int GetPriority(Type type) => this.typePriorities.Dictionary.ContainsKey(type) ? this.typePriorities.Dictionary[type] : 0;

		public bool IsModified(Type type)
		{
			return this.shownTypes.Collection.Contains(type) ||
					 this.hiddenTypes.Collection.Contains(type) ||
					 this.addedIllegalTypes.Collection.Contains(type) ||
					 this.typePriorities.Dictionary.ContainsKey(type) ||
					 this.typeSettings.Dictionary.ContainsKey(type);
		}

		public void SetSettings(Type type, TypeSettings value) => this.typeSettings.Dictionary[type] = value;

		public void RemoveSettings(Type type) => this.typeSettings.Dictionary.Remove(type);

		public void HandleDefaultSettings(Type type, TypeSettings settings, TypeRegistryItemAttribute itemAttribute)
		{
			if (settings.IsDefault())
			{
				bool wasRemoved = this.typeSettings.Dictionary.Remove(type);

				if (wasRemoved)
				{
					EditorUtility.SetDirty(this);
				}
			}
			else
			{
				bool hasDefaultSettings = itemAttribute != null;

				if (hasDefaultSettings)
				{
					if (settings.Name == null || (!string.IsNullOrEmpty(itemAttribute.Name) && settings.Name == itemAttribute.Name))
					{
						settings.Name = string.Empty;
					}

					if (settings.Category == null || (!string.IsNullOrEmpty(itemAttribute.CategoryPath) && settings.Category == itemAttribute.CategoryPath))
					{
						settings.Category = string.Empty;
					}

					if (settings.Icon == itemAttribute.Icon)
					{
						settings.Icon = SdfIconType.None;
					}

					if (itemAttribute.DarkIconColor.HasValue && settings.DarkIconColor.HasValue)
					{
						if (settings.DarkIconColor.Value == itemAttribute.DarkIconColor.Value)
						{
							settings.DarkIconColor = null;
						}
					}

					if (itemAttribute.LightIconColor.HasValue && settings.LightIconColor.HasValue)
					{
						if (settings.LightIconColor.Value == itemAttribute.LightIconColor.Value)
						{
							settings.LightIconColor = null;
						}
					}
				}
				else
				{
					if (settings.Name == null || settings.Name == type.Name)
					{
						settings.Name = string.Empty;
					}
				}

				if (settings.IsDefault())
				{
					bool wasRemoved = this.typeSettings.Dictionary.Remove(type);

					if (wasRemoved)
					{
						EditorUtility.SetDirty(this);
					}
				}
				else
				{
					EditorUtility.SetDirty(this);
				}
			}
		}


		public void SetPriority(Type type, int value, TypeRegistryItemAttribute itemAttribute)
		{
			if (value == 0 || (itemAttribute != null && value == itemAttribute.Priority))
			{
				bool removed = this.typePriorities.Dictionary.Remove(type);

				if (removed)
				{
					EditorUtility.SetDirty(this);
				}
				
				return;
			}

			this.typePriorities.Dictionary[type] = value;

			EditorUtility.SetDirty(this);
		}

		public void ResetType(Type type)
		{
			this.RemoveSettings(type);
			this.hiddenTypes.Collection.Remove(type);
			this.shownTypes.Collection.Remove(type);
			this.addedIllegalTypes.Collection.Remove(type);
			this.typePriorities.Dictionary.Remove(type);

			EditorUtility.SetDirty(this);
		}
	}
}
#endif