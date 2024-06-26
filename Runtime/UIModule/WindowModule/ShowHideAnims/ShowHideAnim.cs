using System;
using DG.Tweening;
using LSCore.AnimationsModule;

namespace LSCore
{
    [Serializable]
    public abstract class ShowHideAnim
    {
        public abstract void Init();
        public abstract Tween Show { get; }
        public abstract Tween Hide { get; }
    }

    public class TwoShowHidAnim : ShowHideAnim
    {
        public AnimSequencer showAnim;
        public AnimSequencer hideAnim;

        public override void Init()
        {
            showAnim.Init();
            hideAnim.Init();
        }

        public override Tween Show => showAnim.Animate();
        public override Tween Hide => hideAnim.Animate();
    }
}