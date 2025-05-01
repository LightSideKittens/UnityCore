//-----------------------------------------------------------------------
// <copyright file="OdinInternalEditorFields.cs" company="Sirenix ApS">
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
using Sirenix.Config;
using Sirenix.Reflection.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	/// <summary> Temporary. </summary>
	/// <warning>This implementation <b>will</b> get refactored.</warning>
	internal static class OdinInternalEditorFields
	{
		private const int INVALID_SELECTOR_ID = 0;
		
		public static int PolymorphicFieldHash = nameof(PolymorphicFieldHash).GetHashCode();

		public static UnityEngine.Object UnityObjectField(UnityEngine.Object value, Type objectType, bool allowSceneObjects, bool readOnly = false,
																		  InspectorProperty property = null)
		{
			return UnityObjectField(EditorGUILayout.GetControlRect(), null, value, objectType, allowSceneObjects, readOnly, property);
		}

		public static UnityEngine.Object UnityObjectField(GUIContent label, UnityEngine.Object value,
																		  Type objectType, bool allowSceneObjects, bool readOnly = false, InspectorProperty property = null)
		{
			return UnityObjectField(EditorGUILayout.GetControlRect(), label, value, objectType, allowSceneObjects, readOnly, property);
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="position">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Whether to allow scene objects.</param>
		/// <param name="readOnly">Determines if the Field is read-only.</param>
		/// <param name="property">Will be used for setting and updating the value, this provides a more consistent way to the handle changes.</param>
		/// <remarks>If a property is assigned through the parameters, the return value should not be used for setting the <see cref="PropertyValueEntry{TValue}"/>, the drawer will handle that.</remarks>
		public static UnityEngine.Object UnityObjectField(Rect position, GUIContent label, UnityEngine.Object value,
																		  Type objectType, bool allowSceneObjects, bool readOnly = false, InspectorProperty property = null)
		{
			// NOTE: We don't allocate an ID here for backwards compatibility; since it would double allocate keyboard IDs.
			int selectorId = property != null ? OdinObjectSelectorIds.OBJECT_FIELD : INVALID_SELECTOR_ID;

			return UnityObjectField(property, selectorId, position, label, value, objectType, allowSceneObjects, readOnly, property);
		}

		/// <summary>
		/// Draws a regular Unity ObjectField, but supports labels being nulls, and also adds a small button that will open the object in a new inspector window.
		/// </summary>
		/// <param name="position">Position and size of the field.</param>
		/// <param name="label">The label to use, or null if no label should be used.</param>
		/// <param name="value">The Unity object.</param>
		/// <param name="objectType">The Unity object type. This supports inheritance.</param>
		/// <param name="allowSceneObjects">Whether to allow scene objects.</param>
		/// <param name="readOnly">Determines if the Field is read-only.</param>
		/// <param name="property">Will be used for setting and updating the value, this provides a more consistent way to the handle changes.</param>
		/// <remarks>If a property is assigned through the parameters, the return value should not be used for setting the <see cref="PropertyValueEntry{TValue}"/>, the drawer will handle that.</remarks>
		public static UnityEngine.Object UnityObjectField(object selectorKey, int selectorId, Rect position, GUIContent label, UnityEngine.Object value,
																		  Type objectType, bool allowSceneObjects, bool readOnly = false, InspectorProperty property = null)
		{
			UnityEngine.Object originalValue = value;

			Rect penRect;

			if (GeneralDrawerConfig.Instance.useOldUnityObjectField)
			{
				bool originalValueWasFakeNull = value == null && !object.ReferenceEquals(value, null);

				// This could be added to also support dragging on object fields.
				// value = DragAndDropUtilities.DragAndDropZone(rect, value, objectType, true, true) as UnityEngine.Object;

				penRect = position;
				penRect.x += penRect.width - 38;
				penRect.width = 20;
				SirenixEditorGUI.BeginDrawOpenInspector(penRect, value, SirenixEditorGUI.IndentLabelRect(position, label != null));

				value = label == null
							  ? EditorGUI.ObjectField(position, value, objectType, allowSceneObjects)
							  : EditorGUI.ObjectField(position, label, value, objectType, allowSceneObjects);

				SirenixEditorGUI.EndDrawOpenInspector(penRect, value);

				if (originalValueWasFakeNull && object.ReferenceEquals(value, null))
				{
					value = originalValue;
				}

				return value;
			}
			
			int id = GUIUtility.GetControlID(EditorGUI_Internals.ObjectFieldHash, FocusType.Keyboard, position);

			if (Event.current.rawType == EventType.MouseDown && Event.current.button == 0 && Event.current.IsMouseOver(position))
			{
				GUIUtility.keyboardControl = id;
			}

			// NOTE: When we get rid of the backwards compatibility check above, we can rid of the INVALID_SELECTOR_ID constant.
			selectorId = selectorId == INVALID_SELECTOR_ID ? id : selectorId;

			bool hasProperty = property != null;

			bool wasFakeNull = value == null && !ReferenceEquals(value, null);

			bool isDragging;

			if (!readOnly)
			{
				EditorGUI.BeginChangeCheck();
				{
					value = DragAndDropUtilities.DropZone(position, value, objectType, allowSceneObjects) as UnityEngine.Object;
				}
				if (EditorGUI.EndChangeCheck())
				{
					if (hasProperty)
					{
						UnityEngine.Object capturedValue = value;

						property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = capturedValue; });

						GUIHelper.RequestRepaint();
					}
				}

				int dragId = DragAndDropUtilities.PrevDragAndDropId;

				isDragging = DragAndDropUtilities.IsDragging && DragAndDropUtilities.CurrentDropId == dragId;
			}
			else
			{
				isDragging = false;
			}

			penRect = position;
			penRect.x += penRect.width - 38;
			penRect.width = 20;
			SirenixEditorGUI.BeginDrawOpenInspector(penRect, value, SirenixEditorGUI.IndentLabelRect(position, label != null));

			if (label != null)
			{
				position = EditorGUI.PrefixLabel(position, id, label);
			}
			else
			{
				position = EditorGUI.IndentedRect(position);
			}

			bool isHover = Event.current.IsMouseOver(position);

			Rect fieldButtonPosition = position.AlignRight(position.height);

			if (SirenixEditorGUI.DoButton(fieldButtonPosition, id, out bool isButtonHover, out bool isButtonActive) && !readOnly)
			{
				OdinObjectSelector.Show(position, selectorKey, selectorId, value, objectType, allowSceneObjects, false, property);
			}

			bool hasKeyboardFocus = GUIUtility.keyboardControl == id;

			if (Event.current.type == EventType.MouseDown && isHover)
			{
				GUIUtility.keyboardControl = id;
			}

			if (!readOnly)
			{
					EditorGUI.BeginChangeCheck();

					value = OdinObjectSelector.GetChangedObject(value, selectorKey, selectorId);

					if (EditorGUI.EndChangeCheck())
					{
						if (hasProperty)
						{
							UnityEngine.Object capturedValue = value;
							property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = capturedValue; });
						}

						GUIHelper.RequestRepaint();
					}
			}

			switch (Event.current.type)
			{
				case EventType.Repaint:
					bool lastGUIEnabled = GUI.enabled;

					if (readOnly)
					{
						GUI.enabled = false;
					}

					var previousIconSize = EditorGUIUtility.GetIconSize();
					EditorGUIUtility.SetIconSize(new Vector2(12f, 12f));

					EditorStyles.objectField.Draw(position, GUIContent.none, isHover, false, isDragging, hasKeyboardFocus);

					Rect contentRect = position;
					contentRect.width -= fieldButtonPosition.width;
					contentRect.width -= 20f;

					contentRect = contentRect.Padding(0, 1.5f);
					contentRect.position += new Vector2(0.5f, 0.5f);

					Texture unityIcon = GetUnityIcon(value, objectType);
					string unityLabel = GetUnityLabel(value, objectType, false);

					if (unityIcon == null)
					{
						contentRect.position += new Vector2(2, 0);
					}

					var labelWidth = SirenixGUIStyles.Label.CalcWidth(unityLabel);

					contentRect = contentRect.AddXMin(2f);
					if (labelWidth > contentRect.width)
					{
						var prevLeft = SirenixGUIStyles.Label.padding.left;
						var prevRight = SirenixGUIStyles.Label.padding.right;
						SirenixGUIStyles.Label.padding.left = 2;
						SirenixGUIStyles.Label.padding.right = 0;
						GUI.Label(contentRect.TakeFromRight(12), GUIHelper.TempContent("...", unityLabel), SirenixGUIStyles.Label);
						SirenixGUIStyles.Label.padding.left = prevLeft;
						GUI.Label(contentRect, GUIHelper.TempContent(unityLabel, unityIcon), SirenixGUIStyles.Label);
						SirenixGUIStyles.Label.padding.right = prevRight;
					}
					else
					{
						GUI.Label(contentRect, GUIHelper.TempContent(unityLabel, unityIcon), SirenixGUIStyles.Label);
					}

					EditorStyles_Internal.ObjectFieldButton.Draw(fieldButtonPosition.Padding(-1, 1, 1, 1),
																				Event.current.IsMouseOver(fieldButtonPosition),
																				isButtonHover,
																				isButtonActive,
																				false);

					if (readOnly)
					{
						GUI.enabled = lastGUIEnabled;
					}

					EditorGUIUtility.SetIconSize(previousIconSize);

					break;

				case EventType.KeyDown:
					if (!hasKeyboardFocus || readOnly)
					{
						break;
					}

					switch (Event.current.keyCode)
					{
						case KeyCode.Backspace:
							if (readOnly)
							{
								break;
							}
							
							value = null;

							if (hasProperty)
							{
								property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = null; });
								GUIHelper.RequestRepaint();
							}

							GUI.changed = true;

							Event.current.Use();
							break;

						case KeyCode.Delete:
							if (readOnly)
							{
								break;
							}
							
							if ((Event.current.modifiers & EventModifiers.Shift) != EventModifiers.None)
							{
								break;
							}

							value = null;

							if (hasProperty)
							{
								property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = null; });
								GUIHelper.RequestRepaint();
							}

							GUI.changed = true;

							Event.current.Use();
							break;

						case KeyCode.V:
							if (readOnly || Event.current.modifiers != EventModifiers.Control)
							{
								break;
							}

							value = HandlePasteEvent(value, objectType, property) as UnityEngine.Object;

							break;
					}

					break;
			}

			switch (Event.current.rawType)
			{
				case EventType.MouseDown:
					Rect area = position;

					area.width -= fieldButtonPosition.width;

					bool isMouseOver = Event.current.IsMouseOver(area);

					if (Event.current.button == 0 && isMouseOver && value != null)
					{
						switch (Event.current.clickCount)
						{
							case 1:
								GUIUtility.keyboardControl = id;

								EditorGUIUtility.PingObject(value);

								Event.current.Use();
								break;

							case 2:
								GUIUtility.keyboardControl = id;

								EditorGUIUtility.PingObject(value);

								AssetDatabase.OpenAsset(value);

								GUIHelper.ExitGUI(false);

								Event.current.Use();
								break;
						}
					}

					break;

				case EventType.KeyDown:
					if (!hasKeyboardFocus)
					{
						break;
					}

					switch (Event.current.keyCode)
					{
						case KeyCode.C:
							if (Event.current.modifiers != EventModifiers.Control)
							{
								break;
							}

							HandleCopyEvent(value);

							break;
					}

					break;
			}

			SirenixEditorGUI.EndDrawOpenInspector(penRect, value);

