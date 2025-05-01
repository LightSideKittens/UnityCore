//-----------------------------------------------------------------------
// <copyright file="HideIfNoComponentAttributeProcessor.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Resolvers
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;

    public class HideIfNoComponentAttributeProcessor<T> : OdinAttributeProcessor<T>
    {
        private const string AttributeTypeName = "HideIfNoComponentAttribute";

        public override bool CanProcessSelfAttributes(InspectorProperty property)
        {
            return property.Attributes.Any(a => a.GetType().Name == AttributeTypeName);
        }

        public override void ProcessSelfAttributes(InspectorProperty property, List<Attribute> attributes)
        {
            var hideIfNoComponentAttribute = attributes.FirstOrDefault(a => a.GetType().Name == AttributeTypeName);
            if (hideIfNoComponentAttribute == null)
            {
                return;
            }

            attributes.Remove(hideIfNoComponentAttribute);

            var componentTypeProperty = hideIfNoComponentAttribute.GetType().GetField("ComponentType", BindingFlags.Public | BindingFlags.Instance);
            var componentType = componentTypeProperty?.GetValue(hideIfNoComponentAttribute) as Type;

            if (componentType == null)
            {
                return;
            }

            var obj = property.SerializationRoot.ValueEntry.WeakSmartValue as MonoBehaviour;
            if (obj == null || !obj.TryGetComponent(componentType, out _))
            {
                attributes.Add(new HideIfAttribute("@true"));
            }
        }
    }
}
#endif