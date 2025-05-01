//-----------------------------------------------------------------------
// <copyright file="RootObjectValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System.ComponentModel;

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    public abstract class RootObjectValidator<TValue> : ValueValidator<TValue> where TValue : UnityEngine.Object
    {
        public TValue Object => this.ValueEntry.SmartValue;
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new TValue Value
        {
            get => base.Value;
            set => base.Value = value;
        }

        public sealed override bool CanValidateProperty(InspectorProperty property)
        {
            if (!(property.IsTreeRoot && this.CanValidateRootProperty(property))) return false;

            var count = property.ValueEntry.ValueCount;

            for (int i = 0; i < count; i++)
            {
                if (!this.CanValidateObject((TValue)property.ValueEntry.WeakValues[i])) return false;
            }

            return true;
        }

        protected virtual bool CanValidateRootProperty(InspectorProperty rootProperty)
        {
            return true;
        }

        protected virtual bool CanValidateObject(TValue obj)
        {
            return true;
        }

        protected override void Validate(ValidationResult result)
        {
            base.Validate(result);
        }
    }
}
#endif