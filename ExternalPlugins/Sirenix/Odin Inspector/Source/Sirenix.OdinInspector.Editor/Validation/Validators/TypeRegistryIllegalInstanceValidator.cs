//-----------------------------------------------------------------------
// <copyright file="TypeRegistryIllegalInstanceValidator.cs" company="Sirenix ApS">
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

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.TypeRegistryIllegalInstanceValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

	public class TypeRegistryIllegalInstanceValidator<T> : ValueValidator<T>
	{
		public override bool CanValidateProperty(InspectorProperty property)
		{
			return !typeof(UnityEngine.Object).IsAssignableFrom(property.ValueEntry.BaseValueType);
		}

		protected override void Validate(ValidationResult result)
		{
			Type baseType = this.Property.ValueEntry.BaseValueType;

			if (TypeRegistryUserConfig.Instance.IsIllegal(baseType))
			{
				result.AddWarning($"The base type of this property is an illegal type: '{baseType}'.");
			}

			if (this.Property.ValueEntry.ValueState == PropertyValueState.NullReference ||
				 !this.ValueEntry.SerializationBackend.SupportsPolymorphism)
			{
				return;
			}

			Type valueType = this.Property.ValueEntry.TypeOfValue;

			if (TypeRegistryUserConfig.Instance.IsIllegal(valueType))
			{
				result.AddWarning($"The current value is of type '{valueType}', which is considered illegal.");
			}
		}
	}
}
#endif