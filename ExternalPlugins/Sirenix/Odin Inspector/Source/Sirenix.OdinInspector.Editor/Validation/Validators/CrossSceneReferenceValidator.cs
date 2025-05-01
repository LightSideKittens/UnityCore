//-----------------------------------------------------------------------
// <copyright file="CrossSceneReferenceValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using Sirenix.OdinInspector.Editor.Validation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[assembly: RegisterValidator(typeof(CrossSceneReferenceValidator))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

	public class CrossSceneReferenceValidator : ValueValidator<UnityEngine.Object>
	{
		protected override void Validate(ValidationResult result)
		{
			if (Application.isPlaying)
			{
				// We don't want to be checking this in play mode, since so many cross scene values
				// not expected to be saved are populated at runtime and will display this error.
				return;
			}

			var value = this.Value;

			if (value == null)
			{
				return;
			}

			GameObject go = null;

			if (value is Component component)
			{
				go = component.gameObject;
			}
			else if (value is UnityEngine.GameObject gameObject)
			{
				go = gameObject;
			}

			if (go == null)
			{
				return;
			}

			GameObject otherGo = null;
			var otherComponent = this.Property.Tree.RootProperty.ValueEntry.WeakSmartValue as Component;

			if (otherComponent != null)
			{
				otherGo = otherComponent.gameObject;
			}

			if (otherGo == null)
			{
				return;
			}

			if (CheckForCrossSceneReferencing(go, otherGo))
			{
				result.AddError("Scene Mismatch (Cross Scene References Not Supported)");
			}
		}

		public override bool CanValidateProperty(InspectorProperty property)
		{
			return EditorSceneManager.preventCrossSceneReferences && property.ValueEntry.SerializationBackend != SerializationBackend.None && typeof(Component).IsAssignableFrom(property.Tree.RootProperty.ValueEntry.TypeOfValue);
		}

		internal static bool CheckForCrossSceneReferencing(UnityEngine.GameObject go, UnityEngine.GameObject go2)
		{
			// If either object is a prefab: cannot become a cross scene reference
			if (EditorUtility.IsPersistent(go) || EditorUtility.IsPersistent(go2))
				return false;

			// If either scene is invalid: cannot become a cross scene reference
			if (!go.scene.IsValid() || !go2.scene.IsValid())
				return false;

			return go.scene != go2.scene;
		}
	}
}
#endif