//-----------------------------------------------------------------------
// <copyright file="DisallowModificationsInAttributeValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.DisallowModificationsInAttributeValidator))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;

    public class DisallowModificationsInAttributeValidator : AttributeValidator<DisallowModificationsInAttribute>
    {
        protected override void Validate(ValidationResult result)
        {
            var kind = OdinPrefabUtility.GetPrefabKind(this.Property);

            if ((kind & this.Attribute.PrefabKind) != 0 && this.Property.ValueEntry.ValueChangedFromPrefab)
            {
                result.AddError($"Modifications on '{this.Attribute.PrefabKind}' for {this.Property.NiceName} are not allowed.");
            }
        }
    }
}
#endif