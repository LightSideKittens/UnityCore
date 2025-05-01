//-----------------------------------------------------------------------
// <copyright file="TypeSearchInfo.cs" company="Sirenix ApS">
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

    public struct TypeSearchInfo
    {
        public Type MatchType;
        public Type[] Targets;
        public TargetMatchCategory[] TargetCategories;
        public double Priority;
        public object CustomData;
    }
}
#endif