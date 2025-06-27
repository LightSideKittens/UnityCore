using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class LocalPosByTransformAnim : BaseAnim<Transform, Transform>
    {
        protected override void InitAction(Transform target)
        {
            target.localPosition = startValue.localPosition;
        }

        protected override Tween AnimAction(Transform target)
        {
            return target.DOLocalMove(endValue.localPosition, duration);
        }
    }
}