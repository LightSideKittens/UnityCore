//-----------------------------------------------------------------------
// <copyright file="RegisterValidatorAttribute.cs" company="Sirenix ApS">
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

    using System;

    /// <summary>
    /// Apply this to an assembly to register validators for the validation system.
    /// This enables locating of all relevant validator types very quickly.
    /// </summary>
    /// <seealso cref="RegisterValidationRuleAttribute"/>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class RegisterValidatorAttribute : Attribute
    {
        public readonly Type ValidatorType;
        public readonly Type SpecialInstantiator;
        public int Priority;

        public RegisterValidatorAttribute(Type validatorType)
        {
            this.ValidatorType = validatorType;
        }

        protected RegisterValidatorAttribute(Type validatorType, Type specialInstantiator)
        {
            this.ValidatorType = validatorType;
            this.SpecialInstantiator = specialInstantiator;
        }
    }
}
#endif