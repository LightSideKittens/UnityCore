using DG.Tweening;
using UnityEngine.UI;

namespace LSCore.AnimationsModule.Animations
{
    public class SliderAnim : BaseAnim<float, Slider>
    {
        protected override void InitAction(Slider target)
        {
            target.value = startValue;
        }

        protected override Tween AnimAction(Slider target)
        {
            return target.DOValue(endValue, Duration);
        }
    }
}