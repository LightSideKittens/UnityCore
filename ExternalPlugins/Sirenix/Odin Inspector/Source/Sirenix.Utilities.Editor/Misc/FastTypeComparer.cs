//-----------------------------------------------------------------------
// <copyright file="FastTypeComparer.cs" company="Sirenix ApS">
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

    public class FastTypeComparer : IEqualityComparer<Type>
    {
        public static readonly FastTypeComparer Instance = new FastTypeComparer();

        public bool Equals(Type x, Type y)
        {
            if (object.ReferenceEquals(x, y)) return true; // Oft-used fast path over regular Type.Equals makes this much faster
            return x == y;
        }

        public int GetHashCode(Type obj)
        {
            return obj.GetHashCode();
        }
    }
}
#endif