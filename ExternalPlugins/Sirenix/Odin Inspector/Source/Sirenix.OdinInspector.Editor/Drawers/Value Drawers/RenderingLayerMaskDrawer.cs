//-----------------------------------------------------------------------
// <copyright file="RenderingLayerMaskDrawer.cs" company="Sirenix ApS">
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
using System.Reflection;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;

[assembly: Sirenix.OdinInspector.Editor.StaticInitializeBeforeDrawing(typeof(Sirenix.OdinInspector.Editor.Drawers.RenderingLayerMaskDrawerMatcher))]

namespace Sirenix.OdinInspector.Editor.Drawers
{
#pragma warning disable

	[OdinDontRegister]
	[DrawerPriority(0, 0, 0.25)] // Same priority as UnityObjectDrawer, since that is the drawer replaced by the matcher.
	public class RenderingLayerMaskDrawer<TStruct> : OdinValueDrawer<TStruct> where TStruct : struct
	{
		private delegate TStruct DoFieldDelegate(GUIContent content, TStruct value, GUILayoutOption[] options);

		private static DoFieldDelegate doField;

		protected override void Initialize()
		{
			if (doField == null)
			{
				MethodInfo doFieldInfo = RenderingLayerMaskReflection.RenderingLayerMaskFieldInfo;

				if (doFieldInfo == null)
				{
#if SIRENIX_INTERNAL
					const string CLASS_NAME = nameof(RenderingLayerMaskReflection);
					const string FIELD_NAME = nameof(RenderingLayerMaskReflection.RenderingLayerMaskFieldInfo);

					Debug.LogError($"[SIRENIX INTERNAL]: The drawer shouldn't have been created if '{CLASS_NAME}.{FIELD_NAME}' is null.");
#endif

					this.SkipWhenDrawing = true;

					return;
				}

				doField = (DoFieldDelegate) Delegate.CreateDelegate(typeof(DoFieldDelegate), doFieldInfo);
			}
		}

		protected override void DrawPropertyLayout(GUIContent label)
		{
			IPropertyValueEntry<TStruct> entry = this.ValueEntry;

			entry.SmartValue = doField(label, entry.SmartValue, Array.Empty<GUILayoutOption>());
		}
	}

	internal static class RenderingLayerMaskDrawerMatcher
	{
		static RenderingLayerMaskDrawerMatcher()
		{
			if (!UnityVersion.IsVersionOrGreater(6000, 0))
			{
				return;
			}

			Type type = RenderingLayerMaskReflection.RenderingLayerMaskType;

			if (type == null || RenderingLayerMaskReflection.RenderingLayerMaskFieldInfo == null)
			{
				return;
			}

			Type drawerType = typeof(RenderingLayerMaskDrawer<>).MakeGenericType(type);

			Type targetMatchType = typeof(UnityObjectDrawer<>);

			DrawerUtilities.SearchIndex.MatchRules.Add(new TypeSearch.TypeMatchRule("Unity RenderingLayerMask Matcher",
																											(info, targets) =>
																											{
																												if (targets.Length != 1 || targets[0] != type)
																												{
																													return null;
																												}

																												if (info.MatchType != targetMatchType)
																												{
																													return null;
																												}

																												return drawerType;
																											}));
		}
	}

	internal static class RenderingLayerMaskReflection
	{
		private const string TYPE_NAME = "UnityEngine.RenderingLayerMask, UnityEngine.CoreModule";

		private const string DRAW_FIELD_METHOD_NAME = "RenderingLayerMaskField";

		public static readonly Type RenderingLayerMaskType;

		public static readonly MethodInfo RenderingLayerMaskFieldInfo;

		static RenderingLayerMaskReflection()
		{
			RenderingLayerMaskType = TwoWaySerializationBinder.Default.BindToType(TYPE_NAME);

			if (RenderingLayerMaskType == null)
			{
#if SIRENIX_INTERNAL
				Debug.LogError("[SIRENIX INTERNAL]: Failed to find System.Type for 'RenderingLayerMask'.");
#endif

				return;
			}

			RenderingLayerMaskFieldInfo = typeof(EditorGUILayout).GetMethod(DRAW_FIELD_METHOD_NAME,
																								 Flags.StaticPublic,
																								 null,
																								 new[]
																								 {
																									 typeof(GUIContent),
																									 RenderingLayerMaskType,
																									 typeof(GUILayoutOption[])
																								 },
																								 null);

			if (RenderingLayerMaskFieldInfo == null)
			{
#if SIRENIX_INTERNAL
				Debug.LogError($"[SIRENIX INTERNAL]: Failed to find method '{nameof(EditorGUILayout)}.{DRAW_FIELD_METHOD_NAME}'.");
#endif

				return;
			}
		}
	}
}
#endif