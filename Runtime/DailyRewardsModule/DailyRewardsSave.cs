using System;
using LSCore.ConditionModule;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;

public static class DailyRewardsSave
{
    [Serializable]
    public class CanClaim : If
    {
        public static bool Yes => DateTime.Now.Ticks > NextClaimDateTime;
        protected override bool Check() => Yes;
    }
    
    public static JObject Config => JTokenGameConfig.Get("DailyRewards");
    
    public static int ClaimedDay
    {
        get => Config.As("claimedDay", 0);
        set => Config["claimedDay"] = value;
    }
    
    public static int Weeks
    {
        get => Config.As("weeks", 0);
        set => Config["weeks"] = value;
    }
    
    public static long NextClaimDateTime
    {
        get => Config.As("nextClaimDateTime", DateTime.Now.Ticks - 1);
        set => Config["nextClaimDateTime"] = value;
    }

    public static bool TryClaim()
    {
        var can = CanClaim.Yes;
        
        if (can)
        {
            var now = DateTime.Now.Ticks;
            NextClaimDateTime = now + TimeSpan.FromDays(1).Ticks;
            ClaimedDay++;
        }
        
        return can;
    }
}