using System;
using DG.Tweening;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public struct SetRelative : IOption
    {
        public void ApplyTo(Tween tween)
        {
            tween.SetRelative();
        }
    }
}