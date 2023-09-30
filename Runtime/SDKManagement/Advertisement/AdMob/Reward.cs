using System;
using GoogleMobileAds.Api;
using Sirenix.OdinInspector;
using static LGCore.SDKManagement.Advertisement.Ads.Reward;

namespace LSCore.SDKManagement.Advertisement.AdMob
{
    [Serializable]
    [InlineProperty(LabelWidth = 30)]
    internal class Reward : BaseAdapter
    {
        private RewardedAd ad;
        private Action<Result> callback;
        protected override string DebugId => "ca-app-pub-3940256099942544/5224354917";
        
        protected override void Internal_Init()
        {
            LoadRewardedAd();
        }
        
        public void LoadRewardedAd()
        {
            ad?.Destroy();
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");
            
            RewardedAd.Load(Id, adRequest,
                (ad, error) =>
                {
                    if (error != null || ad == null)
                    {
                        Burger.Error($"[Ads.Reward] Error:\nId: {Id}\n{error}");
                        return;
                    }
                    
                    Burger.Log($"[Ads.Reward] Loaded");
                    this.ad = ad;
                    Events.Reward.Ad = ad;
                    RegisterReloadHandler(ad);
                });
        }

        public override void Show(Action<Result> onResult)
        {
            callback = onResult;
            callback += ResetCallback;
            
            if (ad != null && ad.CanShowAd())
            {
                ad.Show(_ =>
                {
                    callback?.Invoke(Result.Received);
                });
            }
            else
            {
                callback?.Invoke(Result.FailedDisplay);
            }
        }

        private void ResetCallback(Result _)
        {
            callback = null;
        }

        private void RegisterReloadHandler(RewardedAd ad)
        {
            ad.OnAdFullScreenContentClosed += OnClosed;
            ad.OnAdFullScreenContentFailed += OnDisplayFailed;
        }

        private void OnDisplayFailed(AdError obj)
        {
            callback?.Invoke(Result.FailedDisplay);
            LoadRewardedAd();
        }

        private void OnClosed()
        {
            LoadRewardedAd();
        }
    }
}