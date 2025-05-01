//-----------------------------------------------------------------------
// <copyright file="ILogger.cs" company="Sirenix ApS">
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
    /// Implements methods for logging warnings, errors and exceptions during serialization and deserialization.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs a warning.
        /// </summary>
        /// <param name="warning">The warning to log.</param>
        void LogWarning(string warning);

        /// <summary>
        /// Logs an error.
        /// </summary>
        /// <param name="error">The error to log.</param>
        void LogError(string error);

        /// <summary>
        /// Logs an exception.
        /// </summary>
        /// <param name="exception">The exception to log.</param>
        void LogException(Exception exception);
    }
}