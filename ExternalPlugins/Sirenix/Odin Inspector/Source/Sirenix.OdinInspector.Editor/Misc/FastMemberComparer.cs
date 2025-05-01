//-----------------------------------------------------------------------
// <copyright file="FastMemberComparer.cs" company="Sirenix ApS">
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
    using System.Reflection;

    public class FastMemberComparer : IEqualityComparer<MemberInfo>
    {
        public static readonly FastMemberComparer Instance = new FastMemberComparer();

        public bool Equals(MemberInfo x, MemberInfo y)
        {
            if (object.ReferenceEquals(x, y)) return true; // Oft-used fast path over regular MemberInfo.Equals makes this much faster
            return x == y;
        }

        public int GetHashCode(MemberInfo obj)
        {
            return obj.GetHashCode();
        }
    }
}
#endif