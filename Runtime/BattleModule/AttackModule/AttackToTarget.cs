using System;
using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.Async;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class AttackToTarget : BaseAttack
    {
        [SerializeField] private AnimSequencer anim;
        [NonSerialized] public bool inRadius;
        [NonSerialized] public Transform target;
        
        public override bool AttackCondition
        {
            get
            {
                inRadius = findTargetComp.Find(out target);
                return inRadius;
            }
        }

        public override Tween Attack()
        {
            anim.ResolveBinds("target", target);
            return anim.Animate();
        }
    }
    
    [Serializable]
    public class AttackToTargetAnimation : BaseAttack
    {
        [SerializeField] private AnimationWrapper animation;
        [SerializeField] private AnimationClip clip;
        [NonSerialized] public bool inRadius;
        [NonSerialized] public Transform target;
        
        public override bool AttackCondition
        {
            get
            {
                inRadius = findTargetComp.Find(out target);
                return inRadius;
            }
        }

        public override Tween Attack()
        {
            var tween = Wait.Delay(clip.length);
            animation.Play(clip.name);
            return tween;
        }
    }
}