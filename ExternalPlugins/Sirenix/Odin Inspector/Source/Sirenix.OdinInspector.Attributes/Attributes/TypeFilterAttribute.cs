//-----------------------------------------------------------------------
// <copyright file="TypeFilterAttribute.cs" company="Sirenix ApS">
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

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    //[DontApplyToListElements]
    public class TypeFilterAttribute : Attribute
    {

        /// <summary>
        /// Name of any field, property or method member that implements IList. E.g. arrays or Lists. Obsolete; use the FilterGetter member instead.
        /// </summary>
        [Obsolete("Use the FilterGetter member instead.",
#if SIRENIX_INTERNAL
            true
#else
            false
#endif
        )]
		[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        public string MemberName { get { return this.FilterGetter; } set { this.FilterGetter = value; } }

        /// <summary>
        /// A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.
        /// </summary>
        public string FilterGetter;

        /// <summary>
        /// Gets or sets the title for the dropdown. Null by default.
        /// </summary>
        public string DropdownTitle;

        /// <summary>
        /// If true, the value will be drawn normally after the type selector dropdown has been drawn. False by default.
        /// </summary>
        public bool DrawValueNormally;

        /// <summary>
        /// Creates a dropdown menu for a property.
        /// </summary>
        /// <param name="filterGetter">A resolved string that should evaluate to a value that is assignable to IList; e.g, arrays and lists are compatible.</param>
        public TypeFilterAttribute(string filterGetter)
        {
            this.FilterGetter = filterGetter;
        }
    }
}