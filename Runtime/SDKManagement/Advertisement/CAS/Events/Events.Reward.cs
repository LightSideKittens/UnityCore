using System;
using CAS;

namespace LSCore.SDKManagement.Advertisement.CAS
{
    public static partial class Events
    {
        public static class Reward
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
                    value.OnRewardedAdClicked += OnAdClicked;
                    value.OnRewardedAdImpression += OnAdPaid;
                    value.OnRewardedAdClosed += OnAdClosed;
                    value.OnRewardedAdOpening += OnAdOpened;
                    value.OnRewardedAdFailedToShow += OnAdFailedShow;
                    value.OnRewardedAdFailedToLoad += OnAdFailedLoad;
                }
            }

            private static void OnAdFailedLoad(AdError obj)
            {
                Burger.Log($"[Ads.Events.Reward] {nameof(OnAdFailedLoad)}");
                AdLoadFailed?.Invoke(obj);
            }

            private static void OnAdFailedShow(string obj)
            {
                Burger.Log($"[Ads.Events.Reward] {nameof(OnAdFailedShow)}");
                AdShowFailed?.Invoke(obj);
            }

            private static void OnAdOpened(AdMetaData obj)
            {
                Burger.Log($"[Ads.Events.Reward] {nameof(OnAdOpened)}");
                AdOpened?.Invoke(obj);
            }

            private static void OnAdClosed()
            {
                Burger.Log($"[Ads.Events.Reward] {nameof(OnAdClosed)}");
                AdClosed?.Invoke();
            }

            private static void OnAdPaid(AdMetaData obj)
            {
                Burger.Log($"[Ads.Events.Reward] {nameof(OnAdPaid)}");
                AdPaid?.Invoke(obj);
            }

            private static void OnAdClicked()
            {
                Burger.Log($"[Ads.Events.Reward] {nameof(OnAdClicked)}");
                AdClicked?.Invoke();
            }
        }
    }
}