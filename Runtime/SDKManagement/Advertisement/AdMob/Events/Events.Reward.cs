using System;
using GoogleMobileAds.Api;

namespace LSCore.SDKManagement.Advertisement.AdMob
{
    public static partial class Events
    {
        public static class Reward
        {
            public static event Action AdClicked;
            public static event Action<AdValue> AdPaid;
            public static event Action AdImpressionRecorded;
            public static event Action AdFullScreenContentClosed;
            public static event Action AdFullScreenContentOpened;
            public static event Action<AdError> AdFullScreenContentFailed;

            public static RewardedAd Ad
            {
                set
                {
                    value.OnAdClicked += OnAdClicked;
                    value.OnAdPaid += OnAdPaid;
                    value.OnAdImpressionRecorded += OnAdImpressionRecorded;
                    value.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
                    value.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
                    value.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;
                }
            }

            private static void OnAdFullScreenContentFailed(AdError obj)
            {
                Burger.Log("[Ads.Events.Reward] OnAdFullScreenContentFailed");
                AdFullScreenContentFailed?.Invoke(obj);
            }

            private static void OnAdFullScreenContentOpened()
            {
                Burger.Log("[Ads.Events.Reward] OnAdFullScreenContentOpened");
                AdFullScreenContentOpened?.Invoke();
            }

            private static void OnAdFullScreenContentClosed()
            {
                Burger.Log("[Ads.Events.Reward] OnAdFullScreenContentClosed");
                AdFullScreenContentClosed?.Invoke();
            }

            private static void OnAdImpressionRecorded()
            {
                Burger.Log("[Ads.Events.Reward] OnAdImpressionRecorded");
                AdImpressionRecorded?.Invoke();
            }

            private static void OnAdPaid(AdValue obj)
            {
                Burger.Log("[Ads.Events.Reward] OnAdPaid");
                AdPaid?.Invoke(obj);
            }

            private static void OnAdClicked()
            {
                Burger.Log("[Ads.Events.Reward] OnAdClicked");
                AdClicked?.Invoke();
            }
        }
    }
}