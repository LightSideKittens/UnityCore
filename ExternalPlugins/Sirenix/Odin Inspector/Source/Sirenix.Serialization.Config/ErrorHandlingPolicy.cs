//-----------------------------------------------------------------------
// <copyright file="ErrorHandlingPolicy.cs" company="Sirenix ApS">
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
    /// The policy for handling errors during serialization and deserialization.
    /// </summary>
    public enum ErrorHandlingPolicy
    {
        /// <summary>
        /// Attempts will be made to recover from errors and continue serialization. Data may become invalid.
        /// </summary>
        Resilient,

        /// <summary>
        /// Exceptions will be thrown when errors are logged.
        /// </summary>
        ThrowOnErrors,

        /// <summary>
        /// Exceptions will be thrown when warnings or errors are logged.
        /// </summary>
        ThrowOnWarningsAndErrors
    }
}