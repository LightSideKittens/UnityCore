//-----------------------------------------------------------------------
// <copyright file="SceneObjectsOnlyValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.SceneObjectsOnlyValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using Sirenix.Utilities;
    using UnityEngine;

    public class SceneObjectsOnlyValidator<T> : AttributeValidator<SceneObjectsOnlyAttribute, T>
        where T : UnityEngine.Object
    {
        protected override void Validate(ValidationResult result)
        {
            var value = this.ValueEntry.SmartValue;

            if (value != null)
            {
                var kind = OdinPrefabUtility.GetPrefabKind(value);
                var isScene = (int)(kind & (PrefabKind.InstanceInScene | PrefabKind.NonPrefabInstance)) != 0;

                if (!isScene)
                {
                    string name = value.name;
                    var component = value as Component;
                    if (component != null)
                        name = "from " + component.gameObject.name;

                    result.AddError((value as object).GetType().GetNiceName() + " " + name + " cannot be an asset.");
                }
            }
        }
    }

}
#endif