//-----------------------------------------------------------------------
// <copyright file="DefaultLoggers.cs" company="Sirenix ApS">
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
    /// Defines default loggers for serialization and deserialization. This class and all of its loggers are thread safe.
    /// </summary>
    public static class DefaultLoggers
    {
        private static readonly object LOCK = new object();
        private static volatile ILogger unityLogger;

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static ILogger DefaultLogger
        {
            get
            {
                return UnityLogger;
            }
        }

        /// <summary>
        /// Not yet documented.
        /// </summary>
        public static ILogger UnityLogger
        {
            get
            {
                if (unityLogger == null)
                {
                    lock (LOCK)
                    {
                        if (unityLogger == null)
                        {
                            unityLogger = new CustomLogger(UnityEngine.Debug.LogWarning, UnityEngine.Debug.LogError, UnityEngine.Debug.LogException);
                        }
                    }
                }

                return unityLogger;
            }
        }
    }
}