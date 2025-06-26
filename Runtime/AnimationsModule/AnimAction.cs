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
    public class SingleAnimAction : DoIt
    {
        [SerializeReference] public SingleAnim anim;
        
        public override void Do()
        {
            anim.Animate();
        }
    }
}