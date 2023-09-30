using System;
using DG.Tweening;
using UnityEngine.Audio;

namespace LSCore.AnimationsModule.Animations.Audio.MixerGroup
{
    [Serializable]
    public class AudioMixerFloatAnim : BaseAnim<float>
    {
        public AudioMixerGroup target;
        public string key;

        protected override void Internal_Init()
        {
            target.audioMixer.SetFloat(key, startValue);
        }

        protected override Tween Internal_Animate()
        {
            return target.audioMixer.DOSetFloat(key, endValue, duration);
        }
    }
}