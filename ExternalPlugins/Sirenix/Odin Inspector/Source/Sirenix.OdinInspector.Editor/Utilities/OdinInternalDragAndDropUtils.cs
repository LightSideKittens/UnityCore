//-----------------------------------------------------------------------
// <copyright file="OdinInternalDragAndDropUtils.cs" company="Sirenix ApS">
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
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	/// <summary> Temporary. </summary>
	/// <warning>This implementation <b>will</b> get refactored.</warning>
	internal static class OdinInternalDragAndDropUtils
	{
		public static object ObjectSelectorZone(Rect position,
															 Rect popupPosition,
															 object value,
															 Type type,
															 bool allowSceneObjects,
															 int id,
															 InspectorProperty property,
															 object selectorKey,
															 int selectorId)
		{
			Rect selectRect = position.AlignBottom(15).AlignCenter(45);

			var uObj = value as UnityEngine.Object;

			selectRect.xMin = Mathf.Max(selectRect.xMin, position.xMin);

			bool isMouseOver = Event.current.IsMouseOver(position);
			bool hasKeyboardFocus = GUIUtility.keyboardControl == id;

			bool hide = DragAndDropUtilities.IsDragging || Event.current.type == EventType.Repaint && !isMouseOver;

			if (!hide)
			{
				if (uObj)
				{
					Rect inspectBtn = position.AlignRight(14);
					inspectBtn.height = 14;
					SirenixEditorGUI.BeginDrawOpenInspector(inspectBtn, uObj, position);
					SirenixEditorGUI.EndDrawOpenInspector(inspectBtn, uObj);
				}

				var isPressed = false;

				if (GUI.Button(selectRect, "Select", SirenixGUIStyles.TagButton))
				{
					isPressed = true;
					GUIHelper.RemoveFocusControl();
					Event.current.Use();
				}

				if (hasKeyboardFocus && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
				{
					isPressed = true;
					Event.current.Use();
				}

				if (isPressed)
				{
					if (property != null)
					{
						OdinObjectSelector.Show(popupPosition, selectorKey, selectorId, property, allowSceneObjects);
					}
					else
					{
						OdinObjectSelector.Show(popupPosition, selectorKey, selectorId, value, type, type, allowSceneObjects, false, property);
					}
				}
			}

			if (OdinObjectSelector.IsReadyToClaim(selectorKey, selectorId))
			{
				if (property != null)
				{
					value = OdinObjectSelector.ClaimAndAssign(property);

					GUI.changed = true;

					return value;
				}
				
				GUI.changed = true;
				return OdinObjectSelector.Claim();
			}

			if (hasKeyboardFocus && Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyDown)
			{
				Event.current.Use();
				GUI.changed = true;
				return null;
			}

			if (uObj && Event.current.rawType == EventType.MouseUp && isMouseOver && Event.current.button == 0)
			{
				// For components ping the attached game object instead, because then Unity can figure out to ping prefabs in the project window too.
				UnityEngine.Object pingObj = uObj;

				if (pingObj is Component component)
				{
					pingObj = component.gameObject;
				}

				EditorGUIUtility.PingObject(pingObj);
			}

			EditorGUI.BeginChangeCheck();
			{
				value = OdinObjectSelector.GetChangedObject(value, selectorKey, selectorId);
			}
			if (EditorGUI.EndChangeCheck())
			{
				if (property != null)
				{
					object capturedValue = value;

					property.Tree.DelayActionUntilRepaint(() => { property.ValueEntry.WeakSmartValue = capturedValue; });
				}

				GUIHelper.RequestRepaint();
			}

			return value;
		}

		public static object ObjectPickerZone(Rect rect, object value, Type type, bool allowSceneObjects, int id)
		{
			// TODO: btnId wasn't used, but the GetControlID call is probably still important.
			//var btnId = GUIUtility.GetControlID(FocusType.Passive);
			GUIUtility.GetControlID(FocusType.Passive);
			var objectPicker = ObjectPicker.GetObjectPicker(type.FullName + "+" + GUIHelper.CurrentWindowInstanceID.ToString() + "+" + id, type);
			var selectRect = rect.AlignBottom(15).AlignCenter(45);
			var uObj = value as UnityEngine.Object;
			selectRect.xMin = Mathf.Max(selectRect.xMin, rect.xMin);

			var hide = DragAndDropUtilities.IsDragging || Event.current.type == EventType.Repaint && !rect.Contains(Event.current.mousePosition);

			if (hide)
			{
				GUIHelper.PushColor(new Color(0, 0, 0, 0));
				GUIHelper.PushGUIEnabled(false);
			}

			bool hideInspectorBtn = !hide && !(uObj);

			if (hideInspectorBtn)
			{
				GUIHelper.PushGUIEnabled(false);
				GUIHelper.PushColor(new Color(0, 0, 0, 0));
			}

			var inspectBtn = rect.AlignRight(14);
			inspectBtn.height = 14;
			SirenixEditorGUI.BeginDrawOpenInspector(inspectBtn, uObj, rect);
			SirenixEditorGUI.EndDrawOpenInspector(inspectBtn, uObj);

			if (hideInspectorBtn)
			{
				GUIHelper.PopColor();
				GUIHelper.PopGUIEnabled();
			}

			if (GUI.Button(selectRect, "select", SirenixGUIStyles.TagButton))
			{
				GUIHelper.RemoveFocusControl();
				objectPicker.ShowObjectPicker(value, allowSceneObjects, rect, false);
				Event.current.Use();
			}

			if (Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown && EditorGUIUtility.keyboardControl == id)
			{
				objectPicker.ShowObjectPicker(value, allowSceneObjects, rect, false);
				Event.current.Use();
			}

			if (hide)
			{
				GUIHelper.PopColor();
				GUIHelper.PopGUIEnabled();
			}

			if (objectPicker.IsReadyToClaim)
			{
				GUIHelper.RequestRepaint();
				GUI.changed = true;
				var newValue = objectPicker.ClaimObject();
				Event.current.Use();
				return newValue;
			}

			if (objectPicker.IsPickerOpen && typeof(UnityEngine.Object).IsAssignableFrom(type))
			{
				return objectPicker.CurrentSelectedObject;
			}

			if (Event.current.keyCode == KeyCode.Delete && Event.current.type == EventType.KeyDown && EditorGUIUtility.keyboardControl == id)
			{
				Event.current.Use();
				GUI.changed = true;
				return null;
			}

			if (uObj && Event.current.rawType == EventType.MouseUp && rect.Contains(Event.current.mousePosition) && Event.current.button == 0)
			{
				// For components ping the attached game object instead, because then Unity can figure out to ping prefabs in the project window too.
				UnityEngine.Object pingObj = uObj;
				if (pingObj is Component)
				{
					pingObj = (pingObj as Component).gameObject;
				}

				EditorGUIUtility.PingObject(pingObj);
			}

			return value;
		}

		public static object DoObjectPickerZoneWrapper(Rect rect, Rect popupRect, object value, Type type, bool allowSceneObjects, int id)
		{
			if (GeneralDrawerConfig.Instance.useOldTypeSelector)
			{
				return ObjectPickerZone(rect, value, type, allowSceneObjects, id);
			}

			return ObjectSelectorZone(rect, popupRect, value, type, allowSceneObjects, id, null, null, id);
		}
	}
}
#endif