using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class ShakeRotation : SingleAnim<Transform>
    {
        public float duration;
        public float strength = 90f;
        public int vibrato = 10;
        public float randomness = 90f;
        public bool fadeOut = true;
        public ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full;
        
        protected override Tween AnimAction(Transform target)
        {
            return target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut, randomnessMode);
        }
    }
}