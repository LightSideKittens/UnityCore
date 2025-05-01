//-----------------------------------------------------------------------
// <copyright file="LoggingPolicy.cs" company="Sirenix ApS">
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
    /// The policy for which level of logging to do during serialization and deserialization.
    /// </summary>
    public enum LoggingPolicy
    {
        /// <summary>
        /// Not yet documented.
        /// </summary>
        LogErrors,

        /// <summary>
        /// Not yet documented.
        /// </summary>
        LogWarningsAndErrors,

        /// <summary>
        /// Not yet documented.
        /// </summary>
        Silent
    }
}