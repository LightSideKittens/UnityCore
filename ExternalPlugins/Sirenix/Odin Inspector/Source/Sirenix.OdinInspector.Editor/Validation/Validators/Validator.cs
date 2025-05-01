//-----------------------------------------------------------------------
// <copyright file="Validator.cs" company="Sirenix ApS">
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

    using System.Collections.Generic;

    public interface IValidator
    {
        RevalidationCriteria RevalidationCriteria { get; }
        void RunValidation(ref ValidationResult result);
    }

    public abstract class Validator : IValidator
    {
        public InspectorProperty Property { get; private set; }

        public virtual RevalidationCriteria RevalidationCriteria => RevalidationCriteria.Always;

        public void Initialize(InspectorProperty property)
        {
            this.Property = property;

            this.Initialize();
        }

        public virtual bool CanValidateProperty(InspectorProperty property)
        {
            return true;
        }

        public virtual void RunValidation(ref ValidationResult result)
        {
        }

        protected virtual void Initialize()
        {
        }
        
        public void InitializeResult(ref ValidationResult result)
        {
            if (result == null)
                result = new ValidationResult();

            result.Setup = new ValidationSetup()
            {
                ParentInstance = this.Property.ParentValues[0],
                Validator = this,
                //Value = this.Property.ValueEntry == null ? this.Property.ValueEntry.WeakSmartValue : null,
                Root = this.Property.SerializationRoot.ValueEntry.WeakValues[0] as UnityEngine.Object,
            };

            result.Path = this.Property.Path;
            result.ResultType = ValidationResultType.Valid;
            result.Message = "";
        }

        public class MetaData : List<ResultItemMetaData>
        {
            public void Add(string key, object value)
            {
                this.Add(new ResultItemMetaData(key, value));
            }
        }
    }
}
#endif