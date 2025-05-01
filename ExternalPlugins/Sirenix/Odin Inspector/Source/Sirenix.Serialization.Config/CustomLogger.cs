//-----------------------------------------------------------------------
// <copyright file="CustomLogger.cs" company="Sirenix ApS">
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
    /// A helper class for quickly and easily defining custom loggers.
    /// </summary>
    /// <seealso cref="Sirenix.Serialization.ILogger" />
    public class CustomLogger : ILogger
    {
        private Action<string> logWarningDelegate;
        private Action<string> logErrorDelegate;
        private Action<Exception> logExceptionDelegate;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public CustomLogger(Action<string> logWarningDelegate, Action<string> logErrorDelegate, Action<Exception> logExceptionDelegate)
        {
            if (logWarningDelegate == null)
            {
                throw new ArgumentNullException("logWarningDelegate");
            }

            if (logErrorDelegate == null)
            {
                throw new ArgumentNullException("logErrorDelegate");
            }

            if (logExceptionDelegate == null)
            {
                throw new ArgumentNullException("logExceptionDelegate");
            }

            this.logWarningDelegate = logWarningDelegate;
            this.logErrorDelegate = logErrorDelegate;
            this.logExceptionDelegate = logExceptionDelegate;
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void LogWarning(string warning)
        {
            this.logWarningDelegate(warning);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void LogError(string error)
        {
            this.logErrorDelegate(error);
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public void LogException(Exception exception)
        {
            this.logExceptionDelegate(exception);
        }
    }
}