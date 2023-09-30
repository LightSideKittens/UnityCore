using System;
using GoogleMobileAds.Api;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.SDKManagement.Advertisement.AdMob
{
    [Serializable]
    [InlineProperty(LabelWidth = 30)]
    internal class Banner : Ads.Banner.BaseAdapter
    {
        [SerializeField] private AdPosition position;
        private BannerView bannerView;
        protected override string DebugId => "ca-app-pub-3940256099942544/6300978111";
        
        protected override void Internal_Init()
        {
            bannerView = new BannerView(Id, AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(Screen.width), position);
            var adRequest = new AdRequest();
            adRequest.Keywords.Add("unity-admob-sample");
            bannerView.LoadAd(adRequest);
            bannerView.Hide();
            Events.Banner.Ad = bannerView;
        }

        public override void Show()
        {
            bannerView.Show();
        }

        public override void Hide()
        {
            bannerView.Hide();
        }
    }
}