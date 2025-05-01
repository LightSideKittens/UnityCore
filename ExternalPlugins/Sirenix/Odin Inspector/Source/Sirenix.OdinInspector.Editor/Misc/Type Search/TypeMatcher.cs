//-----------------------------------------------------------------------
// <copyright file="TypeMatcher.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.TypeSearch
{
#pragma warning disable

    using System;

    public abstract class TypeMatcher
    {
        public abstract string Name { get; }
        public abstract Type Match(Type[] targets, ref bool stopMatching);
    }
}
#endif