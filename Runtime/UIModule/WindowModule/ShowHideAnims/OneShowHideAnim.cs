using System;
using DG.Tweening;
using LSCore.AnimationsModule;

namespace LSCore
{
    [Serializable]
    public class InOutShowHideAnim : ShowHideAnim
    {
        public AnimSequencer animation;

        public override void Init() => animation.Init();

        public override Tween Show => animation.Animate();
        public override Tween Hide
        {
            get
            {
                var tween = animation.Animate();
                tween.Goto(tween.Duration(), true);
                tween.PlayBackwards();
                return tween;
            }
        }
    }
}