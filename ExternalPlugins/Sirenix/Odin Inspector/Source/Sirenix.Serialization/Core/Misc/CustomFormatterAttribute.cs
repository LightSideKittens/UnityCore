//-----------------------------------------------------------------------
// <copyright file="CustomFormatterAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization
{
#pragma warning disable

    using System;

    /// <summary>
    /// Attribute indicating that a class which implements the <see cref="IFormatter{T}" /> interface somewhere in its hierarchy is a custom formatter for the type T.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    [AttributeUsage(AttributeTargets.Class)]
    [Obsolete("Use a RegisterFormatterAttribute applied to the containing assembly instead.", true)]
    [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
    public class CustomFormatterAttribute : Attribute
    {
        /// <summary>
        /// The priority of the formatter. Of all the available custom formatters, the formatter with the highest priority is always chosen.
        /// </summary>
        public readonly int Priority;

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFormatterAttribute"/> class with priority 0.
        /// </summary>
        public CustomFormatterAttribute()
        {
            this.Priority = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomFormatterAttribute"/> class.
        /// </summary>
        /// <param name="priority">The priority of the formatter. Of all the available custom formatters, the formatter with the highest priority is always chosen.</param>
        public CustomFormatterAttribute(int priority = 0)
        {
            this.Priority = priority;
        }
    }
}