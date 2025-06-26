using System;
using DG.Tweening;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class Wobble : SingleAnim<Transform>
    {
        public float amplitude = 1f;
        public float frequency = 1f;
        public LSVector3.Axis axis = LSVector3.Axis.All; 
        
        protected override Tween AnimAction(Transform target)
        {
            return target.DOWobble(amplitude, frequency, axis);
        }
    }
}