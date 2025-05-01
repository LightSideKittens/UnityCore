//-----------------------------------------------------------------------
// <copyright file="SharedUniqueControlId.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.Reflection.Editor;
using UnityEngine;

namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

	public static class SharedUniqueControlId
	{
		public static bool IsActive => GUIUtility.hotControl == UniqueId;

		public static int UniqueId
		{
			get
			{
				if (uniqueIdBackingField == null)
				{
					uniqueIdBackingField = GUIUtility_Internals.GetPermanentControlID();
				}

				return uniqueIdBackingField.Value;
			}
		}

		private static int? uniqueIdBackingField;

		public static void SetActive() => GUIUtility.hotControl = UniqueId;

		public static void SetInactive()
		{
			if (GUIUtility.hotControl == UniqueId)
			{
				GUIUtility.hotControl = 0;
			}
		}
	}
}
#endif