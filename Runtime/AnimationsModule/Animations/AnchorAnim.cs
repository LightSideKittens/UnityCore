using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class AnchorAnim : BaseAnim<AnchorAnim.AnchorData, RectTransform>
    {
        [Serializable]
        public struct AnchorData
        {
            public Vector2 min;
            public Vector2 max;
        }
        protected override void InitAction(RectTransform target)
        {
            target.anchorMin = startValue.min;
            target.anchorMax = startValue.max;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            var sequence = DOTween.Sequence();
            sequence.Insert(0, target.DOAnchorMin(endValue.min, Duration));
            sequence.Insert(0, target.DOAnchorMax(endValue.max, Duration));
            return sequence;
        }
    }
}