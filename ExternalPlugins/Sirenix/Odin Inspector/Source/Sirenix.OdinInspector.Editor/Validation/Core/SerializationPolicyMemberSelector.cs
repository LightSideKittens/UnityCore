//-----------------------------------------------------------------------
// <copyright file="SerializationPolicyMemberSelector.cs" company="Sirenix ApS">
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

    using Sirenix.Serialization;
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public class SerializationPolicyMemberSelector : IMemberSelector
    {
        public readonly ISerializationPolicy Policy;

        public SerializationPolicyMemberSelector(ISerializationPolicy policy)
        {
            this.Policy = policy;
        }

        public IList<MemberInfo> SelectMembers(Type type)
        {
            return FormatterUtilities.GetSerializableMembers(type, this.Policy);
        }
    }
}
#endif