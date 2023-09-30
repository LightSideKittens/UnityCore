using System;
using CAS;

namespace LSCore.SDKManagement.Advertisement.CAS
{
    public static partial class Events
    {
        public static class Inter
        {
            public static event Action AdClicked;
            public static event Action<AdMetaData> AdPaid;
            public static event Action AdClosed;
            public static event Action<AdMetaData> AdOpened;
            public static event Action<string> AdShowFailed;
            public static event Action<AdError> AdLoadFailed;

            public static IMediationManager Manager
            {
                set
                {
                    value.OnInterstitialAdClicked += OnAdClicked;
                    value.OnInterstitialAdImpression += OnAdPaid;
                    value.OnInterstitialAdClosed += OnAdClosed;
                    value.OnInterstitialAdOpening += OnAdOpened;
                    value.OnInterstitialAdFailedToShow += OnAdFailedShow;
                    value.OnInterstitialAdFailedToLoad += OnAdFailedLoad;
                }
            }
            
            private static void OnAdFailedLoad(AdError obj)
            {
                Burger.Log("[Ads.Events.Inter] OnAdFullScreenContentFailed");
                AdLoadFailed?.Invoke(obj);
            }

            private static void OnAdFailedShow(string obj)
            {
                Burger.Log("[Ads.Events.Inter] OnAdFailedShow");
                AdShowFailed?.Invoke(obj);
            }

            private static void OnAdOpened(AdMetaData obj)
            {
                Burger.Log("[Ads.Events.Inter] OnAdFullScreenContentOpened");
                AdOpened?.Invoke(obj);
            }

            private static void OnAdClosed()
            {
                Burger.Log("[Ads.Events.Inter] OnAdFullScreenContentClosed");
                AdClosed?.Invoke();
            }

            private static void OnAdPaid(AdMetaData obj)
            {
                Burger.Log("[Ads.Events.Inter] OnAdPaid");
                AdPaid?.Invoke(obj);
            }

            private static void OnAdClicked()
            {
                Burger.Log("[Ads.Events.Inter] OnAdClicked");
                AdClicked?.Invoke();
            }
        }
    }
}