#if false
			if (Event.current.type == EventType.ExecuteCommand)
			{
				if (id == EditorGUIUtility.GetObjectPickerControlID())
				{
					if (Event.current.commandName == ObjectSelector_Internal.OBJECT_SELECTOR_UPDATED_COMMAND)
					{
						value = EditorGUIUtility.GetObjectPickerObject();
						Event.current.Use();
					}
				}
			}
#endif

			if (!readOnly)
			{
				if (OdinObjectSelector.IsReadyToClaim(selectorKey, selectorId))
				{
					if (hasProperty)
					{
						value = (UnityEngine.Object) OdinObjectSelector.ClaimAndAssign(property);
					}
					else
					{
						value = (UnityEngine.Object) OdinObjectSelector.Claim();
					}

					GUI.changed = true;
				}
			}

			if (!readOnly && wasFakeNull && ReferenceEquals(value, null))
			{
				value = originalValue;

				if (hasProperty)
				{
					UnityEngine.Object capturedValue = value;

					property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = capturedValue; });

					GUIHelper.RequestRepaint();
				}
			}

			return value;
		}

		public static object PolymorphicObjectField(object selectorKey, int selectorId,
																  GUIContent label, InspectorProperty property, bool allowSceneObjects, bool readOnly = false, bool showBaseType = true)
		{
			return PolymorphicObjectField(selectorKey, selectorId, EditorGUILayout.GetControlRect(), label, property, allowSceneObjects, readOnly, showBaseType);
		}
		
		public static object PolymorphicObjectField(GUIContent label, InspectorProperty property, bool allowSceneObjects, bool readOnly = false, bool showBaseType = true)
		{
			return PolymorphicObjectField(EditorGUILayout.GetControlRect(),
													label,
													property.ValueEntry.WeakSmartValue,
													property.ValueEntry.TypeOfValue,
													property.ValueEntry.BaseValueType,
													allowSceneObjects,
													!property.ValueEntry.SerializationBackend.SupportsPolymorphism,
													property,
													readOnly,
													showBaseType);
		}

		public static object PolymorphicObjectField(int id, GUIContent label, InspectorProperty property, bool allowSceneObjects,
																  bool readOnly = false, bool showBaseType = true)
		{
			return PolymorphicObjectField(EditorGUILayout.GetControlRect(),
													id,
													GUIUtility.keyboardControl == id,
													label,
													property.ValueEntry.WeakSmartValue,
													property.ValueEntry.TypeOfValue,
													property.ValueEntry.BaseValueType,
													allowSceneObjects,
													!property.ValueEntry.SerializationBackend.SupportsPolymorphism,
													property,
													readOnly,
													showBaseType);
		}

		public static object PolymorphicObjectField(object selectorKey, int selectorId,
																  Rect position, GUIContent label, InspectorProperty property, bool allowSceneObjects,
																  bool readOnly = false, bool showBaseType = true)
		{
			int id = GUIUtility.GetControlID(PolymorphicFieldHash, FocusType.Keyboard, position);

			return PolymorphicObjectField(selectorKey, selectorId, position, id, label, property, allowSceneObjects, readOnly, showBaseType);
		}
		
		public static object PolymorphicObjectField(object selectorKey, int selectorId,
																  Rect position, int id, GUIContent label, InspectorProperty property, bool allowSceneObjects,
																  bool readOnly = false, bool showBaseType = true)
		{
			return PolymorphicObjectField(selectorKey,
													selectorId,
													position,
													id,
													GUIUtility.keyboardControl == id,
													label,
													property.ValueEntry.WeakSmartValue,
													property.ValueEntry.TypeOfValue,
													property.ValueEntry.BaseValueType, 
													allowSceneObjects,
													!property.ValueEntry.SerializationBackend.SupportsPolymorphism,
													property,
													readOnly,
													showBaseType);
		}

		public static object PolymorphicObjectField(Rect position, GUIContent label, InspectorProperty property, bool allowSceneObjects,
																  bool readOnly = false, bool showBaseType = true)
		{
			int id = GUIUtility.GetControlID(PolymorphicFieldHash, FocusType.Keyboard, position);

			return PolymorphicObjectField(position, id, label, property, allowSceneObjects, readOnly, showBaseType);
		}
		
		public static object PolymorphicObjectField(Rect position, int id, GUIContent label, InspectorProperty property,
																  bool readOnly = false, bool showBaseType = true)
		{
			return PolymorphicObjectField(position,
													id,
													GUIUtility.keyboardControl == id,
													label,
													property.ValueEntry.WeakSmartValue,
													property.ValueEntry.TypeOfValue,
													property.ValueEntry.BaseValueType,
													property.GetAttribute<AssetsOnlyAttribute>() == null,
													!property.ValueEntry.SerializationBackend.SupportsPolymorphism,
													property,
													readOnly,
													showBaseType);
		}

		public static object PolymorphicObjectField(GUIContent label, object value, Type objectType, Type baseType, bool readOnly = false,
																  bool showBaseType = true)
		{
			return PolymorphicObjectField(EditorGUILayout.GetControlRect(), label, value, objectType, baseType, true, false, null, readOnly, showBaseType);
		}

		public static object PolymorphicObjectField(Rect position,
																  GUIContent label,
																  object value,
																  Type valueType,
																  Type baseType,
																  bool allowSceneObjects,
																  bool disallowNullValues,
																  InspectorProperty property,
																  bool readOnly = false,
																  bool showBaseType = true)
		{
			int id = GUIUtility.GetControlID(PolymorphicFieldHash, FocusType.Keyboard, position);

			return PolymorphicObjectField(position, id, GUIUtility.keyboardControl == id, label, value, valueType, baseType, allowSceneObjects, disallowNullValues,
													property, readOnly,
													showBaseType);
		}

		// ReSharper disable once UnusedMember.Global -- Used by Wrapper in Utilities
		public static object PolymorphicObjectFieldWrapper(Rect position,
																			object value,
																			Type baseType,
																			bool allowSceneObjects,
																			bool hasKeyboardFocus,
																			int id,
																			bool disallowNullValues = false,
																			bool readOnly = false,
																			bool showBaseType = true,
																			string title = null)
		{
			Type valueType = value == null ? baseType : value.GetType();

			return PolymorphicObjectField(position,
													id,
													hasKeyboardFocus,
													null,
													value,
													valueType,
													baseType,
													allowSceneObjects,
													disallowNullValues,
													null,
													readOnly,
													showBaseType,
													title);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="position"></param>
		/// <param name="id"></param>
		/// <param name="hasKeyboardFocus"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="valueType"></param>
		/// <param name="baseType"></param>
		/// <param name="allowSceneObjects"></param>
		/// <param name="disallowNullValues"></param>
		/// <param name="property"></param>
		/// <param name="readOnly"></param>
		/// <param name="showBaseType"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		/// <remarks>If a property is assigned through the parameters, the return value should not be used for setting the <see cref="PropertyValueEntry{TValue}"/>, the drawer will handle that.</remarks>
		public static object PolymorphicObjectField(Rect position,
																  int id,
																  bool hasKeyboardFocus,
																  GUIContent label,
																  object value,
																  Type valueType,
																  Type baseType,
																  bool allowSceneObjects,
																  bool disallowNullValues,
																  InspectorProperty property,
																  bool readOnly = false,
																  bool showBaseType = true,
																  string title = null)
		{
			int selectorId = property != null ? OdinObjectSelectorIds.POLYMORPHIC_FIELD : id;

			return PolymorphicObjectField(property, selectorId, position, id, hasKeyboardFocus, label, value, valueType, baseType, allowSceneObjects,
													disallowNullValues, property, readOnly, showBaseType, title);
		}

		/// <summary>
		/// TODO
		/// </summary>
		/// <param name="position"></param>
		/// <param name="id"></param>
		/// <param name="hasKeyboardFocus"></param>
		/// <param name="label"></param>
		/// <param name="value"></param>
		/// <param name="valueType"></param>
		/// <param name="baseType"></param>
		/// <param name="allowSceneObjects"></param>
		/// <param name="disallowNullValues"></param>
		/// <param name="property"></param>
		/// <param name="readOnly"></param>
		/// <param name="showBaseType"></param>
		/// <param name="title"></param>
		/// <returns></returns>
		/// <remarks>If a property is assigned through the parameters, the return value should not be used for setting the <see cref="PropertyValueEntry{TValue}"/>, the drawer will handle that.</remarks>
		public static object PolymorphicObjectField(object selectorKey,
																  int selectorId,
																  Rect position,
																  int id,
																  bool hasKeyboardFocus,
																  GUIContent label,
																  object value,
																  Type valueType,
																  Type baseType,
																  bool allowSceneObjects,
																  bool disallowNullValues,
																  InspectorProperty property,
																  bool readOnly = false,
																  bool showBaseType = true,
																  string title = null)
		{
			if (GeneralDrawerConfig.Instance.useOldPolymorphicField)
			{
				if (title != null)
				{
					return OldPolymorphicObjectField(position, value, baseType, title, allowSceneObjects, hasKeyboardFocus, id);
				}

				return OldPolymorphicObjectField(position, value, baseType, allowSceneObjects, hasKeyboardFocus, id);
			}
			
			bool hasProperty = property != null;

			object originalValue = value;

			var valueUnity = value as UnityEngine.Object;
			bool wasFakeNull = valueUnity && valueUnity == null && !ReferenceEquals(valueUnity, null);

			bool isDragging;

			if (!readOnly)
			{
				EditorGUI.BeginChangeCheck();
				{
					value = DragAndDropUtilities.DragAndDropZone(position, value, baseType, true, true, allowSceneObjects);
				}
				if (EditorGUI.EndChangeCheck())
				{
					if (hasProperty)
					{
						object capturedValue = value;

						property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = capturedValue; });

						GUIHelper.RequestRepaint();
					}
				}

				int dragId = DragAndDropUtilities.PrevDragAndDropId;

				isDragging = DragAndDropUtilities.IsDragging && DragAndDropUtilities.CurrentDropId == dragId;

				EditorGUI.BeginChangeCheck();

				value = OdinObjectSelector.GetChangedObject(value, selectorKey, selectorId);

				if (EditorGUI.EndChangeCheck())
				{
					if (hasProperty)
					{
						object capturedValue = value;

						property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = capturedValue; });
					}

					GUIHelper.RequestRepaint();
				}
			}
			else
			{
				isDragging = false;
			}

			bool isNonUnityBase = !typeof(UnityEngine.Object).IsAssignableFrom(baseType);

			if (label != null)
			{
				position = EditorGUI.PrefixLabel(position, id, label);
			}
			else
			{
				position = EditorGUI.IndentedRect(position);
			}

			bool isHover = Event.current.IsMouseOver(position);

			if (Event.current.type == EventType.MouseDown && isHover)
			{
				GUIUtility.keyboardControl = id;
			}

			// Value used for repainting, this needs to be done in-case we go from a regular C# type to a Unity type.
			object visualValue = value;

			if (OdinObjectSelector.IsCurrentSelector(selectorKey, selectorId))
			{
				visualValue = OdinObjectSelector.SelectorObject;
			}

			bool isIllegal = !ReferenceEquals(value, null) && TypeRegistryUserConfig.Instance.IsIllegal(valueType);

			if (isIllegal)
			{
				GUIHelper.PushColor(Color.yellow);
			}

			bool isVisualValueUnityType = typeof(UnityEngine.Object).IsAssignableFrom(visualValue == null ? baseType : visualValue.GetType());

			if (isVisualValueUnityType)
			{
				Rect penRect = position;
				penRect.x += penRect.width - 38;

				// NOTE: account for the dropdown
				if (isNonUnityBase)
				{
					penRect.x -= position.height;
				}

				penRect.width = 20;

				SirenixEditorGUI.BeginDrawOpenInspector(penRect, valueUnity, SirenixEditorGUI.IndentLabelRect(position, label != null));

				Rect dropdownButtonPosition;
				var isDropdownHover = false;

				if (isNonUnityBase)
				{
					dropdownButtonPosition = position.AlignRight(position.height);

					if (SirenixEditorGUI.DoButton(dropdownButtonPosition, out isDropdownHover) && !readOnly)
					{
						OdinObjectSelector.Show(position, selectorKey, selectorId, value, valueType, baseType, allowSceneObjects, disallowNullValues, property);
					}
				}
				else
				{
					dropdownButtonPosition = Rect.zero;
				}

				Rect fieldButtonPosition = position.AlignRight(position.height).SubX(dropdownButtonPosition.width);

				if (SirenixEditorGUI.DoButton(fieldButtonPosition, id, out bool isButtonHover, out bool isButtonActive) && !readOnly)
				{
					OdinObjectSelector.Show(position, selectorKey, selectorId, value, valueType, baseType, allowSceneObjects, disallowNullValues, property, true);
				}

				switch (Event.current.type)
				{
					case EventType.Repaint:
						var visualValueUnity = (UnityEngine.Object) visualValue;

						bool lastGUIEnabled = GUI.enabled;

						if (readOnly)
						{
							GUI.enabled = false;
						}

						EditorStyles.objectField.Draw(position, GUIContent.none, isHover, false, isDragging, hasKeyboardFocus);

						Rect contentRect = position;
						contentRect.width -= dropdownButtonPosition.width;
						contentRect.width -= fieldButtonPosition.width;

						contentRect = contentRect.Padding(0, 1.5f);
						contentRect.position += new Vector2(0.5f, 0.5f);
						
						Rect labelPosition;

						Type iconType = value == null ? baseType : valueType;

						if (isIllegal)
						{
							GetContentPositions(contentRect, out Rect imagePosition, out labelPosition);

							imagePosition = imagePosition.Padding(2).AddX(1);

							SdfIcons.DrawIcon(imagePosition, SdfIconType.ExclamationTriangleFill, Color.yellow);
						}
						else if (TypeRegistry.TryGetIcon(iconType, out SdfIconType icon, out Color? iconColor))
						{
							GetContentPositions(contentRect, out Rect imagePosition, out labelPosition);

							imagePosition = imagePosition.Padding(2).AddX(1);

							if (iconColor != null)
							{
								SdfIcons.DrawIcon(imagePosition, icon, iconColor.Value);
							}
							else
							{
								SdfIcons.DrawIcon(imagePosition, icon);
							}
						}
						else if (visualValueUnity == null)
						{
							GetContentPositions(contentRect, out labelPosition);
						}
						else
						{
							Texture image = GetUnityIcon(visualValueUnity, valueType);

							if (image != null)
							{
								GetContentPositions(contentRect, out Rect imagePosition, out labelPosition);

								GUI.DrawTexture(imagePosition, image, ScaleMode.ScaleToFit);
							}
							else
							{
								GetContentPositions(contentRect, out labelPosition);
							}
						}

						GUI.Label(labelPosition, GetUnityLabel(visualValueUnity, baseType, true));

						if (isNonUnityBase)
						{
							Rect dropdownRect = dropdownButtonPosition.AlignCenter(Math.Min(10, dropdownButtonPosition.width), Math.Min(10, dropdownButtonPosition.height));
							
							if (isDropdownHover)
							{
								SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill);
							}
							else
							{
								if (EditorGUIUtility.isProSkin)
								{
									SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill, new Color(1, 1, 1, 0.5f));
								}
								else
								{
									SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill, new Color(0, 0, 0, 0.5f));
								}
							}
						}

						EditorStyles_Internal.ObjectFieldButton.Draw(fieldButtonPosition.Padding(-1, 1, 1, 1),
																					Event.current.IsMouseOver(fieldButtonPosition),
																					isButtonHover,
																					isButtonActive,
																					false);

						if (readOnly)
						{
							GUI.enabled = lastGUIEnabled;
						}

						break;
				}

				if (Event.current.rawType == EventType.MouseDown)
				{
					Rect area = position;

					area.width -= dropdownButtonPosition.width;
					area.width -= fieldButtonPosition.width;

					bool isMouseOver = Event.current.IsMouseOver(area);

					if (Event.current.button == 0 && isMouseOver && valueUnity != null)
					{
						switch (Event.current.clickCount)
						{
							case 1:
								GUIUtility.keyboardControl = id;

								EditorGUIUtility.PingObject(valueUnity);

								Event.current.Use();
								break;

							case 2:
								GUIUtility.keyboardControl = id;

								EditorGUIUtility.PingObject(valueUnity);

								AssetDatabase.OpenAsset(valueUnity);

								GUIHelper.ExitGUI(false);

								Event.current.Use();
								break;
						}
					}
				}

				SirenixEditorGUI.EndDrawOpenInspector(penRect, valueUnity);
			}
			else
			{
				// NOTE: this is here to maintain consistent IDs when switching between Unity and Polymorphic, since BeginDrawOpenInspector gets a button ID.
				GUIUtility.GetControlID(GUI_Internals.ButtonHash, FocusType.Passive, position);

				Rect dropdownButtonPosition = position.AlignRight(position.height);

				if (SirenixEditorGUI.DoButton(dropdownButtonPosition, out bool isDropdownHover) && !readOnly)
				{
					OdinObjectSelector.Show(position, selectorKey, selectorId, value, valueType, baseType, allowSceneObjects, disallowNullValues, property);
				}

				Rect focusPosition = position;
				focusPosition.width -= dropdownButtonPosition.width;

				if (SirenixEditorGUI.DoButton(focusPosition, id, out isHover, out bool isActive) && !readOnly)
				{
					GUIUtility.keyboardControl = id;
					//OdinObjectSelector.Show(position, selectorKey, selectorId, value, valueType, baseType, allowSceneObjects, disallowNullValues, property);
				}


				switch (Event.current.type)
				{
					case EventType.Repaint:
						bool lastGUIEnabled = GUI.enabled;

						if (readOnly)
						{
							GUI.enabled = false;
						}

						string valueLabelText;

						string baseTypeName = showBaseType ? $" ({TypeRegistry.GetNiceName(baseType)})" : string.Empty;

						if (EditorGUI.showMixedValue)
						{
							valueLabelText = $"— Conflict{baseTypeName}";
						}
						else
						{
							if (title != null)
							{
								valueLabelText = $" {title}";
							}
							else
							{
								valueLabelText = visualValue == null ? $" None{baseTypeName}" : $"{TypeRegistry.GetNiceName(valueType)}{baseTypeName}";
							}
						}

						EditorStyles.objectField.Draw(position, GUIContent.none, isHover, false, isDragging, hasKeyboardFocus);

						Rect dropdownRect = dropdownButtonPosition.AlignCenter(Math.Min(10, dropdownButtonPosition.width), Math.Min(10, dropdownButtonPosition.height));

						if (isDropdownHover)
						{
							SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill);
							//SdfIcons.DrawIcon(position.AlignRight(20).Padding(4, 5, 5, 5), SdfIconType.CaretDownFill);
						}
						else
						{
							if (EditorGUIUtility.isProSkin)
							{
								SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill, new Color(1, 1, 1, 0.5f));
								//SdfIcons.DrawIcon(position.AlignRight(20).Padding(4, 5, 5, 5), SdfIconType.CaretDownFill, new Color(1, 1, 1, 0.5f));
							}
							else
							{
								SdfIcons.DrawIcon(dropdownRect, SdfIconType.CaretDownFill, new Color(0, 0, 0, 0.5f));
								//SdfIcons.DrawIcon(position.AlignRight(20).Padding(4, 5, 5, 5), SdfIconType.CaretDownFill, new Color(0, 0, 0, 0.5f));
							}
						}

