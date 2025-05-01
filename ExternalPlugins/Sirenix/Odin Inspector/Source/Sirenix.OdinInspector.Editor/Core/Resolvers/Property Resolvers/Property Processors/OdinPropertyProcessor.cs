//-----------------------------------------------------------------------
// <copyright file="OdinPropertyProcessor.cs" company="Sirenix ApS">
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

    using System;
    using System.Collections.Generic;

    public abstract class OdinPropertyProcessor
    {
        public InspectorProperty Property { get; private set; }

        public abstract void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos);

        public virtual bool CanProcessForProperty(InspectorProperty property)
        {
            return true;
        }

        protected virtual void Initialize()
        {
        }

        public static OdinPropertyProcessor Create(Type processorType, InspectorProperty property)
        {
            if (processorType == null)
            {
                throw new ArgumentNullException("processorType");
            }

            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            if (!typeof(OdinPropertyProcessor).IsAssignableFrom(processorType))
            {
                throw new ArgumentException("Type is not a MemberPropertyProcessor");
            }

            var result = (OdinPropertyProcessor)Activator.CreateInstance(processorType);
            result.Property = property;
            result.Initialize();
            return result;
        }

        public static T Create<T>(InspectorProperty property) where T : OdinPropertyProcessor, new()
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            var result = new T();
            result.Property = property;
            result.Initialize();
            return result;
        }
    }

    public abstract class OdinPropertyProcessor<TValue> : OdinPropertyProcessor
    {
        private IPropertyValueEntry<TValue> valueEntry;

        public IPropertyValueEntry<TValue> ValueEntry
        {
            get
            {
                if (this.valueEntry == null)
                {
                    this.valueEntry = this.Property.TryGetTypedValueEntry<TValue>();
                }

                return this.valueEntry;
            }
        }
    }

    public abstract class OdinPropertyProcessor<TValue, TAttribute> : OdinPropertyProcessor<TValue>
        where TAttribute : Attribute
    {
    }
}
#endif