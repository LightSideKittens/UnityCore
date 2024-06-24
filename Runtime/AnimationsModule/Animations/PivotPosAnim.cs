using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class PivotPosAnim : BaseAnim<Vector2, RectTransform>
    {
        protected override void InitAction(RectTransform target)
        {
            target.pivot = startValue;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            return target.DOPivot(endValue,Duration);
        }
    }
    
    [Serializable]
    public class PivotPosXAnim : BaseAnim<float, RectTransform>
    {
        protected override void InitAction(RectTransform target)
        {
            var p = target.pivot;
            p.x = startValue;
            target.pivot = p;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            return target.DOPivotX(endValue,Duration);
        }
    }
    
    [Serializable]
    public class PivotPosYAnim : BaseAnim<float, RectTransform>
    {
        protected override void InitAction(RectTransform target)
        {
            var p = target.pivot;
            p.y = startValue;
            target.pivot = p;
        }

        protected override Tween AnimAction(RectTransform target)
        {
            return target.DOPivotY(endValue,Duration);
        }
    }
}