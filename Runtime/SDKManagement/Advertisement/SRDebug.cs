#if DEBUG
using System.ComponentModel;
using LSCore.SDKManagement.Advertisement;
using UnityEngine;
using UnityEngine.Scripting;

public class SRDebugData
{
    private static SRDebugData instance = new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Init()
    {
        SRDebug.Instance.AddOptionContainer(instance);
    }
    
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
#endif