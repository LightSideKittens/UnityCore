//-----------------------------------------------------------------------
// <copyright file="AttributeValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    public abstract class AttributeValidator<TAttribute> : Validator, IAttributeValidator
        where TAttribute : Attribute
    {
        private bool? isValueValidator_backing;

        private bool IsValueValidator
        {
            get
            {
                if (!this.isValueValidator_backing.HasValue)
                {
                    this.isValueValidator_backing = this is IAttributeValueValidator;
                }

                return this.isValueValidator_backing.Value;
            }
        }

        private int attributeNumber;

        int IAttributeValidator.AttributeNumber { get { return this.attributeNumber; } }

        Type IAttributeValidator.AttributeType { get { return typeof(TAttribute); } }

        public TAttribute Attribute { get; private set; }

        internal virtual IPropertyValueEntry InternalValueEntry => null;

        public sealed override void RunValidation(ref ValidationResult result)
        {
            this.InitializeResult(ref result);

            bool isValueValidator = this.IsValueValidator;

            if (isValueValidator)
            {
                if (this.InternalValueEntry == null)
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = "Property " + this.Property.NiceName + " did not have validator " + this.GetType().GetNiceName() + "'s expected value entry of type '" + (this as IAttributeValueValidator).GetValueType().GetNiceName() + "' on it, but instead a value entry of type '" + this.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
                    return;
                }
            }

            try
            {
                this.Validate(result);
            }
            catch (Exception ex)
            {
                while (ex is TargetInvocationException)
                {
                    ex = ex.InnerException;
                }

                result.ResultType = ValidationResultType.Error;
                result.Message = "An exception was thrown during validation: " + ex.ToString();
            }
        }

        protected virtual void Validate(ValidationResult result)
        {
            result.ResultType = ValidationResultType.Warning;
            result.Message = "Validation logic for " + this.GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
        }

        void IAttributeValidator.SetAttributeInstanceAndNumber(Attribute attribute, int number)
        {
            this.Attribute = (TAttribute)attribute;
            this.attributeNumber = number;
        }
    }

    public abstract class AttributeValidator<TAttribute, TValue> : AttributeValidator<TAttribute>, IAttributeValueValidator
        where TAttribute : Attribute
    {
        internal override IPropertyValueEntry InternalValueEntry => this.ValueEntry;

        private IPropertyValueEntry<TValue> valueEntry;

        public IPropertyValueEntry<TValue> ValueEntry
        {
            get
            {
                if (this.valueEntry == null)
                {
                    this.valueEntry = this.Property.TryGetTypedValueEntry<TValue>();
                }

                return this.valueEntry;
            }
        }
        
        public TValue Value
        {
            get => this.ValueEntry.SmartValue;
            set => this.ValueEntry.SmartValue = value;
        }

        Type IAttributeValueValidator.GetValueType()
        {
            return typeof(TValue);
        }

        protected override void Validate(ValidationResult result)
        {
            result.ResultType = ValidationResultType.Warning;
            result.Message = "Validation logic for " + this.GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
        }
    }

    namespace Internal
    {
        public interface IAttributeValidator
        {
            Type AttributeType { get; }
            int AttributeNumber { get; }
            void SetAttributeInstanceAndNumber(Attribute attribute, int number);
        }

        internal interface IAttributeValueValidator
        {
            Type GetValueType();
        }
    }
}
#endif