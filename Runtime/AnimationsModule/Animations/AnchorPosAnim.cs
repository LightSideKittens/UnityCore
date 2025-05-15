using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class AnchorPosAnim : BaseAnim<Vector2, RectTransform>
    {
        protected override void InitAction(RectTransform target)
        {
            target.anchoredPosition = startValue;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            return target.DOAnchorPos(endValue, Duration);
        }
    }
    
    [Serializable]
    public class LocalPosAnim : BaseAnim<Vector3, Transform>
    {
        protected override void InitAction(Transform target)
        {
            target.localPosition = startValue;
        }

        protected override Tween AnimAction(Transform target)
        {
            return target.DOLocalMove(endValue, Duration);
        }
    }
}