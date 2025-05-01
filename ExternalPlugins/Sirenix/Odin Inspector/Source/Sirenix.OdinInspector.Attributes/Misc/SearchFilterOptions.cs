//-----------------------------------------------------------------------
// <copyright file="SearchFilterOptions.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System;

    /// <summary>
    /// Options for filtering search.
    /// </summary>
    [Flags]
    public enum SearchFilterOptions
    {
        PropertyName = 1 << 0,
        PropertyNiceName = 1 << 1,
        TypeOfValue = 1 << 2,
        ValueToString = 1 << 3,
        ISearchFilterableInterface = 1 << 4,
        All = ~0
    }
}