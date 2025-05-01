//-----------------------------------------------------------------------
// <copyright file="UnityNetworkingUtility.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Internal
{
#pragma warning disable

    using Sirenix.Utilities;
    using System;

    /// <summary>
    /// Contains references to UnityEngine.Networking types. These types have been removed in Unity 2019+, and thus may be null.
    /// </summary>
    internal static class UnityNetworkingUtility
    {
        public readonly static Type NetworkBehaviourType;
        public readonly static Type SyncListType;

        private readonly static Func<object, int> getNetworkChannelMethod;
        private readonly static Func<object, float> getNetworkIntervalMethod;

        static UnityNetworkingUtility()
        {
            NetworkBehaviourType = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.Networking.NetworkBehaviour");
            SyncListType = AssemblyUtilities.GetTypeByCachedFullName("UnityEngine.Networking.SyncList`1");

            if (NetworkBehaviourType != null)
            {
                getNetworkChannelMethod = DeepReflection.CreateWeakInstanceValueGetter<int>(NetworkBehaviourType, "GetNetworkChannel()");
                getNetworkIntervalMethod = DeepReflection.CreateWeakInstanceValueGetter<float>(NetworkBehaviourType, "GetNetworkSendInterval()");
            }
        }

        public static bool IsUnityEngineNetworkingAvailable
        {
            get { return NetworkBehaviourType != null; }
        }

        public static int GetNetworkChannel(UnityEngine.MonoBehaviour networkBehaviour)
        {
            if (IsUnityEngineNetworkingAvailable == false)
            {
                throw new InvalidOperationException("UnityEngine.Networking is not available!");
            }
            if (networkBehaviour == null)
            {
                throw new ArgumentNullException("networkBehaviour");
            }
            if (NetworkBehaviourType.IsAssignableFrom(networkBehaviour.GetType()) == false)
            {
                throw new InvalidCastException("networkBehaviour object does not inherit from UnityEngine.Networking.NetworkBehaviour!");
            }

            return getNetworkChannelMethod(networkBehaviour);
        }

        public static float GetNetworkingInterval(UnityEngine.MonoBehaviour networkBehaviour)
        {
            if (IsUnityEngineNetworkingAvailable == false)
            {
                throw new InvalidOperationException("UnityEngine.Networking is not available!");
            }
            if (networkBehaviour == null)
            {
                throw new ArgumentNullException("networkBehaviour");
            }
            if (NetworkBehaviourType.IsAssignableFrom(networkBehaviour.GetType()) == false)
            {
                throw new InvalidCastException("networkBehaviour object does not inherit from UnityEngine.Networking.NetworkBehaviour!");
            }

            return getNetworkIntervalMethod(networkBehaviour);
        }
    }
}
#endif