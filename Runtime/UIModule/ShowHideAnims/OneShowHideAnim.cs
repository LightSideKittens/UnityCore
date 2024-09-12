using System;
using DG.Tweening;
using LSCore.AnimationsModule;

namespace LSCore
{
    [Serializable]
    public class OneShowHideAnim : ShowHideAnim
    {
        public AnimSequencer animation;

        public override void Init() => animation.Init();

        public override Tween Show() => animation.Animate();
        public override Tween Hide() => animation.Animate();
    }
    
    [Serializable]
    public class InOutShowHideAnim : OneShowHideAnim
    {
        public override Tween Hide()
        {
            var tween = base.Hide();
            tween.Goto(tween.Duration(), true);
            tween.PlayBackwards();
            return tween;
        }
    }
}