#if true
						position.width -= dropdownButtonPosition.width;

						Rect labelPosition;

						Type iconType = value == null ? baseType : valueType;

						if (isIllegal)
						{
							GetContentPositions(position, out Rect iconPosition, out labelPosition);

							iconPosition = iconPosition.Padding(2).AddX(1);

							SdfIcons.DrawIcon(iconPosition, SdfIconType.ExclamationTriangleFill, Color.yellow);
						}
						else if (TypeRegistry.TryGetIcon(iconType, out SdfIconType icon, out Color? iconColor))
						{
							GetContentPositions(position, out Rect iconPosition, out labelPosition);

							iconPosition = iconPosition.Padding(2).AddX(1);

							if (iconColor != null)
							{
								SdfIcons.DrawIcon(iconPosition, icon, iconColor.Value);
							}
							else
							{
								SdfIcons.DrawIcon(iconPosition, icon);
							}
						}
						else if (value == null)
						{
							GetContentPositions(position, out labelPosition);
						}
						else
						{
							GetContentPositions(position, out Rect iconPosition, out labelPosition);

							iconPosition = iconPosition.Padding(2).AddX(1);

							SdfIcons.DrawIcon(iconPosition, SdfIconType.PuzzleFill);
						}

						GUI.Label(labelPosition, valueLabelText);
