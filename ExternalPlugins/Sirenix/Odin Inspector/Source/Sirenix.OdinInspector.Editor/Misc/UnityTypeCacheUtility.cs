//-----------------------------------------------------------------------
// <copyright file="UnityTypeCacheUtility.cs" company="Sirenix ApS">
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

    using Sirenix.Utilities;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using UnityEditor;

#if SIRENIX_INTERNAL
    [Obsolete("Use UnityEditor.TypeCache instead", true)]
#else
    [Obsolete("Use UnityEditor.TypeCache instead", false)]
#endif
    public static class UnityTypeCacheUtility
    {
        public static readonly bool IsAvailable = true;

        public static IList<Type> GetTypesDerivedFrom(Type type)
        {
            return TypeCache.GetTypesDerivedFrom(type);
        }

        public static IList<Type> GetTypesWithAttribute<T>() where T : Attribute
        {
            return TypeCache.GetTypesWithAttribute<T>();
        }

        public static IList<Type> GetTypesWithAttribute(Type attributeType)
        {
            return TypeCache.GetTypesWithAttribute(attributeType);
        }
    }
}
#endif