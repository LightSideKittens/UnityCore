//-----------------------------------------------------------------------
// <copyright file="LegacyOdinValidatorCompatibility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinValidator.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    internal static class LegacyOdinValidatorCompatibility
    {
#if SIRENIX_INTERNAL
        [Obsolete("NO!", false)]
#endif
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source) => new HashSet<T>(source);
    }
}
#endif