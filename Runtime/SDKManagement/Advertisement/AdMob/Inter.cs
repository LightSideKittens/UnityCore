using System;
using GoogleMobileAds.Api;
using Sirenix.OdinInspector;

namespace LSCore.SDKManagement.Advertisement.AdMob
{
    [Serializable]
    [InlineProperty(LabelWidth = 30)]
    internal class Inter : Ads.Inter.BaseAdapter
    {
        private InterstitialAd ad;
        protected override string DebugId => "ca-app-pub-3940256099942544/1033173712";
        
        protected override void Internal_Init()
        {
            LoadInterstitialAd();
        }
        
        public void LoadInterstitialAd()
        {
            ad?.Destroy();
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");
            InterstitialAd.Load(Id, adRequest, (ad, error) =>
            {
                if (error != null || ad == null)
                {
                    Burger.Error($"[Ads.Inter] Error:\nId: {Id}\n{error}");
                    return;
                }

                Burger.Log("[Ads.Inter] Loaded");
                this.ad = ad;
                Events.Inter.Ad = ad;
                RegisterReloadHandler(ad);
            });
        }

        public override void Show()
        {
            if (ad != null && ad.CanShowAd())
            {
                ad.Show();
            }
        }
        
        private void RegisterReloadHandler(InterstitialAd ad)
        {
            ad.OnAdFullScreenContentClosed += LoadInterstitialAd;
            ad.OnAdFullScreenContentFailed += _ => LoadInterstitialAd();
        }
    }
}