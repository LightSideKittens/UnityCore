using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("LGCore.Ads.Admob")]
[assembly: InternalsVisibleTo("LGCore.Ads.CAS")]

namespace LSCore.SDKManagement.Advertisement
{ 
    public static partial class Ads
    {
        public static bool Enabled
        {
            set
            {
                Banner.Enabled = value;
                Inter.Enabled = value;
                Reward.Enabled = value;
            }
        }
    }
}