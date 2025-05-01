//-----------------------------------------------------------------------
// <copyright file="ReferenceEqualityComparer.cs" company="Sirenix ApS">
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
    /// Compares objects by reference only, ignoring equality operators completely. This is used by the property tree reference dictionaries to keep track of references.
    /// </summary>
    internal class ReferenceEqualityComparer<T> : IEqualityComparer<T> where T : class
    {
        /// <summary>
        /// A default, cached instance of this generic variant of the reference equality comparer.
        /// </summary>
        public static readonly ReferenceEqualityComparer<T> Default = new ReferenceEqualityComparer<T>();

        /// <summary>
        /// Returns true if the object references are equal.
        /// </summary>
        public bool Equals(T x, T y)
        {
            return object.ReferenceEquals(x, y);
        }

        /// <summary>
        /// Returns the result of the object's own GetHashCode method.
        /// </summary>
        public int GetHashCode(T obj)
        {
            try
            {
                return obj.GetHashCode();
            }
            catch (NullReferenceException)
            {
                return -1;
            }
        }
    }
}