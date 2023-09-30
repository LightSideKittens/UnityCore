using System;
using GoogleMobileAds.Api;

namespace LSCore.SDKManagement.Advertisement.AdMob
{
    public static partial class Events
    {
        public static class Banner
        {
            public static event Action AdClicked;
            public static event Action<AdValue> AdPaid;
            public static event Action AdImpressionRecorded;
            public static event Action AdFullScreenContentClosed;
            public static event Action AdFullScreenContentOpened;
            public static event Action BannerAdLoaded;
            public static event Action BannerAdLoadFailed;
            
            public static BannerView Ad
            {
                set
                {
                    value.OnAdClicked += OnAdClicked;
                    value.OnAdPaid += OnAdPaid;
                    value.OnAdImpressionRecorded += OnAdImpressionRecorded;
                    value.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
                    value.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
                    value.OnBannerAdLoaded += OnBannerAdLoaded;
                    value.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
                }
            }

            private static void OnBannerAdLoadFailed(LoadAdError obj)
            {
                Burger.Log("[Events.Banner] OnBannerAdLoadFailed");
                BannerAdLoadFailed?.Invoke();
            }

            private static void OnBannerAdLoaded()
            {
                Burger.Log("[Ads.Events.Banner] OnBannerAdLoaded");
                BannerAdLoaded?.Invoke();
            }

            private static void OnAdFullScreenContentOpened()
            {
                Burger.Log("[Ads.Events.Banner] OnAdFullScreenContentOpened");
                AdFullScreenContentOpened?.Invoke();
            }

            private static void OnAdFullScreenContentClosed()
            {
                Burger.Log("[Ads.Events.Banner] OnAdFullScreenContentClosed");
                AdFullScreenContentClosed?.Invoke();
            }

            private static void OnAdImpressionRecorded()
            {
                Burger.Log("[Ads.Events.Banner] OnAdImpressionRecorded");
                AdImpressionRecorded?.Invoke();
            }

            private static void OnAdPaid(AdValue obj)
            {
                Burger.Log("[Ads.Events.Banner] OnAdPaid");
                AdPaid?.Invoke(obj);
            }

            private static void OnAdClicked()
            {
                Burger.Log("[Ads.Events.Banner] OnAdClicked");
                AdClicked?.Invoke();
            }
        }
    }
}