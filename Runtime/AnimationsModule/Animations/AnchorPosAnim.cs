using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class AnchorPosAnim : BaseAnim<Vector2>
    {
        public RectTransform target;

        protected override void Internal_Init()
        {
            target.anchoredPosition = startValue;
        }

        protected override Tween Internal_Animate()
        {
            return target.DOAnchorPos(endValue, duration);
        }
    }
}