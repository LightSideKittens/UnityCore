//-----------------------------------------------------------------------
// <copyright file="ValidationEvents.cs" company="Sirenix ApS">
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

    public static class ValidationEvents
    {
        public static event Action<ValidationStateChangeInfo> OnValidationStateChanged;

        internal static void InvokeOnValidationStateChanged(ValidationStateChangeInfo info)
        {
            if (OnValidationStateChanged != null)
            {
                OnValidationStateChanged(info);
            }
        }
    }

    public struct ValidationStateChangeInfo
    {
        [Obsolete("Get the validator from the result isntead.", OdinDefineSymbols.SIRENIX_INTERNAL)]
        public IValidator Validator => this.ValidationResult.Setup.Validator as IValidator;

        public ValidationResult ValidationResult;
    }
}
#endif