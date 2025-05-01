//-----------------------------------------------------------------------
// <copyright file="RequireComponentValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.RequireComponentValidator<>))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor;
    using Sirenix.Utilities;
    using UnityEditor;
    using UnityEngine;

    public class RequireComponentValidator<T> : AttributeValidator<RequireComponent, T>
        where T : Component
    {
        public override bool CanValidateProperty(InspectorProperty property)
        {
            return property == property.Tree.RootProperty;
        }

        protected override void Validate(ValidationResult result)
        {
            var value = this.ValueEntry.SmartValue;

            bool ignore = false;

            if ((value as UnityEngine.Object) == null)
                ignore = true;

            if (ignore)
            {
                result.ResultType = ValidationResultType.IgnoreResult;
                return;
            }

            Validate(result, value, Attribute.m_Type0);
            Validate(result, value, Attribute.m_Type1);
            Validate(result, value, Attribute.m_Type2);
        }

        private void Validate(ValidationResult result, T target, System.Type type)
        {
            if (type != null && typeof(UnityEngine.Component).IsAssignableFrom(type))
            {
                if (target.gameObject.GetComponent(type) == null)
                {
                    var msg = target.gameObject.name + " is missing required component of type '" + type.GetNiceName() + "'";
                    if (!IsAbstractOrSpecialCase(type))
                    {
                        if (this.Property.Tree.PrefabModificationHandler.HasPrefabs)
                        {
                            result.AddError(msg).WithFix(Fix.Create<AddWithPrefabSupport>("Add missing components", (x) =>
                            {
                                if (x.AddToPrefab)
                                {
                                    var prefab = PrefabUtility.GetCorrespondingObjectFromOriginalSource(target);

                                    AddMissingComponents(prefab.gameObject);
                                }
                                else
                                {
                                    AddMissingComponents(target.gameObject);
                                }
                            }));
                        }
                        else
                        {
                            result.AddError(msg).WithFix(Fix.Create("Add missing components", () =>
                            {
                                AddMissingComponents(target.gameObject);
                            }));
                        }
                    }
                    else
                    {
                        result.AddError(msg); // TODO: Create variant that can find find assignable components.
                    }
                }
            }
        }

        private static bool IsAbstractOrSpecialCase(System.Type type)
        {
            if (type == typeof(Collider))
                return true;

            return type.IsAbstract;
        }

        private void AddMissingComponents(GameObject target)
        {
            if (this.Attribute.m_Type0 != null && !target.GetComponent(this.Attribute.m_Type0) && !IsAbstractOrSpecialCase(this.Attribute.m_Type0))
            {
                Undo.AddComponent(target, this.Attribute.m_Type0);
            }

            if (this.Attribute.m_Type1 != null && !target.GetComponent(this.Attribute.m_Type1) && !IsAbstractOrSpecialCase(this.Attribute.m_Type1))
            {
                Undo.AddComponent(target, this.Attribute.m_Type1);
            }

            if (this.Attribute.m_Type2 != null && !target.GetComponent(this.Attribute.m_Type2) && !IsAbstractOrSpecialCase(this.Attribute.m_Type2))
            {
                Undo.AddComponent(target, this.Attribute.m_Type2);
            }
        }

        private class AddWithPrefabSupport
        {
            public bool AddToPrefab = true;
        }
    }
}
#endif