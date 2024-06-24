using System;
using DG.Tweening;
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
            return target.DOAnchorPos(endValue,Duration);
        }
    }
}