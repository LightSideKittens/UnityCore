//-----------------------------------------------------------------------
// <copyright file="FixUnityAboutWindowBeforeEmit.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Reflection;
    using UnityEditor;

    /// <summary>
    /// <para>This class fixes Unity's about window, by invoking "UnityEditor.VisualStudioIntegration.UnityVSSupport.GetAboutWindowLabel" before any dynamic assemblies have been defined.</para>
    /// <para>This is because dynamic assemblies in the current AppDomain break that method, and Unity's about window depends on it.</para>
    /// </summary>
    [InitializeOnLoad]
    internal static class FixUnityAboutWindowBeforeEmit
    {
        static FixUnityAboutWindowBeforeEmit()
        {
            Fix();
        }

        private static bool isFixed = false;

        public static void Fix()
        {
            if (!isFixed)
            {
                isFixed = true;

                Type unityVSSupport = typeof(Editor).Assembly.GetType("UnityEditor.VisualStudioIntegration.UnityVSSupport");

                if (unityVSSupport != null)
                {
                    MethodInfo getAboutWindowLabel = unityVSSupport.GetMethod("GetAboutWindowLabel", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);

                    if (getAboutWindowLabel != null)
                    {
                        try
                        {
                            getAboutWindowLabel.Invoke(null, null);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}
#endif