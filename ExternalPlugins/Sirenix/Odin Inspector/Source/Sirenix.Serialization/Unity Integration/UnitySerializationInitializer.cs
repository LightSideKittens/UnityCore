//-----------------------------------------------------------------------
// <copyright file="UnitySerializationInitializer.cs" company="Sirenix ApS">
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

    using UnityEngine;

    /// <summary>
    /// Utility class which initializes the Sirenix serialization system to be compatible with Unity.
    /// </summary>
    public static class UnitySerializationInitializer
    {
        private static readonly object LOCK = new object();
        private static bool initialized = false;

        public static bool Initialized { get { return initialized; } }

        public static RuntimePlatform CurrentPlatform { get; private set; }
        
        /// <summary>
        /// Initializes the Sirenix serialization system to be compatible with Unity.
        /// </summary>
        public static void Initialize()
        {
            if (!initialized)
            {
                lock (LOCK)
                {
                    if (!initialized)
                    {
                        try
                        {
                            // Ensure that the config instance is loaded before deserialization of anything occurs.
                            // If we try to load it during deserialization, Unity will throw exceptions, as a lot of
                            // the Unity API is disallowed during serialization and deserialization.
                            GlobalSerializationConfig.LoadInstanceIfAssetExists();
                        
                            CurrentPlatform = Application.platform;

                            if (Application.isEditor) return;

                            ArchitectureInfo.SetRuntimePlatform(CurrentPlatform);

                            //if (CurrentPlatform == RuntimePlatform.Android)
                            //{
                            //    //using (var system = new AndroidJavaClass("java.lang.System"))
                            //    //{
                            //    //    string architecture = system.CallStatic<string>("getProperty", "os.arch");
                            //    //    ArchitectureInfo.SetIsOnAndroid(architecture);
                            //    //}
                            //}
                            //else if (CurrentPlatform == RuntimePlatform.IPhonePlayer)
                            //{
                            //    ArchitectureInfo.SetIsOnIPhone();
                            //}
                            //else
                            //{
                            //    ArchitectureInfo.SetIsNotOnMobile();
                            //}
                        }
                        finally
                        {
                            initialized = true;
                        }
                    }
                }
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeRuntime()
        {
            Initialize();
        }

#if UNITY_EDITOR

        [UnityEditor.InitializeOnLoadMethod]
        private static void InitializeEditor()
        {
            Initialize();
        }
#endif
    }
}