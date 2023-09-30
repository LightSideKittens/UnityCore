using System.ComponentModel;
using LSCore.SDKManagement.Advertisement;
using UnityEngine.Scripting;

public partial class SROptions
{
    [Category("Ads/Banner")]
    [Preserve]
    public void ShowBanner()
    {
        Ads.Banner.Show();
    }
    
    [Category("Ads/Banner")]
    [Preserve]
    public void HideBanner()
    {
        Ads.Banner.Hide();
    }
    
    [Category("Ads/Inter")]
    [Preserve]
    public void ShowInter()
    {
        Ads.Inter.Show();
    }
    
    [Category("Ads/Reward")]
    [Preserve]
    public void ShowReward()
    {
        Ads.Reward.Show(result => Burger.Log($"[Debugger] Reward Ads {result}"));
    }
}