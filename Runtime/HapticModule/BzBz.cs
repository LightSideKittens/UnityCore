using System;
using Lofelt.NiceVibrations;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

public static class BzBz
{
    public enum Preset
    {
        Selection = 0,
        Success = 1,
        Warning = 2,
        Failure = 3,
        Light = 4,
        Medium = 5,
        Heavy = 6,
        Rigid = 7,
        Soft = 8,
        None = -1
    }
    
    [Serializable]
    public class Muter : BaseToggleData
    {
        public static event Action<bool> Changed;
        protected override bool Get => Unmuted;

        protected override bool Set
        {
            set
            {
                HapticController.hapticsEnabled = value;
                Unmuted = value;
            }
        }
    }
    
    public static JObject Config => config ?? JTokenGameConfig.Get("BzBzSettings");

    public static bool Unmuted
    {
        get => Config.As("unmuted", true);
        set => Config["unmuted"] = value;
    }
    
    private static JObject config;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        HapticController.Init();
        var muter = new Muter();
        muter.IsOn = Unmuted;
    }

    public static void Play(this Preset preset)
    {
        HapticPatterns.PlayPreset((HapticPatterns.PresetType)preset);
    }
}
