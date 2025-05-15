using System;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class AnimAction : DoIt
    {
        public AnimSequencer anim;
        
        public override void Invoke()
        {
            anim.Animate();
        }
    }
}