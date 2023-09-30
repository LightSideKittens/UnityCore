#if APPSFLYER
using System;
using UnityEngine;

namespace LSCore.SDKManagement
{
    public partial class SDKInitializer
    {
        [Serializable]
        public class AppsFlyer : Base
        {
            [SerializeField] private bool isDebug;
            [SerializeField] private string appsFlyerDevKey = "YOUR_APPSFLYER_DEV_KEY";
            [SerializeField] private string appId;
                
            protected override void Internal_Init(Action<string> onComplete)
            {
                AppsFlyerSDK.AppsFlyer.initSDK(appsFlyerDevKey, appId);
                AppsFlyerSDK.AppsFlyer.setIsDebug(isDebug);
                AppsFlyerSDK.AppsFlyer.startSDK();
                onComplete(string.Empty);
            }
        }
    }
}
#endif