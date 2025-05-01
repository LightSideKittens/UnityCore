//-----------------------------------------------------------------------
// <copyright file="ISearchFilterable.cs" company="Sirenix ApS">
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

    /// <summary>
    /// Implement this interface to create custom matching
    /// logic for search filtering in the inspector.
    /// </summary>
    /// <example>
    /// <para>The following example shows how you might do this:</para>
    /// <code>
    /// public class MyCustomClass : ISearchFilterable
    /// {
    ///     public bool SearchEnabled;
    ///     public string MyStr;
    ///     
    ///     public bool IsMatch(string searchString)
    ///     {
    ///         if (SearchEnabled)
    ///         {
    ///             return MyStr.Contains(searchString);
    ///         }
    ///         
    ///         return false;
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface ISearchFilterable
    {
        bool IsMatch(string searchString);
    }
}