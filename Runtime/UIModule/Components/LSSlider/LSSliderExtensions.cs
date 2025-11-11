using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using LSCore;

public static class LSSliderExtensions
{
    public static Tween DOValue(this LSSlider target, float endValue, float duration)
    {
        TweenerCore<float, float, FloatOptions> t = DOTween.To(() => target.value, x => target.value = x, endValue, duration);
        t.SetTarget(target);
        return t;
    } 
}