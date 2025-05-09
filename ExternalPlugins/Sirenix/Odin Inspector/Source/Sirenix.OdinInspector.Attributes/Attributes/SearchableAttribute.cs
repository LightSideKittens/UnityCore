//-----------------------------------------------------------------------
// <copyright file="SearchableAttribute.cs" company="Sirenix ApS">
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
    /// Adds a search filter that can search the children of the field or type on
    /// which it is applied. Note that this does not currently work when directly
    /// applied to dictionaries, though a search field "above" the dictionary will
    /// still search the dictionary's properties if it is searching recursively.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [DontApplyToListElements]
    public class SearchableAttribute : Attribute
    {
        /// <summary>
        /// Whether to use fuzzy string matching for the search.
        /// Default value: true.
        /// </summary>
        public bool FuzzySearch = true;

        /// <summary>
        /// The options for which things to use to filter the search.
        /// Default value: All.
        /// </summary>
        public SearchFilterOptions FilterOptions = SearchFilterOptions.All;

        /// <summary>
        /// Whether to search recursively, or only search the top level properties.
        /// Default value: true.
        /// </summary>
        public bool Recursive = true;
    }
}