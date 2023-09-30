using System;
using CAS;

namespace LSCore.SDKManagement.Advertisement.CAS
{
    public static partial class Events
    {
        public static class Banner
        {
            public static event Action AdClicked;
            public static event Action<AdMetaData> AdPaid;
            public static event Action AdLoaded;
            public static event Action AdLoadFailed;
            
            public static IAdView Ad
            {
                set
                {
                    value.OnClicked += OnAdClicked;
                    value.OnImpression += OnAdPaid;
                    value.OnLoaded += OnBannerAdLoaded;
                    value.OnFailed += OnBannerAdLoadFailed;
                }
            }

            private static void OnBannerAdLoadFailed(IAdView view, AdError data)
            {
                Burger.Log($"[Events.Banner] OnBannerAdLoadFailed {data}");
                AdLoadFailed?.Invoke();
            }

            private static void OnBannerAdLoaded(IAdView view)
            {
                Burger.Log("[Ads.Events.Banner] OnBannerAdLoaded");
                AdLoaded?.Invoke();
            }

            private static void OnAdPaid(IAdView view, AdMetaData data)
            {
                Burger.Log("[Ads.Events.Banner] OnAdPaid");
                AdPaid?.Invoke(data);
            }

            private static void OnAdClicked(IAdView _)
            {
                Burger.Log("[Ads.Events.Banner] OnAdClicked");
                AdClicked?.Invoke();
            }
        }
    }
}