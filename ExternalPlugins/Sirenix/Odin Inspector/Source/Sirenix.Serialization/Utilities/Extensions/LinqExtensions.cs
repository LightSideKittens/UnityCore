//-----------------------------------------------------------------------
// <copyright file="LinqExtensions.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization.Utilities
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Various LinQ extensions.
    /// </summary>
    internal static class LinqExtensions
    {
        /// <summary>
        /// Perform an action on each item.
        /// </summary>
		/// <param name="source">The source.</param>
		/// <param name="action">The action to perform.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }

            return source;
        }

        /// <summary>
        /// Perform an action on each item.
        /// </summary>
		/// <param name="source">The source.</param>
		/// <param name="action">The action to perform.</param>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T, int> action)
        {
            int counter = 0;

            foreach (var item in source)
            {
                action(item, counter++);
            }

            return source;
        }

        /// <summary>
        /// Add a collection to the end of another collection.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="append">The collection to append.</param>
        public static IEnumerable<T> Append<T>(this IEnumerable<T> source, IEnumerable<T> append)
        {
            foreach (var item in source)
            {
                yield return item;
            }

            foreach (var item in append)
            {
                yield return item;
            }
        }
    }
}