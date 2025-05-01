//-----------------------------------------------------------------------
// <copyright file="ComponentProvider.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.Validation;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEngine;

    public abstract class ComponentProvider
    {
        public abstract PropertyComponent CreateComponent(InspectorProperty property);
    }

    public abstract class PropertyComponent
    {
        public readonly InspectorProperty Property;

        public PropertyComponent(InspectorProperty property)
        {
            this.Property = property;
        }

        public virtual void Reset() { }
    }

    public sealed class ValidationComponentProvider : ComponentProvider
    {
        public IValidatorLocator ValidatorLocator;

        public ValidationComponentProvider()
        {
            this.ValidatorLocator = new DefaultValidatorLocator();
        }

        public ValidationComponentProvider(IValidatorLocator validatorLocator)
        {
            this.ValidatorLocator = validatorLocator;
        }

        public override PropertyComponent CreateComponent(InspectorProperty property)
        {
            return new ValidationComponent(property, this.ValidatorLocator);
        }
    }

    public sealed class ValidationComponent : PropertyComponent, IDisposable
    {
        public readonly IValidatorLocator ValidatorLocator;
        private IList<Validator> validators;

        private static readonly System.Diagnostics.Stopwatch Stopwatch = new System.Diagnostics.Stopwatch();

        public ValidationComponent(InspectorProperty property, IValidatorLocator validatorLocator) : base(property)
        {
            this.ValidatorLocator = validatorLocator;
        }

        public void Dispose()
        {
            if (this.validators != null)
            {
                for (int i = 0; i < this.validators.Count; i++)
                {
                    var disposable = this.validators[i] as IDisposable;

                    if (disposable != null)
                    {
                        try
                        {
                            disposable.Dispose();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                }

                this.validators = null;
            }
        }

        public IList<Validator> GetValidators()
        {
            if (this.validators == null)
            {
                if (this.ValidatorLocator.PotentiallyHasValidatorsFor(this.Property))
                {
                    this.validators = this.ValidatorLocator.GetValidators(this.Property);
                }
                else this.validators = new Validator[0];
            }

            return this.validators;
        }

        public override void Reset()
        {
            this.validators = null;
        }

        public void ValidateProperty(ref List<ValidationResult> results, bool explodeMultiResults = false)
        {
            if (results == null)
            {
                results = new List<ValidationResult>();
            }

            if (this.validators == null)
            {
                this.GetValidators();
            }

            for (int i = 0; i < this.validators.Count; i++)
            {
                var validator = this.validators[i];

                ValidationResult result = new ValidationResult();

                try
                {
                    Stopwatch.Restart();
                    validator.RunValidation(ref result);

                    Stopwatch.Stop();
                    if (result != null)
                    {
                        if (explodeMultiResults)
                        {
                            result.Explode(ref results, Stopwatch.Elapsed.TotalMilliseconds);
                        }
                        else
                        {
                            result.ValidationTimeMS = Stopwatch.Elapsed.TotalMilliseconds;
                            results.Add(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Stopwatch.Stop();

                    while (ex is TargetInvocationException)
                    {
                        ex = ex.InnerException;
                    }

                    results.Add(new ValidationResult()
                    {
                        Message = "Exception was thrown during validation of property " + this.Property.NiceName + ": " + ex.ToString(),
                        ResultType = ValidationResultType.Error,
                        Setup = new ValidationSetup()
                        {
                            ParentInstance = this.Property.ParentValues[0],
                            Root = this.Property.SerializationRoot.ValueEntry.WeakValues[0],
                            Validator = validator,
                        },
                        Path = this.Property.Path,
                        ValidationTimeMS = Stopwatch.Elapsed.TotalMilliseconds,
                    });
                }
            }
        }
    }
}
#endif