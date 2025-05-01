//-----------------------------------------------------------------------
// <copyright file="InfoBoxValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.InfoBoxValidator))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.OdinInspector.Editor.ValueResolvers;

    [NoValidationInInspector]
    public class InfoBoxValidator : AttributeValidator<InfoBoxAttribute>
    {
        private ValueResolver<bool> showMessageGetter;
        private ValueResolver<string> messageGetter;

        protected override void Initialize()
        {
            this.showMessageGetter = ValueResolver.Get<bool>(this.Property, this.Attribute.VisibleIf, true);
            this.messageGetter = ValueResolver.GetForString(this.Property, this.Attribute.Message);
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.showMessageGetter == null) return;

            if (this.showMessageGetter.HasError || this.messageGetter.HasError)
            {
                result.Message = ValueResolver.GetCombinedErrors(this.showMessageGetter, this.messageGetter);
                result.ResultType = ValidationResultType.Error;
                return;
            }
            
            bool showMessage = this.showMessageGetter.GetValue();

            if (showMessage)
            {
                result.ResultType = this.Attribute.InfoMessageType.ToValidationResultType();
                result.Message = this.messageGetter.GetValue();
            }
        }
    }
}
#endif