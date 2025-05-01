//-----------------------------------------------------------------------
// <copyright file="IRootValidatorValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
//[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.IRootValidatorValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    //public class IRootValidatorValidator<T> : RootObjectValidator<T>
    //    where T : UnityEngine.Object, IRootValidator
    //{
    //    protected override void Validate(ValidationResult result)
    //    {
    //        var container = new ValidationResultContainer();
    //        this.Object.Validate(container);

    //        foreach (var item in container.Results)
    //        {
    //            var type = item.Type == ValidationResultContainer.ValidationResultItemType.Error ? ValidationResultType.Error : ValidationResultType.Warning;
    //            result.Add(new ResultItem(item.Message, type));
    //        }
    //    }
    //}
}
#endif