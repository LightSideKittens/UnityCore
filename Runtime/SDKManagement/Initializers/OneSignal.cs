#if ONE_SIGNAL
using System;
using UnityEngine;

namespace LSCore.SDKManagement
{
    public partial class SDKInitializer
    {
        [Serializable]
        public class OneSignal : Base
        {
            [SerializeField] private string appId;
            
            protected override void Internal_Init(Action<string> onComplete)
            {
                OneSignalSDK.OneSignal.Initialize(appId);
                onComplete(string.Empty);
            }
        }
    }
}
#endif