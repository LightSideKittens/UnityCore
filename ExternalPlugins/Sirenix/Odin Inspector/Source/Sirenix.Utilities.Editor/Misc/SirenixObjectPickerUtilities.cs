//-----------------------------------------------------------------------
// <copyright file="SirenixObjectPickerUtilities.cs" company="Sirenix ApS">
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
using System.Text;
using Sirenix.Reflection.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	public static class SirenixObjectPickerUtilities
	{
		private static readonly Type ObjectSelectorWindowType;

		static SirenixObjectPickerUtilities()
		{
			const string OBJECT_SELECTOR_ASSEMBLY_QUALIFIED_NAME = "UnityEditor.ObjectSelector, UnityEditor";

			ObjectSelectorWindowType = TwoWaySerializationBinder.Default.BindToType(OBJECT_SELECTOR_ASSEMBLY_QUALIFIED_NAME);

#if SIRENIX_INTERNAL
			if (ObjectSelectorWindowType == null)
			{
				Debug.LogError($"Failed to find ObjectSelector type using '{nameof(TwoWaySerializationBinder)}' with the query: '{OBJECT_SELECTOR_ASSEMBLY_QUALIFIED_NAME}'");
			}
#endif
		}

		public static string GetSearchFilterForPolymorphicType(Type type)
		{
			if (type == typeof(object))
			{
				return string.Empty;
			}
			
			var buffer = new StringBuilder();
			
			TypeCache.TypeCollection types = TypeCache.GetTypesDerivedFrom(type);
			
			for (var i = 0; i < types.Count; i++)
			{
				if (!types[i].IsSubclassOf(typeof(UnityEngine.Object)))
				{
					continue;
				}
				
				buffer.Append('t');
				buffer.Append(':');
				buffer.Append(types[i].Name);
				buffer.Append(' ');
			}

			return buffer.ToString();
		}


		public static void MoveCaretToEndOfSearchFilter()
		{
			if (ObjectSelectorWindowType == null)
			{
				return;
			}

			UnityEngine.Object[] selectorWindows = Resources.FindObjectsOfTypeAll(ObjectSelectorWindowType);

			bool doesWindowExists = selectorWindows.Length > 0;

			if (!doesWindowExists)
			{
				return;

			}

			UnityEngine.Object window = selectorWindows[0];

			EditorApplication.delayCall += () =>
			{
				if (EditorWindow.focusedWindow != window)
				{
					return;
				}
				
				TextEditor recycledEditor = EditorGUI_Internals.RecycledEditor;

				recycledEditor.selectIndex = recycledEditor.cursorIndex = recycledEditor.text.Length;
			};
		}
	}
}
#endif