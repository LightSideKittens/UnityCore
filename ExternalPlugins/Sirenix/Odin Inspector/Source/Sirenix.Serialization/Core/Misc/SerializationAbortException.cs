//-----------------------------------------------------------------------
// <copyright file="SerializationAbortException.cs" company="Sirenix ApS">
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
    /// An exception thrown when the serialization system has encountered an issue so severe that serialization is being aborted. If this exception is caught in the serialization system somewhere, it should be rethrown.
    /// </summary>
    public class SerializationAbortException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public SerializationAbortException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SerializationException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public SerializationAbortException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}