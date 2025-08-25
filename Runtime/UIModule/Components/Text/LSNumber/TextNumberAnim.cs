using System;
using DG.Tweening;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations.Text
{
    [Serializable]
    public class TextNumberAnim : BaseAnim<int, LSNumber>
    {
        protected override void InitAction(LSNumber target)
        {
            target.Number = startValue;
        }

        protected override Tween AnimAction(LSNumber target)
        {
            return DOVirtual.Float(startValue, endValue, duration, value => target.Number = Mathf.CeilToInt(value));
        }
    }
}