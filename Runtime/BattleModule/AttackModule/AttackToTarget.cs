using System;
using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.Async;
using UnityEngine;

namespace LSCore
{
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
    
    public class AttackToTargetAnimation : BaseAttack
    {
        [SerializeField] private AnimationWrapper animation;
        [SerializeField] private AnimationClip clip;
        [SerializeField] private bool needFaceToTarget = true;
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
            var sequence = DOTween.Sequence().Append(Wait.Delay(clip.length));
            
            if (needFaceToTarget)
            {
                sequence.Append();
            }
            
            animation.Play(clip.name);
            return sequence;
        }
    }
}