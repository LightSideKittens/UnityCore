using static LSCore.SDKManagement.Advertisement.Ads.Reward;

namespace LSCore.SDKManagement.Advertisement
{
    public static class RewardExtensions
    {
        public static bool IsReceived(this Ads.Reward.Result result) => result == Ads.Reward.Result.Received;

        public static bool IsClosed(this Ads.Reward.Result result) => result == Ads.Reward.Result.Closed;

        public static bool IsFailedDisplay(this Ads.Reward.Result result) => result == Ads.Reward.Result.FailedDisplay;
    }
}