using System;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class AnimAction : LSAction
    {
        public AnimSequencer anim;
        
        public override void Invoke()
        {
            anim.Animate();
        }
    }
}