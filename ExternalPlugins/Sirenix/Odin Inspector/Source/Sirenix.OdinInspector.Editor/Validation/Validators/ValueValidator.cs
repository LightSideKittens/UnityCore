//-----------------------------------------------------------------------
// <copyright file="ValueValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System.ComponentModel;

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public abstract class ValueValidator<TValue> : Validator, DefaultValidatorLocator.IValueValidator_InternalTemporaryHack
    {
        // TODO: Remove this grossness the moment type matching has been fixed to not match value validators to attributes and vice versa
        Type DefaultValidatorLocator.IValueValidator_InternalTemporaryHack.ValidatedType { get { return typeof(TValue); } }

        private IPropertyValueEntry<TValue> valueEntry;

        public IPropertyValueEntry<TValue> ValueEntry
        {
            get
            {
                if (this.valueEntry == null || !object.ReferenceEquals(this.valueEntry, this.Property.ValueEntry))
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

        protected virtual void Validate(ValidationResult result)
        {
            result.ResultType = ValidationResultType.Warning;
            result.Message = "Validation logic for " + this.GetType().GetNiceName() + " has not been implemented yet. Override Validate(ValidationResult result) to implement validation logic.";
        }

        public sealed override void RunValidation(ref ValidationResult result)
        {
            this.InitializeResult(ref result);

            //this.ValueEntry = this.Property.ValueEntry as IPropertyValueEntry<TValue>;

            if (this.ValueEntry == null)
            {
                result.ResultType = ValidationResultType.Error;
                result.Message = "Property " + this.Property.NiceName + " did not have validator " + this.GetType().GetNiceName() + "'s expected value entry of type '" + typeof(TValue).GetNiceName() + "' on it, but instead a value entry of type '" + this.Property.ValueEntry.TypeOfValue.GetNiceName() + "'!";
                return;
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
                result.Message = ex.ToString();
            }
        }
    }
}
#endif