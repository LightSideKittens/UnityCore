using System;
using System.Text;
using GoogleMobileAds.Api;
using UnityEngine;

namespace LSCore.SDKManagement.Advertisement.AdMob
{
    [Serializable]
    public class AdMobInitializer : SDKInitializer.Base
    {
        [SerializeField] private Banner banner;
        [SerializeField] private Inter inter;
        [SerializeField] private Reward reward;

        protected override void Internal_Init(Action<string> onComplete)
        {
            MobileAds.RaiseAdEventsOnUnityMainThread = true;
            MobileAds.Initialize(OnInit);

            void OnInit(InitializationStatus initStatus)
            {
                var builder = new StringBuilder();

                foreach (var (className, status) in initStatus.getAdapterStatusMap())
                {
                    builder.Append(@$"
{className}:
State: {status.InitializationState},
Description: {status.Description},
");
                }

                InitAdPlacements();
                onComplete($"Result: {builder.ToString()}");
            }
        }

        private void InitAdPlacements()
        {
            banner.Init();
            inter.Init();
            reward.Init();
        }
    }
}