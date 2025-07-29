using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class ShakeRotation : SingleAnim<Transform>
    {
        public float duration;
        public Vector3 strength = Vector3.one * 90;
        public int vibrato = 10;
        public float randomness = 90f;
        public bool fadeOut = true;
        public ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full;
        
        protected override Tween AnimAction(Transform target)
        {
            return target.DOShakeRotation(duration, strength, vibrato, randomness, fadeOut, randomnessMode);
        }
    }
    
    [Serializable]
    public class ShakePosition : SingleAnim<Transform>
    {
        public float duration;
        public Vector3 strength = Vector3.one * 90;
        public int vibrato = 10;
        public float randomness = 90f;
        public bool fadeOut = true;
        public ShakeRandomnessMode randomnessMode = ShakeRandomnessMode.Full;
        
        protected override Tween AnimAction(Transform target)
        {
            var tween = target.DOShakePosition(duration, strength, vibrato, randomness, false, fadeOut, randomnessMode);
            return tween;
        }
    }
}