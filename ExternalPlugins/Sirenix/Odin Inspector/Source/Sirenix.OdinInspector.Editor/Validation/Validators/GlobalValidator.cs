//-----------------------------------------------------------------------
// <copyright file="GlobalValidator.cs" company="Sirenix ApS">
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
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Sirenix.Internal")]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    public abstract class GlobalValidator : IValidator
    {
        public RevalidationCriteria RevalidationCriteria => RevalidationCriteria.Always;

        internal IEnumerable<ValidationResult> RunValidation()
        {
            var result = new ValidationResult()
            {
                Setup = new ValidationSetup()
                {
                    Validator = this,
                }
            };

            var enumerator = this.RunValidation(result);

            if (enumerator != null)
            {
                foreach (var ignore in enumerator)
                {
                    yield return null;
                }
            }

            yield return result;
        }

        public abstract IEnumerable RunValidation(ValidationResult result);

        void IValidator.RunValidation(ref ValidationResult result)
        {
            var enumerator = this.RunValidation(result).GetEnumerator();
            while (enumerator.MoveNext())
            {
            }
        }
    }
}
#endif