//-----------------------------------------------------------------------
// <copyright file="CleanupUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Utilities.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;
    using UnityEditor;

    public static class CleanupUtility
    {
        private static List<UnityEngine.Object> toCleanUpUnityObjects = new List<UnityEngine.Object>();
        private static List<WeakReference> toCleanUpDisposables = new List<WeakReference>();

        static CleanupUtility()
        {
            AssemblyReloadEvents.beforeAssemblyReload += CleanUp;
        }

        public static void DestroyObjectOnAssemblyReload(UnityEngine.Object unityObj)
        {
            if (unityObj == null) return;
            toCleanUpUnityObjects.Add(unityObj);
        }


        public static void DisposeObjectOnAssemblyReload(IDisposable disposable)
        {
            if (disposable == null) return;
            toCleanUpDisposables.Add(new WeakReference(disposable));
        }

        private static void CleanUp()
        {
            foreach (var unityObj in toCleanUpUnityObjects)
            {
                try
                {
                    if (unityObj != null)
                    {
                        UnityEngine.Object.DestroyImmediate(unityObj);
                    }
                }
                catch { }
            }

            foreach (var reference in toCleanUpDisposables)
            {

                try
                {
                    IDisposable disposable = reference.Target as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
                catch { }
            }

            toCleanUpUnityObjects.Clear();
            toCleanUpDisposables.Clear();
        }
    }
}
#endif