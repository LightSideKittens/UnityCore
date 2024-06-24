using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class ScaleAnim : BaseAnim<Vector3, Transform>
    {
        protected override void InitAction(Transform target)
        {
            target.localScale = startValue;
        }
        
        protected override Tween AnimAction(Transform target)
        {
            return target.DOScale(endValue,Duration);
        }
    }
}