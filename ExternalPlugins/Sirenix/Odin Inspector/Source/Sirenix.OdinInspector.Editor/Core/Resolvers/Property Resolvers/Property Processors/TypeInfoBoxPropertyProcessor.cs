//-----------------------------------------------------------------------
// <copyright file="TypeInfoBoxPropertyProcessor.cs" company="Sirenix ApS">
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

    using System.Collections.Generic;
    using Sirenix.OdinInspector;

    [ResolverPriority(-10)]
    public class TypeInfoBoxPropertyProcessor<T> : OdinPropertyProcessor<T, TypeInfoBoxAttribute>
    {
        public override void ProcessMemberProperties(List<InspectorPropertyInfo> memberInfos)
        {
            var attr = this.Property.GetAttribute<TypeInfoBoxAttribute>();
            memberInfos.AddDelegate("InjectedTypeInfoBox", () => { }, -100000, new InfoBoxAttribute(attr.Message), new OnInspectorGUIAttribute("@"));
        }
    }
}
#endif