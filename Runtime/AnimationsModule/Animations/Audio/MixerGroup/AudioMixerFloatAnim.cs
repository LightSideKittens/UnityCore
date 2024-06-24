using System;
using DG.Tweening;
using UnityEngine.Audio;

namespace LSCore.AnimationsModule.Animations.Audio.MixerGroup
{
    [Serializable]
    public class AudioMixerFloatAnim : BaseAnim<float, AudioMixerGroup>
    {
        public string key;

        protected override void InitAction(AudioMixerGroup target)
        {
            target.audioMixer.SetFloat(key, startValue);
        }

        protected override Tween AnimAction(AudioMixerGroup target)
        {
            return target.audioMixer.DOSetFloat(key, endValue,Duration);
        }
    }
}