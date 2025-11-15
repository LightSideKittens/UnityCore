using System;
using System.ComponentModel;
using DG.Tweening;
using LSCore;
using LSCore.Async;
using LSCore.Extensions;
using UnityEngine;

[Serializable]
public class DailyReward : ViewState.Switcher
{
    [Serializable]
    public class IncreaseWeek : DoIt
    {
        public override void Do()
        {
            if (DailyRewardsSave.ClaimedDay / 7 > DailyRewardsSave.Weeks)
            {
                DailyRewardsSave.Weeks += 1;
            }
        }
    }
    
    public int day;

    protected override string CurrentState
    {
        get
        {
            var claimedDay = DailyRewardsSave.ClaimedDay;
            
            if (DailyRewardsSave.Weeks == claimedDay / 7)
            {
                claimedDay %= 7;
            }
            
            if (claimedDay >= day) return "claimed";
            if(claimedDay == day - 1 && DailyRewardsSave.CanClaim.Yes) return "readyToClaim";
            return DefaultState;
        }
    }

    protected override string DefaultState => "locked";
    protected override string ViewJObjectKey => $"day_{day}";
    
    [Serializable]
    public class ChestSlider : ViewState.BaseSliderChanger
    {
        protected override string ViewJObjectKey => "chest_slider";
        protected override int ActualValue => DailyRewardsSave.ClaimedDay;

        protected override int SavedValue
        {
            get => ViewJObject.As("claimedDay", 0);
            set => ViewJObject["claimedDay"] = value;
        }
    }

    [Serializable]
    public class ClaimButton : DoIt
    {
        [SerializeReference] public DoIt[] claimActionsPerDay;
        [SerializeReference] public DoIt buttonDid;
        
        public LSButton button;
        public GameObject disableState;
        public LocalizationText text;
        private Tween timer;
        private bool lastCan;
        
        public override void Do()
        {
            button.Did += OnDid;
            lastCan = !DailyRewardsSave.CanClaim.Yes;
            UpdateButtonState();
            DailyRewardsSave.Config.PropertyChanged += UpdateButtonState;
            DestroyEvent.AddOnDestroy(button, OnDestroy);
        }

        private void OnDestroy()
        {
            DailyRewardsSave.Config.PropertyChanged -= UpdateButtonState;
            timer?.Kill();
        }

        private void UpdateButtonState(object sender, PropertyChangedEventArgs e) => UpdateButtonState();

        private void UpdateButtonState()
        {
            var can = DailyRewardsSave.CanClaim.Yes;
            if(lastCan == can) return;
            lastCan = can;
            if (can)
            {
                timer?.Kill();
                text.Localize("claim");
            }
            else
            {
                var nextDateTime = DailyRewardsSave.NextClaimDateTime;
                timer = nextDateTime.Seconder(time =>
                {
                    text.Localize("nextRewardInX", time.Timelyze(Timely.Preset.Compact3));
                    if (DailyRewardsSave.CanClaim.Yes)
                    {
                        DailyRewardsSave.NextClaimDateTime.Ticks--;
                    }
                });
            }
            disableState.SetActive(!can);
        }

        private void OnDid()
        {
            var claimedDay = DailyRewardsSave.ClaimedDay;
            if (DailyRewardsSave.TryClaim())
            {
                claimActionsPerDay[claimedDay % 7].Do();
                Analytic.LogEvent("daily_reward_claimed", ("day", claimedDay));
            }
            
            buttonDid.Do();
        }
    }
}