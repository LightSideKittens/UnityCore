using System;
using DG.Tweening;

namespace LSCore.AnimationsModule.Animations.Options
{
    [Serializable]
    public class SetDelay : IOption
    {
        public float delay;
        
        public void ApplyTo(Tween tween)
        {
            tween.SetDelay(delay);
        }
    }
}