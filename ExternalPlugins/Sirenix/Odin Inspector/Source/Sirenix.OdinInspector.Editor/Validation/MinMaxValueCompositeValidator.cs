//-----------------------------------------------------------------------
// <copyright file="MinMaxValueCompositeValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.MinMaxValueCompositeValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class MinMaxValueCompositeValidator<T> : ValueValidator<T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> minValueGetter;
        private ValueResolver<double> maxValueGetter;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            var hasMinValueAttribute = property.Attributes.HasAttribute<MinValueAttribute>();
            var hasMaxValueAttribute = property.Attributes.HasAttribute<MaxValueAttribute>();
            return (IsNumber || IsVector) && hasMinValueAttribute && hasMaxValueAttribute;
        }

        protected override void Initialize()
        {
            var minValueAttribute = this.Property.Attributes.GetAttribute<MinValueAttribute>();
            var maxValueAttribute = this.Property.Attributes.GetAttribute<MaxValueAttribute>();
            this.minValueGetter = ValueResolver.Get(this.Property, minValueAttribute.Expression, minValueAttribute.MinValue);
            this.maxValueGetter = ValueResolver.Get(this.Property, maxValueAttribute.Expression, maxValueAttribute.MaxValue);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.minValueGetter.HasError)
            {
                result.Message = this.minValueGetter.ErrorMessage;
                result.ResultType = ValidationResultType.Error;
                return;
            }

            if (this.maxValueGetter.HasError)
            {
                result.Message = this.maxValueGetter.ErrorMessage;
                result.ResultType = ValidationResultType.Error;
                return;
            }

            var min = this.minValueGetter.GetValue();
            var max = this.maxValueGetter.GetValue();
            var value = this.ValueEntry.SmartValue;

            if (!GenericNumberUtility.NumberIsInRange(value, min, max, out var error))
            {
                result.Message = error;
                result.ResultType = ValidationResultType.Error;
            }
        }
    }
}
#endif