#else
						var padding = EditorStyles.popup.padding;
						position = position.Padding(padding.left, padding.right, padding.top, padding.bottom);

						if (isIllegal)
						{
							Rect iconRect = position.TakeFromLeft(position.height - 2).Padding(2);

							SdfIcons.DrawIcon(iconRect, SdfIconType.ExclamationTriangleFill, Color.yellow);
						}
						else if (TypeRegistry.TryGetIcon(valueType, out SdfIconType icon, out Color? iconColor))
						{
							Rect iconRect = position.TakeFromLeft(position.height - 2).Padding(2);

							if (iconColor != null)
							{
								SdfIcons.DrawIcon(iconRect, icon, iconColor.Value);
							}
							else
							{
								SdfIcons.DrawIcon(iconRect, icon);
							}
						}
#if true
						else if (value != null)
						{
							Rect iconRect = position.TakeFromLeft(position.height - 2).Padding(2);

							SdfIcons.DrawIcon(iconRect, SdfIconType.PuzzleFill);
						}
#endif

						GUI.Label(position, GUIHelper.TempContent(valueLabelText));
#endif

						if (readOnly)
						{
							GUI.enabled = lastGUIEnabled;
						}

						break;
				}
			}

			if (hasKeyboardFocus)
			{
				if (Event.current.type == EventType.KeyDown)
				{
					switch (Event.current.keyCode)
					{
						case KeyCode.Backspace:
							if (readOnly)
							{
								break;
							}

							value = null;

							if (hasProperty)
							{
								property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = null; });

								GUIHelper.RequestRepaint();
							}

							GUI.changed = true;

							Event.current.Use();
							break;

						case KeyCode.Delete:
							if (readOnly)
							{
								break;
							}

							if ((Event.current.modifiers & EventModifiers.Shift) != EventModifiers.None)
							{
								break;
							}

							if (hasProperty)
							{
								property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = null; });

								GUIHelper.RequestRepaint();
							}

							value = null;

							GUI.changed = true;

							Event.current.Use();
							break;

						case KeyCode.V:
							if (readOnly || Event.current.modifiers != EventModifiers.Control)
							{
								break;
							}

							value = HandlePasteEvent(value, baseType, property);

							break;
					}
				}

				if (Event.current.rawType == EventType.KeyDown)
				{
					switch (Event.current.keyCode)
					{
						case KeyCode.C:
							if (Event.current.modifiers != EventModifiers.Control)
							{
								break;
							}

							HandleCopyEvent(value);

							break;
					}
				}
			}

			if (!readOnly)
			{
				if (OdinObjectSelector.IsReadyToClaim(selectorKey, selectorId))
				{
					if (hasProperty)
					{
						value = OdinObjectSelector.ClaimAndAssign(property);
					}
					else
					{
						value = OdinObjectSelector.Claim();
					}

					GUI.changed = true;
				}

				if (wasFakeNull && ReferenceEquals(valueUnity, null))
				{
					value = originalValue;
				}
			}

			if (isIllegal)
			{
				GUIHelper.PopColor();
			}

			return value;
		}

		private const int DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT = 30;

		public static object UnityPreviewObjectField(object selectorKey, int selectorId,
																	GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects,
																	float height = DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right, InspectorProperty property = null)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, height);
			return UnityPreviewObjectField(selectorKey, selectorId, rect, label, value, objectType, allowSceneObjects, alignment, property);
		}

		public static object UnityPreviewObjectField(object selectorKey, int selectorId,
																	Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right, InspectorProperty property = null)
		{
			return UnityPreviewObjectField(selectorKey, selectorId, rect, label, value, objectType, alignment, false, true, true, allowSceneObjects, property);
		}

		public static object UnityPreviewObjectField(object selectorKey, int selectorId, GUIContent label, UnityEngine.Object value, Texture preview,
																	Type objectType,
																	bool allowSceneObjects, float height = DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right,
																	InspectorProperty property = null)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, height);
			return UnityPreviewObjectField(selectorKey, selectorId, rect, label, value, preview, objectType, allowSceneObjects, alignment, property);
		}

		public static object UnityPreviewObjectField(object selectorKey, int selectorId, Rect rect, GUIContent label, UnityEngine.Object value, Texture preview,
																	Type objectType,
																	bool allowSceneObjects,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right, InspectorProperty property = null)
		{
			return UnityPreviewObjectField(selectorKey, selectorId, rect, label, value, preview, objectType, alignment, false, true, true, allowSceneObjects,
													 property);
		}
		
		public static object UnityPreviewObjectField(GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects,
																	float height = DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right, InspectorProperty property = null)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, height);
			return UnityPreviewObjectField(rect, label, value, objectType, allowSceneObjects, alignment, property);
		}


		public static object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type objectType, bool allowSceneObjects,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right, InspectorProperty property = null)
		{
			return UnityPreviewObjectField(rect, label, value, objectType, alignment, false, true, true, allowSceneObjects, property);
		}

		public static object UnityPreviewObjectField(GUIContent label, UnityEngine.Object value, Texture preview, Type objectType,
																	bool allowSceneObjects, float height = DEFAULT_PREVIEW_OBJECT_FIELD_HEIGHT,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right,
																	InspectorProperty property = null)
		{
			Rect rect = EditorGUILayout.GetControlRect(label != null, height);
			return UnityPreviewObjectField(rect, label, value, preview, objectType, allowSceneObjects, alignment, property);
		}

		public static object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Texture preview, Type objectType,
																	bool allowSceneObjects,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment =
																		Sirenix.Utilities.Editor.ObjectFieldAlignment.Right, InspectorProperty property = null)
		{
			return UnityPreviewObjectField(rect, label, value, preview, objectType, alignment, false, true, true, allowSceneObjects, property);
		}

		public static object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Texture preview, Type type,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment, bool dragOnly = false,
																	bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true,
																	InspectorProperty property = null)
		{
			var id = DragAndDropUtilities.GetDragAndDropId(rect);

			var selectorId = property == null ? id : OdinObjectSelectorIds.DROP_ZONE_SELECTOR;

			return UnityPreviewObjectField(property, selectorId, id, rect, label, value, preview, type, alignment, dragOnly, allowMove, allowSwap,
													 allowSceneObjects, property);
		}

		public static object UnityPreviewObjectField(object selectorKey, int selectorId, Rect rect, GUIContent label,
																	UnityEngine.Object value, Texture preview, Type type,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment, bool dragOnly = false,
																	bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true,
																	InspectorProperty property = null)
		{
			var id = DragAndDropUtilities.GetDragAndDropId(rect);

			return UnityPreviewObjectField(selectorKey, selectorId, id, rect, label, value, preview, type, alignment, dragOnly, allowMove, allowSwap,
													 allowSceneObjects, property);
		}

		public static object UnityPreviewObjectField(object selectorKey, int selectorId, int id, Rect rect, GUIContent label,
																	UnityEngine.Object value, Texture preview, Type type,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment, bool dragOnly = false,
																	bool allowMove = true, bool allowSwap = true, bool allowSceneObjects = true,
																	InspectorProperty property = null)
		{
			var originalValue = value;
			
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, id, label);
			}
			else
			{
				rect = EditorGUI.IndentedRect(rect);
			}

			Rect popupPosition = rect;

			if (alignment == Sirenix.Utilities.Editor.ObjectFieldAlignment.Left)
			{
				rect = rect.AlignLeft(rect.height);
			}
			else if (alignment == Sirenix.Utilities.Editor.ObjectFieldAlignment.Center)
			{
				rect = rect.AlignCenter(rect.height);
			}
			else
			{
				rect = rect.AlignRight(rect.height);
			}

			DragAndDropUtilities.DrawDropZone(rect, preview, null, id);

			if (!dragOnly)
			{
				if (property != null)
				{
					value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
				}
				else
				{
					value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;
				}

				if (GeneralDrawerConfig.Instance.useOldUnityPreviewField)
				{
					value = OdinInternalDragAndDropUtils.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object;
				}
				else
				{
					value = OdinInternalDragAndDropUtils.ObjectSelectorZone(rect, popupPosition, value, type, allowSceneObjects, id, property,
																							  selectorKey, selectorId) as UnityEngine.Object;
				}
				
				if (GUIUtility.keyboardControl == id)
				{
					if (Event.current.type == EventType.KeyDown)
					{
						switch (Event.current.keyCode)
						{
							case KeyCode.V:
								if (Event.current.modifiers != EventModifiers.Control)
								{
									break;
								}

								value = HandlePasteEvent(value, type, property) as UnityEngine.Object;

								break;
						}
					}

					if (Event.current.rawType == EventType.KeyDown)
					{
						switch (Event.current.keyCode)
						{
							case KeyCode.C:
								if (Event.current.modifiers != EventModifiers.Control)
								{
									break;
								}

								HandleCopyEvent(value);

								break;
						}
					}
				}
			}

			value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;

			if (property != null && originalValue != value)
			{
				UnityEngine.Object capturedValue = value;

				property.Tree.DelayActionUntilRepaint(() =>
				{
					property.ValueEntry.WeakSmartValue = capturedValue;
					GUIHelper.RequestRepaint();
				});

				GUIHelper.RequestRepaint();
			}

			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
				GUIUtility.hotControl = id;
			}

			return value;
		}

		public static object UnityPreviewObjectField(Rect rect, GUIContent label, UnityEngine.Object value, Type type,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment,
																	bool dragOnly = false, bool allowMove = true, bool allowSwap = true,
																	bool allowSceneObjects = true, InspectorProperty property = null)
		{
			var id = DragAndDropUtilities.GetDragAndDropId(rect);

			var selectorId = property == null ? id : OdinObjectSelectorIds.DROP_ZONE_SELECTOR;

			return UnityPreviewObjectField(property, selectorId, id, rect, label, value, type, alignment, dragOnly, allowMove, allowSwap, allowSceneObjects,
													 property);
		}

		public static object UnityPreviewObjectField(object selectorKey, int selectorId, Rect rect,
																	GUIContent label, UnityEngine.Object value, Type type,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment,
																	bool dragOnly = false, bool allowMove = true, bool allowSwap = true,
																	bool allowSceneObjects = true, InspectorProperty property = null)
		{
			var id = DragAndDropUtilities.GetDragAndDropId(rect);

			return UnityPreviewObjectField(selectorKey, selectorId, id, rect, label, value, type, alignment, dragOnly, allowMove, allowSwap, allowSceneObjects,
													 property);
		}

		public static object UnityPreviewObjectField(object selectorKey, int selectorId, int id, Rect rect,
																	GUIContent label, UnityEngine.Object value, Type type,
																	Sirenix.Utilities.Editor.ObjectFieldAlignment alignment,
																	bool dragOnly = false, bool allowMove = true, bool allowSwap = true,
																	bool allowSceneObjects = true, InspectorProperty property = null)
		{
			UnityEngine.Object originalValue = value;
			
			if (label != null)
			{
				rect = EditorGUI.PrefixLabel(rect, id, label);
			}
			else
			{
				rect = EditorGUI.IndentedRect(rect);
			}

			Rect popupPosition = rect;

			if (alignment == Sirenix.Utilities.Editor.ObjectFieldAlignment.Left)
			{
				rect = rect.AlignLeft(rect.height);
			}
			else if (alignment == Sirenix.Utilities.Editor.ObjectFieldAlignment.Center)
			{
				rect = rect.AlignCenter(rect.height);
			}
			else
			{
				rect = rect.AlignRight(rect.height);
			}

			DragAndDropUtilities.DrawDropZone(rect, value, null, id);

			if (!dragOnly)
			{
				value = DragAndDropUtilities.DropZone(rect, value, type, id) as UnityEngine.Object;

				if (GeneralDrawerConfig.Instance.useOldUnityPreviewField)
				{
					value = OdinInternalDragAndDropUtils.ObjectPickerZone(rect, value, type, allowSceneObjects, id) as UnityEngine.Object;
				}
				else
				{
					value = OdinInternalDragAndDropUtils.ObjectSelectorZone(rect, popupPosition, value, type, allowSceneObjects, id, property, selectorKey,
																							  selectorId) as UnityEngine.Object;
				}

				if (GUIUtility.keyboardControl == id)
				{
					if (Event.current.type == EventType.KeyDown)
					{
						switch (Event.current.keyCode)
						{
							case KeyCode.V:
								if (Event.current.modifiers != EventModifiers.Control)
								{
									break;
								}

								value = HandlePasteEvent(value, type, property) as UnityEngine.Object;

								break;
						}
					}

					if (Event.current.rawType == EventType.KeyDown)
					{
						switch (Event.current.keyCode)
						{
							case KeyCode.C:
								if (Event.current.modifiers != EventModifiers.Control)
								{
									break;
								}

								HandleCopyEvent(value);

								break;
						}
					}
				}
			}

			value = DragAndDropUtilities.DragZone(rect, value, type, allowMove, allowSwap, id) as UnityEngine.Object;

			if (property != null && originalValue != value)
			{
				UnityEngine.Object capturedValue = value;

				property.Tree.DelayActionUntilRepaint(() =>
				{
					property.ValueEntry.WeakSmartValue = capturedValue;
					GUIHelper.RequestRepaint();
				});

				GUIHelper.RequestRepaint();
			}

			if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
			{
				GUIUtility.keyboardControl = id;
				GUIUtility.hotControl = id;
			}

			return value;
		}


		public static UnityEngine.Object UnityObjectFieldWrapper(Rect position, GUIContent label, UnityEngine.Object value, Type objectType,
																					bool allowSceneObjects, bool readOnly = false)
		{
			return UnityObjectField(position, label, value, objectType, allowSceneObjects, readOnly);
		}

		private static Texture GetUnityIcon(UnityEngine.Object obj, Type type) => EditorGUIUtility.ObjectContent(obj, type).image;

		private static string GetUnityLabel(UnityEngine.Object obj, Type type, bool useNiceName)
		{
			bool isNull = obj == null;

			string label;

			string typeName = useNiceName ? $" ({type.GetNiceName()})" : $" ({ObjectNames.NicifyVariableName(type.Name)})";
				
			if (EditorGUI.showMixedValue)
			{
				label = $"— Conflict{typeName}";
			}
			else if (isNull)
			{
				if (!ReferenceEquals(obj, null) && obj.GetInstanceID() != 0)
				{
					label = $"Missing{typeName}";
				}
				else
				{
					label = $"None{typeName}";
				}
			}
			else
			{
				GUIContent content = EditorGUIUtility.ObjectContent(obj, type);
				label = content.text;
			}

			return label;
		}

		private static void GetContentPositions(Rect position, out Rect textPosition) => textPosition = position;

		private static void GetContentPositions(Rect position, out Rect iconPosition, out Rect textPosition)
		{
			iconPosition = position.TakeFromLeft(position.height - 2.5f).AddX(1);

			textPosition = position;
		}

		private static void HandleCopyEvent(object value)
		{
			var unityObj = value as UnityEngine.Object;

			if (value == null)
			{
				return;
			}

			if (unityObj)
			{
				if (unityObj == null)
				{
					return;
				}
				
				Clipboard.Copy(unityObj, CopyModes.CopyReference);
			}
			else
			{
				Clipboard.Copy(value, CopyModes.DeepCopy);
			}

			Event.current.Use();
		}

		private static object HandlePasteEvent(object value, Type baseType, InspectorProperty property)
		{
			if (!Clipboard.CanPaste(baseType))
			{
				return value;
			}

			if (property != null)
			{
				if (!property.ValueEntry.IsEditable)
				{
					return value;
				}

				int valueCount = property.ValueEntry.ValueCount;

				if (valueCount <= 0)
				{
					return value;
				}

				value = Clipboard.Paste();

				object capturedValue = value;

				property.Tree.DelayActionUntilRepaint(() =>
				{
					property.ValueEntry.WeakValues[0] = capturedValue;

					for (var i = 1; i < property.ValueEntry.ValueCount; i++)
					{
						property.ValueEntry.WeakValues[i] = Clipboard.Paste();
					}

					GUIHelper.RequestRepaint();
				});

				GUIHelper.RequestRepaint();
			}
			else
			{
				value = Clipboard.Paste();
			}

			Event.current.Use();

			return value;
		}

		public static object OldPolymorphicObjectField(Rect rect, object value, Type type, bool allowSceneObjects, bool hasKeyboardFocus, int id)
		{
			var e = Event.current.type;

			var dropId = DragAndDropUtilities.GetDragAndDropId(rect);
			var penRect = rect;
			var uObj = value as UnityEngine.Object;

			if (uObj)
			{
				penRect.x += penRect.width - 38;
				penRect.width = 20;

				SirenixEditorGUI.BeginDrawOpenInspector(penRect, uObj, rect);
			}

			if (e == EventType.Repaint)
			{
				GUIContent title;
				if (EditorGUI.showMixedValue)
				{
					title = new GUIContent("   " + "— Conflict (" + type.GetNiceName() + ")");
				}
				else if (value == null)
				{
					title = new GUIContent("   " + "Null (" + type.GetNiceName() + ")");
				}
				else if (uObj)
				{
					string baseType = value.GetType() == type ? "" : " : " + type.GetNiceName();
					title = new GUIContent("   " + uObj.name + " (" + value.GetType().GetNiceName() + baseType + ")");
				}
				else
				{
					string baseType = value.GetType() == type ? "" : " : " + type.GetNiceName();
					title = new GUIContent("   " + value.GetType().GetNiceName() + baseType);
				}

				EditorStyles.objectField.Draw(rect, title, id, DragAndDropUtilities.HoveringAcceptedDropZone == dropId);

				if (uObj)
				{
					var thumbnail = GUIHelper.GetAssetThumbnail(uObj, value.GetType(), true);

					if (thumbnail != null)
					{
						GUI.DrawTexture(rect.AlignLeft(rect.height * 0.75f).SetHeight(rect.height * 0.75f).AddX(3).AddY(1.5f), thumbnail);
					}
				}
				else
				{
					if (UnityVersion.IsVersionOrGreater(2019, 3))
					{
						EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height - 3).AlignCenterY(rect.height - 3).AddY(1));
					}
					else
					{
						EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height));
					}
				}
			}

			if (uObj)
			{
				SirenixEditorGUI.EndDrawOpenInspector(penRect, uObj);
			}

			//// Handle Unity dragging manually for now
			//if ((e == EventType.DragUpdated || e == EventType.DragPerform) && rect.Contains(Event.current.mousePosition) && DragAndDrop.objectReferences.Length == 1)
			//{
			//    UnityEngine.Object obj = DragAndDrop.objectReferences[0];

			//    bool accept = false;

			//    if (type.IsAssignableFrom(obj.GetType()))
			//    {
			//        accept = true;
			//    }
			//    else if (obj is GameObject && (type.InheritsFrom(typeof(Component)) || type.IsInterface))
			//    {
			//        obj = (obj as GameObject).GetComponent(type);

			//        if (obj != null)
			//        {
			//            accept = true;
			//        }
			//    }

			//    if (accept)
			//    {
			//        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
			//        Event.current.Use();

			//        if (e == EventType.DragPerform)
			//        {
			//            DragAndDrop.AcceptDrag();
			//            GUI.changed = true;
			//            return obj;
			//        }
			//    }
			//}

			var objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + GUIUtility.GetControlID(FocusType.Passive), type);

			value = DragAndDropUtilities.DropZone(rect, value, type, true, dropId);

			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) ||
				 hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
			{
				var forceOpenPicker = rect.AlignRight(16).Contains(Event.current.mousePosition);

				if (!forceOpenPicker && uObj)
				{
					if (Event.current.clickCount == 1)
					{
						EditorGUIUtility.PingObject(uObj);
					}
					else if (Event.current.clickCount == 2)
					{
						AssetDatabase.OpenAsset(uObj);
					}
				}
				else
				{
					objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
				}

				Event.current.Use();
			}

			if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
			{
				GUI.changed = true;
				return objectPicker.ClaimObject();
			}

			return value;
		}

		public static object OldPolymorphicObjectField(Rect rect, object value, Type type, string title, bool allowSceneObjects, bool hasKeyboardFocus, int id)
		{
			var e = Event.current.type;
			var dropId = DragAndDropUtilities.GetDragAndDropId(rect);
			var penRect = rect;
			var uObj = value as UnityEngine.Object;

			if (uObj)
			{
				penRect.x += penRect.width - 38;
				penRect.width = 20;

				SirenixEditorGUI.BeginDrawOpenInspector(penRect, uObj, rect);
			}

			if (e == EventType.Repaint)
			{
				EditorStyles.objectField.Draw(rect, GUIHelper.TempContent($"   {title}"), id, DragAndDropUtilities.HoveringAcceptedDropZone == dropId);

				if (uObj)
				{
					var thumbnail = GUIHelper.GetAssetThumbnail(uObj, value.GetType(), true);

					if (thumbnail != null)
					{
						GUI.DrawTexture(rect.AlignLeft(rect.height * 0.75f).SetHeight(rect.height * 0.75f).AddX(3).AddY(1.5f), thumbnail);
					}
				}
				else
				{
					if (UnityVersion.IsVersionOrGreater(2019, 3))
					{
						EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height - 3).AlignCenterY(rect.height - 3).AddY(1));
					}
					else
					{
						EditorIcons.StarPointer.Draw(rect.AlignLeft(rect.height));
					}
				}
			}

			if (uObj)
			{
				SirenixEditorGUI.EndDrawOpenInspector(penRect, uObj);
			}

			var objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + GUIUtility.GetControlID(FocusType.Passive), type);

			value = DragAndDropUtilities.DropZone(rect, value, type, true, dropId);

			if (Event.current.type == EventType.MouseDown && Event.current.button == 0 && rect.Contains(Event.current.mousePosition) ||
				 hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
			{
				var forceOpenPicker = rect.AlignRight(16).Contains(Event.current.mousePosition);

				if (!forceOpenPicker && uObj)
				{
					if (Event.current.clickCount == 1)
					{
						EditorGUIUtility.PingObject(uObj);
					}
					else if (Event.current.clickCount == 2)
					{
						AssetDatabase.OpenAsset(uObj);
					}
				}
				else
				{
					objectPicker.ShowObjectPicker(value, allowSceneObjects, rect);
				}

				Event.current.Use();
			}

			if (objectPicker.IsReadyToClaim && Event.current.type == EventType.Repaint)
			{
				GUI.changed = true;
				return objectPicker.ClaimObject();
			}

			return value;
		}
	}
}
#endif