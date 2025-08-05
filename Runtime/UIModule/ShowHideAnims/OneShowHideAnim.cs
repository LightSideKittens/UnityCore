using System;
using DG.Tweening;
using LSCore.AnimationsModule;

namespace LSCore
{
    [Serializable]
    public class OneShowHideAnim : ShowHideAnim
    {
        public AnimSequencer animation;

        public override Tween Show() => animation.Animate();
        public override Tween Hide() => animation.Animate();
    }
    
    [Serializable]
    public class InOutShowHideAnim : OneShowHideAnim
    {
        private Tween tween;

        public override Tween Show()
        {
            tween ??= base.Show().SetAutoKill(false);
            tween.PlayForward();
            return tween;
        }

        public override Tween Hide()
        {
            tween ??= Show();
            tween.PlayBackwards();
            return tween;
        }
    }
}