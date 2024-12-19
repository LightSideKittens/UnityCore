using System;
using DG.Tweening;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class AttackToTarget : BaseAttack
    {
        [SerializeReference] public BaseUnitAnim anim;
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

        protected override Tween Attack()
        {
            anim.ResolveBinds("target", target);
            return anim.Animate();
        }

        public override void Stop() => anim.Stop();
    }
}