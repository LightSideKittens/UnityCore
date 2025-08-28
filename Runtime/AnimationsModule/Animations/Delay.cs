using System;
using DG.Tweening;
using LSCore.Async;

namespace LSCore.AnimationsModule.Animations
{
    [Serializable]
    public class Delay : BaseAnim
    {
        public float delay;
        public override bool NeedInit => false;
        protected override void Internal_Init() { }
        
        protected override Tween Internal_Animate()
        {
            return Wait.Delay(delay);
        }
    }
}