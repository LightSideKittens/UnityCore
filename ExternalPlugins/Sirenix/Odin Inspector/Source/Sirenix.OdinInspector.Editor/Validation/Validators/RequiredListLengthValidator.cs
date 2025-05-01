//-----------------------------------------------------------------------
// <copyright file="RequiredListLengthValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.RequiredListLengthValidator))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using System;
    using System.Collections;

    public class RequiredListLengthValidator : AttributeValidator<RequiredListLengthAttribute>
    {
        private ValueResolver<int> minLength;
        private ValueResolver<int> maxLength;

        protected override void Initialize()
        {
            this.minLength = ValueResolver.Get<int>(this.Property, this.Attribute.MinLengthGetter, this.Attribute.MinLength);
            this.maxLength = ValueResolver.Get<int>(this.Property, this.Attribute.MaxLengthGetter, this.Attribute.MaxLength);
        }

        public override bool CanValidateProperty(InspectorProperty property)
        {
            if (property.ChildResolver is ICollectionResolver && property.ValueEntry != null && typeof(IList).IsAssignableFrom(property.Info.TypeOfValue))
            {
                var attr = property.GetAttribute<RequiredListLengthAttribute>();
                if (attr.PrefabKindIsSet)
                {
                    var targetKind = attr.PrefabKind;
                    var kind = OdinPrefabUtility.GetPrefabKind(property);
                    if ((kind & targetKind) != 0)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return true;
                }
            }

            return false;
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.minLength.HasError)
            {
                result.AddError(this.minLength.ErrorMessage);
            }
            else if (this.maxLength.HasError)
            {
                result.AddError(this.maxLength.ErrorMessage);
            }
            else
            {
                var p = this.Property;
                var hasMin = this.Attribute.MinLengthIsSet || this.Attribute.MinLengthGetter != null;
                var hasMax = this.Attribute.MaxLengthIsSet || this.Attribute.MaxLengthGetter != null;

                if (!hasMax && !hasMin)
                    return;

                var val = p.ValueEntry.WeakSmartValue;
                if (val == null)
                {
                    result.AddError(p.NiceName + " is required");
                    return;
                }

                var collection = p.ValueEntry.WeakValues[0] as IList;
                var count = collection.Count;
                var min = this.minLength.GetValue();
                var max = this.maxLength.GetValue();

                if (min == max && count != min && hasMin && hasMin)
                {
                    WithFix(ref result.AddError($"Collection should have exactly <b>{min}</b> number of elements, but has <color=red>{count}</color>."), p, min);
                }
                else if (hasMin && hasMax)
                {
                    if (min > max)
                        result.AddError($"The minimum required length ({min}) is more than the maximum required length ({max}).");
                    else if (count < min)
                        WithFix(ref result.AddError($"Collection should contain between <b>{min} and {max}</b> number of elements, but only has <color=red>{count}</color>."), p, min);
                    else if (count > max)
                        WithFix(ref result.AddError($"Collection should contain between <b>{min} and {max}</b> number of elements, but has <color=red>{count}</color>."), p, max);
                }
                else if (hasMin && count < min)
                {
                    WithFix(ref result.AddError($"Collection should contain at least <b>{min}</b> number of elements, but only has <color=red>{count}</color>."), p, min);
                }
                else if (hasMax && count > max)
                {
                    WithFix(ref result.AddError($"Collection should not contain more than <b>{max}</b> number of elements, but has <color=red>{count}</color>."), p, max);
                }
            }
        }

        public static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        static void WithFix(ref ResultItem err, InspectorProperty p, int targetSize)
        {
            if (p.ChildResolver is IOrderedCollectionResolver && p.ParentValues.Count == 1)
            {
                err.EnableRichText();
                err.WithFix("Set length to " + targetSize, () =>
                {
                    p.ChildResolver.ForceUpdateChildCount();
                    var collection = p.ValueEntry.WeakValues[0] as IList;
                    var count = collection.Count;
                    var resolver = p.ChildResolver as IOrderedCollectionResolver;
                    var delta = Math.Abs(count - targetSize);

                    if (targetSize > count)
                        for (int j = 0; j < delta; j++)
                            resolver.QueueAdd(GetDefault(resolver.ElementType), 0);
                    else
                        for (int j = 0; j < delta; j++)
                            resolver.QueueRemoveAt(targetSize - (1 + j), 0);
                    resolver.ApplyChanges();
                }, true);
            }
        }
    }
}
#endif