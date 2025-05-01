//-----------------------------------------------------------------------
// <copyright file="DelayedAttributePropertyProcessor.cs" company="Sirenix ApS">
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

    using UnityEngine;
    using System.Collections.Generic;

    [ResolverPriority(-1000000)]
    public class DelayedAttributeProcessor<T> : OdinPropertyProcessor<T, DelayedAttribute>
        where T : struct
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            for (int i = 0; i < propertyInfos.Count; i++)
            {
                propertyInfos[i].GetEditableAttributesList().Add(new DelayedAttribute());
            }
        }
    }

    [ResolverPriority(-1000000)]
    public class DelayedPropertyAttributeProcessor<T> : OdinPropertyProcessor<T, DelayedPropertyAttribute>
        where T : struct
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> propertyInfos)
        {
            for (int i = 0; i < propertyInfos.Count; i++)
            {
                propertyInfos[i].GetEditableAttributesList().Add(new DelayedPropertyAttribute());
            }
        }
    }
}
#endif