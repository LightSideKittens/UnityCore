//-----------------------------------------------------------------------
// <copyright file="MinValueValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.MinValueValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;

    public class MinValueValidator<T> : AttributeValidator<MinValueAttribute, T>
        where T : struct
    {
        private static readonly bool IsNumber = GenericNumberUtility.IsNumber(typeof(T));
        private static readonly bool IsVector = GenericNumberUtility.IsVector(typeof(T));

        private ValueResolver<double> minValueGetter;

        public override bool CanValidateProperty(InspectorProperty property)
        {
            // If there are both MinValue and MaxValue attributes, let the MinMaxValueCompositeValidator take over.
            var hasMaxValueAttribute = property.Attributes.HasAttribute<MaxValueAttribute>();
            return (IsNumber || IsVector) && !hasMaxValueAttribute;
        }

        protected override void Initialize()
        {
            this.minValueGetter = ValueResolver.Get(this.Property, this.Attribute.Expression, this.Attribute.MinValue);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.minValueGetter.HasError)
            {
                result.Message = this.minValueGetter.ErrorMessage;
                result.ResultType = ValidationResultType.Error;
                return;
            }

            var min = this.minValueGetter.GetValue();
            const double max = double.PositiveInfinity;
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