using System;
using DG.Tweening;
using UnityEngine.UI;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class FilledAmountAnim : BaseAnim<float, Image>
    {
        protected override void InitAction(Image target)
        {
            target.fillAmount = startValue;
        }

        protected override Tween AnimAction(Image target)
        {
            return target.DOFillAmount(endValue, Duration);
        }
    }
}