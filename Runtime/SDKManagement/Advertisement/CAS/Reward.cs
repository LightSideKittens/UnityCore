using System;
using CAS;
using Sirenix.OdinInspector;
using static LSCore.SDKManagement.Advertisement.Ads.Reward;

namespace LSCore.SDKManagement.Advertisement.CAS
{
    [Serializable]
    [InlineProperty(LabelWidth = 30)]
    internal class Reward : Ads.Reward.BaseAdapter
    {
        protected override bool HasId => false;
        private Action<Ads.Reward.Result> callback;

        protected override void Internal_Init()
        {
            var manager = CASInitializer.Manager;
            Events.Reward.Manager = manager;
            manager.OnRewardedAdCompleted += OnRewardAdCompleted;
            manager.OnRewardedAdFailedToShow += OnRewardAdFailed;
        }

        private void OnRewardAdFailed(string error) => callback?.Invoke(Ads.Reward.Result.FailedDisplay);
        private void OnRewardAdCompleted() => callback?.Invoke(Ads.Reward.Result.Received);
        
        public override void Show(Action<Ads.Reward.Result> onResult)
        {
            callback = onResult;
            callback += ResetCallback;
            CASInitializer.Manager.ShowAd(AdType.Rewarded);
        }
        
        private void ResetCallback(Ads.Reward.Result _) => callback = null;
    }
}