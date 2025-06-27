using System;
using LSCore.AnimationsModule.Animations;
using UnityEngine;

namespace LSCore.AnimationsModule
{
    [Serializable]
    public class AnimAction : DoIt
    {
        public AnimSequencer anim;
        
        public override void Do()
        {
            anim.Animate();
        }
    }
    
    [Serializable]
    public class BaseAnimAction : DoIt
    {
        [SerializeReference] public BaseAnim anim;
        
        public override void Do()
        {
            anim.Animate();
        }
    }
}