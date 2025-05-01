//-----------------------------------------------------------------------
// <copyright file="OdinEditorWebUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Internal
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using UnityEditor;
    using UnityEngine;
    using UnityEngine.Networking;

    public static class OdinEditorWebUtility
    {
        private static readonly MethodInfo UnityWebRequest_SendWebRequest_Method;
        private static readonly EventInfo AsyncOperation_Completed_Event;

        private static readonly PropertyInfo UnityWebRequest_IsError_Property;
        private static readonly PropertyInfo UnityWebRequest_IsHttpError_Property;
        private static readonly PropertyInfo UnityWebRequest_IsNetworkError_Property;
        private static readonly PropertyInfo UnityWebRequest_Result_Property;

        public static readonly bool RequiredUnityApiIsAvailable = true;

        static OdinEditorWebUtility()
        {
            UnityWebRequest_SendWebRequest_Method =
                typeof(UnityWebRequest).GetMethod("SendWebRequest", Flags.InstancePublic, null, new Type[] { }, null)
                ?? typeof(UnityWebRequest).GetMethod("Send", Flags.InstancePublic, null, new Type[] { }, null);

            if (UnityWebRequest_SendWebRequest_Method == null)
            {
                RequiredUnityApiIsAvailable = false;
            }

            AsyncOperation_Completed_Event = typeof(AsyncOperation).GetEvent("completed", Flags.InstancePublic);

            UnityWebRequest_IsError_Property = typeof(UnityWebRequest).GetProperty("isError", Flags.InstancePublic);
            UnityWebRequest_IsHttpError_Property = typeof(UnityWebRequest).GetProperty("isHttpError", Flags.InstancePublic);
            UnityWebRequest_IsNetworkError_Property = typeof(UnityWebRequest).GetProperty("isNetworkError", Flags.InstancePublic);
            UnityWebRequest_Result_Property = typeof(UnityWebRequest).GetProperty("result", Flags.InstancePublic);

            if (UnityWebRequest_IsError_Property == null && UnityWebRequest_IsHttpError_Property == null && UnityWebRequest_IsNetworkError_Property == null && UnityWebRequest_Result_Property == null)
            {
                RequiredUnityApiIsAvailable = false;
            }
        }

        public static AsyncOperation SendWebRequest(UnityWebRequest request)
        {
            return (AsyncOperation)UnityWebRequest_SendWebRequest_Method.Invoke(request, null);
        }

        public static bool RequestIsError(UnityWebRequest request)
        {
            if (UnityWebRequest_Result_Property != null)
            {
                object value = UnityWebRequest_Result_Property.GetValue(request, null);
                int enumValue = Convert.ToInt32(value);

                // 0 = InProgress
                // 1 = Success
                // Rest = errors of various kinds
                return enumValue > 1;
            }

            if (UnityWebRequest_IsNetworkError_Property != null && (bool)UnityWebRequest_IsNetworkError_Property.GetValue(request, null))
            {
                return true;
            }

            if (UnityWebRequest_IsHttpError_Property != null && (bool)UnityWebRequest_IsHttpError_Property.GetValue(request, null))
            {
                return true;
            }

            if (UnityWebRequest_IsError_Property != null && (bool)UnityWebRequest_IsError_Property.GetValue(request, null))
            {
                return true;
            }

            return false;
        }

        public static void SubscribeOnCompleted(AsyncOperation operation, Action<AsyncOperation> action)
        {
            if (operation.isDone)
            {
                try
                {
                    action(operation);
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
                return;
            }

            if (AsyncOperation_Completed_Event != null)
            {
                AsyncOperation_Completed_Event.AddEventHandler(operation, action);
            }
            else
            {
                EditorApplication.CallbackFunction update = null;

                update = () =>
                {
                    if (operation.isDone)
                    {
                        EditorApplication.update -= update;
                        try
                        {
                            action(operation);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                        }
                    }
                };

                EditorApplication.update += update;
            }
        }
    }
}
#endif