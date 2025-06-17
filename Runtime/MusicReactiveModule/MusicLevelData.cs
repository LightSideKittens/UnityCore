using System;
using LSCore;
using MusicEventSystem.Configs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

[Serializable]
public class MusicLevelData
{
    public AssetReferenceT<AudioClip> audioClip;
    public AudioClip AudioClip => (AudioClip)audioClip.Asset;
    public string configName;
    public float startTime;
    [MinValue(1f)]
    public float endTime = MusicData.DefaultEndTime;
}