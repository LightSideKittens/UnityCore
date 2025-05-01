//-----------------------------------------------------------------------
// <copyright file="NullableReferenceDrawer.cs" company="Sirenix ApS">
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
using Sirenix.OdinInspector.Editor.ActionResolvers;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

// NOTE: this using statement is important for later Unity versions
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

	[AllowGUIEnabledForReadonly]
	[DrawerPriority(0, 0, 2000)]
	public sealed class NullableReferenceDrawer<T> : OdinValueDrawer<T>, IDefinesGenericMenuItems
	{
		private bool allowSceneObjects;
		
		private bool ShowBaseType
		{
			get
			{
				if (this.polymorphicSettings == null)
				{
					return GeneralDrawerConfig.Instance.showBaseType;
				}

				if (this.polymorphicSettings.ShowBaseTypeIsSet)
				{
					return this.polymorphicSettings.ShowBaseType;
				}

				return GeneralDrawerConfig.Instance.showBaseType;
			}
		}

		private bool ReadOnlyIfNotNullReference => this.polymorphicSettings?.ReadOnlyIfNotNullReference ?? false;

		private bool shouldDrawReferencePicker;
		private bool drawChildren;
		private bool isValueUnityType;

		private SearchField searchField;
		private PropertySearchFilter searchFilter;

		private InlinePropertyAttribute inlineAttribute;
		private PolymorphicDrawerSettingsAttribute polymorphicSettings;

		private OdinDrawer[] bakedDrawerArray;

		protected override void Initialize()
		{
			this.bakedDrawerArray = this.Property.GetActiveDrawerChain().BakedDrawerArray;

			var searchableAttribute = this.Property.GetAttribute<SearchableAttribute>();

			if (searchableAttribute != null)
			{
				this.searchFilter = new PropertySearchFilter(this.Property, searchableAttribute);
				this.searchField = new SearchField();
			}

			this.isValueUnityType = typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.TypeOfValue);

			this.inlineAttribute = this.Property.Attributes.GetAttribute<InlinePropertyAttribute>();
			this.polymorphicSettings = this.Property.GetAttribute<PolymorphicDrawerSettingsAttribute>();
            this.allowSceneObjects = InspectorPropertyInfoUtility.InspectorPropertySupportsAssigningSceneReferences(this.Property);
		}

		public void PopulateGenericMenu(InspectorProperty property, GenericMenu genericMenu)
		{
			var entry = property.ValueEntry as IPropertyValueEntry<T>;

			bool isReadOnly = entry.ValueState != PropertyValueState.NullReference && ReadOnlyIfNotNullReference;

			bool isChangeable = property.ValueEntry.SerializationBackend.SupportsPolymorphism
									  && !entry.BaseValueType.IsValueType
									  && entry.BaseValueType != typeof(string)
									  && property.GetAttribute<TypeFilterAttribute>() == null;

			if (!GeneralDrawerConfig.Instance.useNewObjectSelector || GeneralDrawerConfig.Instance.useOldPolymorphicField)
			{
				if (isChangeable)
				{
					if (entry.IsEditable && !isReadOnly)
					{
						var objectPicker = ObjectPicker.GetObjectPicker(entry, entry.BaseValueType);
						
						var rect = entry.Property.LastDrawnValueRect;
						
						rect.position = GUIUtility.GUIToScreenPoint(rect.position);
						
						rect.height = 20;
						genericMenu.AddItem(new GUIContent("Change Type"), false, () => { objectPicker.ShowObjectPicker(entry.WeakSmartValue, false, rect); });
					}
					else
					{
						genericMenu.AddDisabledItem(new GUIContent("Change Type"));
					}
				}

				return;
			}
			
			if (!isChangeable)
			{
				return;
			}

			bool isSystemTypeDrawer = this.ValueEntry.BaseValueType == typeof(Type);

			if (!isSystemTypeDrawer)
			{
				if (entry.IsEditable && !isReadOnly)
				{
					Rect rect = entry.Property.LastDrawnValueRect;
					rect.position = GUIUtility.GUIToScreenPoint(rect.position);
					rect.height = 20;

					genericMenu.AddItem(new GUIContent("Change Type"), false, () =>
					{
						this.Property.Tree.DelayActionUntilRepaint(() =>
						{
							if (Event.current.type == EventType.Layout)
							{
								return;
							}

							Rect popupPosition = this.Property.LastDrawnValueRect;

							popupPosition = popupPosition.AlignCenter(600);

							// NOTE: We provide the same Selector ID the drawers would provide, to ensure we don't have to do a bunch of custom code to handle assignment
							int selectorId;

#if true
							selectorId = OdinObjectSelectorIds.ODIN_DRAWER_FIELD;
#else
							if (typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.BaseValueType) && !this.ValueEntry.BaseValueType.IsInterface)
							{
								if (this.Property.GetAttribute<PreviewFieldAttribute>() == null)
								{
									selectorId = OdinObjectSelectorIds.OBJECT_FIELD;
								}
								else
								{
									selectorId = OdinObjectSelectorIds.PREVIEW_OBJECT_FIELD;
								}
							}
							else
							{
								if (!this.ValueEntry.BaseValueType.IsInterface &&
									 !typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.BaseValueType) &&
									 typeof(object) != this.ValueEntry.BaseValueType &&
									 this.ValueEntry.WeakSmartValue != null &&
									 typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.TypeOfValue) &&
									 this.Property.GetAttribute<PreviewFieldAttribute>() != null)
								{
									selectorId = OdinObjectSelectorIds.PREVIEW_OBJECT_FIELD;
								}
								else
								{
									selectorId = OdinObjectSelectorIds.POLYMORPHIC_FIELD;
								}
							}
#endif

							OdinObjectSelector.Show(popupPosition, this.Property, selectorId, this.Property, this.allowSceneObjects);
						});
						GUIHelper.RequestRepaint();
					});
				}
				else
				{
					genericMenu.AddDisabledItem(new GUIContent("Change Type"));
				}
			}

			Type currentType;

			if (isSystemTypeDrawer)
			{
				currentType = this.ValueEntry.WeakSmartValue == null ? this.ValueEntry.BaseValueType : this.ValueEntry.WeakSmartValue as Type;
			}
			else
			{
				currentType = this.ValueEntry.WeakSmartValue == null ? this.ValueEntry.BaseValueType : this.ValueEntry.TypeOfValue;
			}

			if (TypeRegistry.IsModifiableType(currentType))
			{
				genericMenu.AddItem(new GUIContent("Customize Type"), false, () =>
				{
					var window = EditorWindow.GetWindow<TypeRegistryUserConfigWindow>();

					if (this.ValueEntry.WeakSmartValue == null)
					{
						window.TypeToScrollTo = this.ValueEntry.BaseValueType;
					}
					else
					{
						if (this.ValueEntry.WeakSmartValue is Type type)
						{
							window.TypeToScrollTo = type;
						}
						else
						{
							window.TypeToScrollTo = this.ValueEntry.TypeOfValue;
						}
					}
				});
			}
			else
			{
				genericMenu.AddDisabledItem(new GUIContent("Customize Type"));
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			if (this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				this.CallNextDrawer(label);
			}
			
			if (Event.current.type == EventType.Layout)
			{
				this.shouldDrawReferencePicker = ShouldDrawReferenceObjectPicker(this.ValueEntry);

				this.drawChildren = this.Property.Children.Count > 0;

				if (this.Property.Children.Count > 0)
				{
					this.drawChildren = true;
				}
				else if (this.ValueEntry.ValueState != PropertyValueState.None)
				{
					this.drawChildren = false;
				}
				else
				{
					// NOTE: handling weird edge case from the old NullableReferenceDrawer
					this.drawChildren = this.bakedDrawerArray[this.bakedDrawerArray.Length - 2] != this;
				}

				bool isCurrentObjectSelectorProperty = OdinObjectSelector.SelectorProperty == this.Property;

				if (isCurrentObjectSelectorProperty)
				{
					if (OdinObjectSelector.SelectorObject == null ||
						 (!typeof(UnityEngine.Object).IsAssignableFrom(this.ValueEntry.TypeOfValue) &&
						  typeof(UnityEngine.Object).IsAssignableFrom(OdinObjectSelector.SelectorObject?.GetType())))
					{
						this.drawChildren = false;
					}
				}
			}

			if (this.ValueEntry.ValueState == PropertyValueState.NullReference)
			{
				if (this.isValueUnityType)
				{
					this.CallNextDrawer(label);
				}
				else
				{
					if (!this.ValueEntry.SerializationBackend.SupportsPolymorphism && this.ValueEntry.IsEditable)
					{
						SirenixEditorGUI.ErrorMessageBox("Unity-backed value is null. This should already be fixed by the FixUnityNullDrawer!" +
																	" It is likely that this type has been incorrectly guessed by Odin to be serialized by Unity when it is actually not." +
																	" Please create an issue on Odin's issue tracker stating how to reproduce this error message.");
					}

					this.DrawField(label);
				}
			}
			else
			{
				if (this.shouldDrawReferencePicker)
				{
					this.DrawField(label);
				}
				else
				{
					this.CallNextDrawer(label);
				}
			}

			if (!GeneralDrawerConfig.Instance.useNewObjectSelector || GeneralDrawerConfig.Instance.useOldPolymorphicField)
			{
				var objectPicker = ObjectPicker.GetObjectPicker(this.ValueEntry, this.ValueEntry.BaseValueType);
				if (objectPicker.IsReadyToClaim)
				{
					var obj = objectPicker.ClaimObject();
					this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
					{
						this.ValueEntry.WeakValues[0] = obj;
						for (int j = 1; j < this.ValueEntry.ValueCount; j++)
						{
							// NOTE: "Sirenix.Serialization." is important for later Unity versions
							this.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(obj);
						}
					});
				}
			}

			if (GeneralDrawerConfig.Instance.useNewObjectSelector)
			{
				if (OdinObjectSelector.IsReadyToClaim(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD))
				{
					OdinObjectSelector.ClaimAndAssign(this.Property);
				}
			}
		}

		private void DrawReferencePicker(Rect position, int id)
		{
			bool lastMixedValue = EditorGUI.showMixedValue;

			if (this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
			{
				EditorGUI.showMixedValue = true;
			}

			bool isReadOnly = this.ValueEntry.ValueState != PropertyValueState.NullReference && this.ReadOnlyIfNotNullReference;
			
			if (GeneralDrawerConfig.Instance.useOldPolymorphicField)
			{
				EditorGUI.BeginChangeCheck();
				var prev = EditorGUI.showMixedValue;
				if (this.ValueEntry.ValueState == PropertyValueState.ReferenceValueConflict)
				{
					EditorGUI.showMixedValue = true;
				}

				//var newValue = PolymorphicFieldHandler_WILL_BE_DEPRECATED.DoField(position, id, null, this.Property, isReadOnly, this.ShowBaseType);
				var newValue = OdinInternalEditorFields.PolymorphicObjectField(position, id, null, this.Property, this.allowSceneObjects, isReadOnly, this.ShowBaseType);

				EditorGUI.showMixedValue = prev;

				if (EditorGUI.EndChangeCheck())
				{
					this.ValueEntry.Property.Tree.DelayActionUntilRepaint(() =>
					{
						this.ValueEntry.WeakValues[0] = newValue;
						for (int j = 1; j < this.ValueEntry.ValueCount; j++)
						{
							// NOTE: "Sirenix.Serialization." is important for later Unity versions
							this.ValueEntry.WeakValues[j] = Sirenix.Serialization.SerializationUtility.CreateCopy(newValue);
						}
					});
				}
			}
			else
			{
				//PolymorphicFieldHandler_WILL_BE_DEPRECATED.DoField(position, id, null, this.Property, isReadOnly, this.ShowBaseType);
				OdinInternalEditorFields.PolymorphicObjectField(this.Property, OdinObjectSelectorIds.ODIN_DRAWER_FIELD,
																				position, id, null, this.Property, this.allowSceneObjects, isReadOnly, this.ShowBaseType);
			}

			EditorGUI.showMixedValue = lastMixedValue;
		}

		private void DrawField(GUIContent label)
		{
			if (this.inlineAttribute != null)
			{
				this.DrawFieldInline(label);
				return;
			}

			Rect position = EditorGUILayout.GetControlRect();

			Rect labelPosition = Rect.zero;

			bool hasLabel = label != null && !string.IsNullOrEmpty(label.text);

			if (hasLabel || (this.searchFilter != null && this.drawChildren))
			{
				position = SirenixEditorGUI.PrefixRect(position, out labelPosition);
			}
			else
			{
				position = EditorGUI.IndentedRect(position);
			}

			int id = GUIUtility.GetControlID(OdinInternalEditorFields.PolymorphicFieldHash, FocusType.Keyboard, position);

			GUIHelper.PushIndentLevel(0);
			
			if (this.drawChildren)
			{
				if (hasLabel)
				{
					if (this.searchFilter != null)
					{
						int additionalSize = EditorGUIUtility.hierarchyMode ? 0 : SirenixEditorGUI.FoldoutWidth;

						this.DrawSearchFilter(labelPosition.TakeFromRight(labelPosition.width - EditorStyles.label.CalcWidth(label) - additionalSize));
					}
				
					this.Property.State.Expanded = SirenixEditorGUI.Foldout(ref labelPosition, id, label, this.Property.State.Expanded, true);
				}
				else
				{
					if (this.searchFilter != null)
					{
						this.Property.State.Expanded = SirenixEditorGUI.Foldout(ref labelPosition, id, label, this.Property.State.Expanded, true);
						this.DrawSearchFilter(labelPosition);
					}
					else
					{
						this.Property.State.Expanded = SirenixEditorGUI.Foldout(ref position, id, label, this.Property.State.Expanded, true);
					}
				}
			}
			else if (hasLabel)
			{
				EditorGUI.HandlePrefixLabel(labelPosition, labelPosition, label, id);
			}

			this.DrawReferencePicker(position, id);

			GUIHelper.PopIndentLevel();

			if (this.drawChildren)
			{
				bool toggle = this.ValueEntry.ValueState != PropertyValueState.NullReference && this.Property.State.Expanded;

				if (SirenixEditorGUI.BeginFadeGroup(this, toggle))
				{
					if (this.searchFilter != null && this.searchFilter.HasSearchResults)
					{
						this.searchFilter.DrawSearchResults();
					}
					else
					{
						EditorGUI.indentLevel++;

						if (hasLabel)
						{
							this.CallNextDrawer(null);
						}
						else
						{
							this.CallNextDrawer(null);
						}

						EditorGUI.indentLevel--;
					}
				}

				SirenixEditorGUI.EndFadeGroup();
			}
		}

		private void DrawFieldInline(GUIContent label)
		{
			bool shouldPushLabelWidth = this.inlineAttribute.LabelWidth > 0;

			if (label == null || label == GUIContent.none)
			{
				if (shouldPushLabelWidth)
				{
					GUIHelper.PushLabelWidth(this.inlineAttribute.LabelWidth);
				}

				Rect position = EditorGUILayout.GetControlRect();
				int id = GUIUtility.GetControlID(OdinInternalEditorFields.PolymorphicFieldHash, FocusType.Keyboard, position);

				this.DrawReferencePicker(position, id);

				if (this.drawChildren)
				{
					this.CallNextDrawer(null);
				}

				if (shouldPushLabelWidth)
				{
					GUIHelper.PopLabelWidth();
				}

				return;
			}

			SirenixEditorGUI.BeginVerticalPropertyLayout(label);
			{
				Rect position = EditorGUILayout.GetControlRect();
				int id = GUIUtility.GetControlID(OdinInternalEditorFields.PolymorphicFieldHash, FocusType.Keyboard, position);

				this.DrawReferencePicker(position, id);

				if (shouldPushLabelWidth)
				{
					GUIHelper.PushLabelWidth(this.inlineAttribute.LabelWidth);
				}

				if (this.drawChildren)
				{
					this.CallNextDrawer(null);
				}

				if (shouldPushLabelWidth)
				{
					GUIHelper.PopLabelWidth();
				}

				GUILayout.Space(UnityVersion.IsVersionOrGreater(2019, 3) ? 5 : 4);
			}
			SirenixEditorGUI.EndVerticalPropertyLayout();
		}

		private void DrawSearchFilter(Rect position)
		{
			if (this.searchFilter == null)
			{
				return;
			}

			position = position.Padding(2, 0);

			if (position.width < 16)
			{
				GUI.Label(position, "...");
				return;
			}

			string newTerm = this.searchField.Draw(position, this.searchFilter.SearchTerm, "Find Property...");

			if (newTerm == this.searchFilter.SearchTerm)
			{
				return;
			}

			this.searchFilter.SearchTerm = newTerm;

			this.Property.Tree.DelayActionUntilRepaint(() =>
			{
				if (!string.IsNullOrEmpty(newTerm))
				{
					this.Property.State.Expanded = true;
				}

				this.searchFilter.UpdateSearch();
				GUIHelper.RequestRepaint();
			});
		}

		/// <summary>
		/// Returns a value that indicates if this drawer can be used for the given property.
		/// </summary>
		protected override bool CanDrawValueProperty(InspectorProperty property)
		{
			if (property.IsTreeRoot)
			{
				return false;
			}

			Type type = property.ValueEntry.BaseValueType;

			return (type.IsClass || type.IsInterface) && type != typeof(string) && !typeof(UnityEngine.Object).IsAssignableFrom(type);
		}
		  
		private static bool ShouldDrawReferenceObjectPicker(IPropertyValueEntry<T> entry)
		{
			return entry.SerializationBackend.SupportsPolymorphism
					 && !entry.BaseValueType.IsValueType
					 && entry.BaseValueType != typeof(string)
					 && !(entry.Property.ChildResolver is ICollectionResolver)
					 && !entry.BaseValueType.IsArray
					 && entry.IsEditable
					 && !entry.BaseValueType.InheritsFrom(typeof(IDictionary))
					 && !(entry.WeakSmartValue as UnityEngine.Object)
					 && entry.Property.GetAttribute<HideReferenceObjectPickerAttribute>() == null;
		}
	}
}
#endif