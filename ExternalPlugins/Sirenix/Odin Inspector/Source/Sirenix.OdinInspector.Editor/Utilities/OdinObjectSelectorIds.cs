//-----------------------------------------------------------------------
// <copyright file="OdinObjectSelectorIds.cs" company="Sirenix ApS">
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
using System.Collections.Generic;
using System.Reflection;
using Sirenix.OdinInspector.Editor.Internal;
using Sirenix.OdinInspector.Editor.Validation;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

#if SIRENIX_INTERNAL
[assembly: RegisterValidator(typeof(OdinObjectSelectorIdsValidator))]
#endif

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	/// <summary>
	/// Contains a set of Unique IDs used for various parts of Odin that don't rely on ControlIds as the ID identifier for OdinObjectSelector.
	/// </summary>
	public static class OdinObjectSelectorIds
	{
		private const int BASE = Int32.MinValue + 8192;

		public const int POLYMORPHIC_FIELD = BASE + 1;
		public const int OBJECT_FIELD = BASE + 2;
		public const int PREVIEW_OBJECT_FIELD = BASE + 3;
		public const int DROP_ZONE_SELECTOR = BASE + 4;
		public const int LOCALIZATION_EDITOR = BASE + 5;
		public const int ODIN_DRAWER_FIELD = BASE + 6;
	}

#if SIRENIX_INTERNAL
	public class OdinObjectSelectorIdsValidator : GlobalValidator
	{
		public override IEnumerable RunValidation(ValidationResult result)
		{
			FieldInfo[] fields = typeof(OdinObjectSelectorIds).GetFields(BindingFlags.Static | BindingFlags.Public);

			var existingIds = new Dictionary<int, string>();

			PopulateDictionaryWithUnityIdsInType(existingIds, typeof(GUI));
			PopulateDictionaryWithUnityIdsInType(existingIds, typeof(EditorGUI));

			for (var i = 0; i < fields.Length; i++)
			{
				FieldInfo currentField = fields[i];

				if (currentField.GetReturnType() != typeof(int))
				{
					result.AddError($"Expected return type of '{typeof(int)}' got '{currentField.GetReturnType()}' for field '{currentField.Name}'.");
					continue;
				}

				var id = (int) currentField.GetValue(null);

				if (existingIds.TryGetValue(id, out string originalIdField))
				{
					result.AddError($"The ID '{id}' for '{currentField.Name}' is already used for '{originalIdField}'.");
					continue;
				}

				existingIds.Add(id, currentField.Name);
			}

			return null;
		}

		private static void PopulateDictionaryWithUnityIdsInType(Dictionary<int, string> target, Type type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.NonPublic);

			foreach (FieldInfo field in fields)
			{
				if (field.GetReturnType() != typeof(int))
				{
					continue;
				}

				if (!field.Name.EndsWith("Field") && !field.Name.EndsWith("Hash"))
				{
					continue;
				}

				target[(int) field.GetValue(null)] = field.Name;
			}
		}
	}
#endif
}
#endif