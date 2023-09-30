using System;
using CAS;
using Sirenix.OdinInspector;

namespace LSCore.SDKManagement.Advertisement.CAS
{
    [Serializable]
    [InlineProperty(LabelWidth = 30)]
    internal class Inter : Ads.Inter.BaseAdapter
    {
        protected override bool HasId => false;

        protected override void Internal_Init()
        {
            Events.Inter.Manager = CASInitializer.Manager;
        }

        public override void Show()
        {
            CASInitializer.Manager.ShowAd(AdType.Interstitial);
        }
    }
}