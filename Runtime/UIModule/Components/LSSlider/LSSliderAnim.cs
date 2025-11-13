using System;
using DG.Tweening;
using LSCore.Async;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class LSSliderAnim : BaseAnim<float, LSSlider>
    {
        protected override void InitAction(LSSlider target)
        {
            target.value = startValue;
        }

        protected override Tween AnimAction(LSSlider target)
        {
            return target.DOValue(endValue, duration);
        }
    }
}