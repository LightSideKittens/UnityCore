using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace LSCore.Extensions
{
    public static class DOTweenExt
    {
        public static void Complete(object id) => DOTween.TweensById(id)?.ForEach(x => x.Goto(x.isBackwards ? 0 : 1));
        
        public static Tween DOFloat(this MaterialPropertyBlock target, float duration, int propId, float endValue)
        {
            if (!target.HasProperty(propId))
            {
                if (Debugger.logPriority > 0)
                    Debugger.LogMissingMaterialProperty(propId);
                return null;
            }

            var value = target.GetFloat(propId);
            TweenerCore<float, float, FloatOptions> t = DOTween.To(() => value, x =>
            {
                value = x;
                target.SetFloat(propId, x);
            }, endValue, duration);
            t.SetTarget(target);
            return t;
        }
    }
}