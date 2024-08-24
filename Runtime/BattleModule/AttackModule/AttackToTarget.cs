using System;
using DG.Tweening;
using LSCore.AnimationsModule;
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
}