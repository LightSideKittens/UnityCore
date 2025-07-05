using System;
using DG.Tweening;
using LSCore.Async;
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
            return Wait.Delay(duration).OnStart(() =>
            {
                target.DOLocalMove(endValue.localPosition, duration);
            }).SetTarget(endValue);
        }
    }
}