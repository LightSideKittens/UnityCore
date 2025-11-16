using System;
using LSCore;
using LSCore.ConditionModule;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;

public static class DailyRewardsSave
{
    [Serializable]
    public class CanClaim : If
    {
        public static bool Yes => DeviceTime.Now > NextClaimDateTime;
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
    
    [ResetStatic] private static DeviceTime nextClaimDateTime;
    public static DeviceTime NextClaimDateTime => 
        nextClaimDateTime ??= new DeviceTimeJObject(Config, "nextClaimDateTime").Value;

    public static bool TryClaim()
    {
        var can = CanClaim.Yes;
        
        if (can)
        {
            NextClaimDateTime.Time = DeviceTime.Now + TimeSpan.FromDays(1);
            ClaimedDay++;
        }
        
        return can;
    }
}