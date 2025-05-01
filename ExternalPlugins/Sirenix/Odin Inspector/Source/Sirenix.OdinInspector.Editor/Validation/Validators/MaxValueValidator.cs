//-----------------------------------------------------------------------
// <copyright file="MaxValueValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.MaxValueValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class MaxValueValidator<T> : AttributeValidator<MaxValueAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> maxValueGetter;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            // If there are both MinValue and MaxValue attributes, let the MinMaxValueCompositeValidator take over.
            var hasMinValueAttribute = property.Attributes.HasAttribute<MinValueAttribute>();
            return (IsNumber || IsVector) && !hasMinValueAttribute;
        }

        protected override void Initialize()
        {
            this.maxValueGetter = ValueResolver.Get(this.Property, this.Attribute.Expression, this.Attribute.MaxValue);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.maxValueGetter.HasError)
            {
                result.Message = this.maxValueGetter.ErrorMessage;
                result.ResultType = ValidationResultType.Error;
                return;
            }

            var max = this.maxValueGetter.GetValue();
            const double min = double.NegativeInfinity;
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