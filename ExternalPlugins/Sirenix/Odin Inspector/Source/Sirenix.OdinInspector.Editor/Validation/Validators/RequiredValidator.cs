//-----------------------------------------------------------------------
// <copyright file="RequiredValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.RequiredValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class RequiredValidator<T> : AttributeValidator<RequiredAttribute, T>
        where T : class
    {
        private ValueResolver<string> errorMessageGetter;

        protected override void Initialize()
        {
            if (this.Attribute.ErrorMessage != null)
            {
                this.errorMessageGetter = ValueResolver.GetForString(this.Property, this.Attribute.ErrorMessage);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            if (!this.IsValid(this.ValueEntry.SmartValue))
            {
                var severity = this.Attribute.MessageType.ToValidatorSeverity();
                var message = this.errorMessageGetter != null ? this.errorMessageGetter.GetValue() : (this.Property.NiceName + " is required");

                result.Add(severity, message)
                    .WithFix<FixArgs<T>>(arg =>
                    {
                        this.Property.ValueEntry.WeakSmartValue = arg.NewValue;
                    }, false);
            }
        }

        private bool IsValid(T memberValue)
        {
            if (object.ReferenceEquals(memberValue, null))
                return false;

            if (memberValue is string && string.IsNullOrEmpty(memberValue as string))
                return false;

            if (memberValue is UnityEngine.Object && (memberValue as UnityEngine.Object) == null)
                return false;

            return true;
        }
    }

    [ShowOdinSerializedPropertiesInInspector]
    internal class FixArgs<T>
    {
        public T NewValue;
    }
}
#endif