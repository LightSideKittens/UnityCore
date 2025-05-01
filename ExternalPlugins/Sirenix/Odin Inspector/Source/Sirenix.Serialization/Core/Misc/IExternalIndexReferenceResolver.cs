//-----------------------------------------------------------------------
// <copyright file="IExternalIndexReferenceResolver.cs" company="Sirenix ApS">
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

    /// <summary>
    /// Resolves external index references to reference objects during serialization and deserialization.
    /// </summary>
    public interface IExternalIndexReferenceResolver
    {
        /// <summary>
        /// Tries to resolve the given reference index to a reference value.
        /// </summary>
        /// <param name="index">The index to resolve.</param>
        /// <param name="value">The resolved value.</param>
        /// <returns><c>true</c> if the index could be resolved to a value, otherwise <c>false</c>.</returns>
        bool TryResolveReference(int index, out object value);

        /// <summary>
        /// Determines whether the specified value can be referenced externally via this resolver.
        /// </summary>
        /// <param name="value">The value to reference.</param>
        /// <param name="index">The index of the resolved value, if it can be referenced.</param>
        /// <returns><c>true</c> if the reference can be resolved, otherwise <c>false</c>.</returns>
        bool CanReference(object value, out int index);
    }
}