using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using LSCore.AnimationsModule;
using LSCore.Async;
using Sirenix.OdinInspector;
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
        [HideIf("@animation != null")]
        [SerializeField] private AnimationWrapper wrapper;
        [HideIf("@wrapper != null")]
        [SerializeField] private Animation animation;
        
        [ValueDropdown("Clips")]
        [SerializeField] private AnimationClip clip;
        [NonSerialized] public bool inRadius;
        [NonSerialized] public Transform target;

        private IEnumerable<AnimationClip> Clips => from AnimationState state in Anim select state.clip;

        private Animation Anim
        {
            get
            {
                if (animation != null) return animation;
                return wrapper.Animation;
            }
        }

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
            Play();
            return tween;
        }

        private void Play()
        {
            if (animation != null)
            {
                animation.Play(clip.name);
            }
            else
            {
                wrapper.Play(clip.name);
            }
        }
    }
}