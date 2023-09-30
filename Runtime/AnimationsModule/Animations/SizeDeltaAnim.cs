using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class SizeDeltaAnim : BaseAnim<Vector2>
    {
        public RectTransform target;

        protected override void Internal_Init()
        {
            target.sizeDelta = startValue;
        }

        protected override Tween Internal_Animate()
        {
            return target.DOSizeDelta(endValue, duration);
        }
    }
}