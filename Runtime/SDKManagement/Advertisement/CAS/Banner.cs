using System;
using CAS;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.SDKManagement.Advertisement.CAS
{
    [Serializable]
    [InlineProperty(LabelWidth = 30)]
    internal class Banner : Ads.Banner.BaseAdapter
    {
        [SerializeField] private AdSize size;
        private IAdView bannerView;
        protected override bool HasId => false;

        protected override void Internal_Init()
        {
            bannerView = CASInitializer.Manager.GetAdView(size);
            
            bannerView.SetActive(false);
            Events.Banner.Ad = bannerView;
        }

        public override void Show()
        {
            bannerView.SetActive(true);
        }

        public override void Hide()
        {
            bannerView.SetActive(false);
        }
    }
}