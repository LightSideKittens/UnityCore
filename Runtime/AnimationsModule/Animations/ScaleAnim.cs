using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class ScaleAnim : BaseAnim<Vector3>
    {
        public Transform target;

        protected override void Internal_Init()
        {
            target.localScale = startValue;
        }
        
        protected override Tween Internal_Animate()
        {
            return target.DOScale(endValue, duration);
        }
    }
}