using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class SizeDeltaAnim : BaseAnim<Vector2, RectTransform>
    {
        protected override void InitAction(RectTransform target)
        {
            target.sizeDelta = startValue;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            return target.DOSizeDelta(endValue,Duration);
        }
    }
}