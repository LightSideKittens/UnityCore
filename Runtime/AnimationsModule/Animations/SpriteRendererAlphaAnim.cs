using System;
using DG.Tweening;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class SpriteRendererAlphaAnim : BaseAnim<float, SpriteRenderer>
    {
        protected override Tween AnimAction(SpriteRenderer target)
        {
            return target.DOFade(endValue, duration);
        }

        protected override void InitAction(SpriteRenderer target)
        {
            target.Alpha(startValue);
        }
    }
}