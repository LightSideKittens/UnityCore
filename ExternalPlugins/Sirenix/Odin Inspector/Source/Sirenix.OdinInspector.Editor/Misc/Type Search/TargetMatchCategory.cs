//-----------------------------------------------------------------------
// <copyright file="TargetMatchCategory.cs" company="Sirenix ApS">
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

    [Flags]
    public enum TargetMatchCategory
    {
        Value = 1,
        Attribute = 1 << 1,
        All = Value | Attribute,
    }
}
#endif