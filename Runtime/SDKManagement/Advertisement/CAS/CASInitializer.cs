using System;
using CAS;
using UnityEngine;

namespace LSCore.SDKManagement.Advertisement.CAS
{
    [Serializable]
    public class CASInitializer : SDKInitializer.Base
    {
        [SerializeField] private Banner banner;
        [SerializeField] private Inter inter;
        [SerializeField] private Reward reward;
        public static IMediationManager Manager { get; private set; }
        
        protected override void Internal_Init(Action<string> onComplete)
        {
            Manager = MobileAds.BuildManager().Build();
            InitAdPlacements();
            onComplete(string.Empty);
        }

        private void InitAdPlacements()
        {
            banner.Init();
            inter.Init();
            reward.Init();
        }
    }
}