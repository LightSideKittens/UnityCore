using System;
using DG.Tweening;
using LSCore.Async;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    public class AutoAttackComponent : BaseAttackComponent
    {
        private FindTargetComp findTargetComp;
        private MoveComp moveComp;
        protected Tween attackLoopEmitter;
        private bool canAttack;

        protected override void OnInit()
        {
            findTargetComp = transform.Get<FindTargetComp>();
            moveComp = transform.Get<MoveComp>();
        }

        protected override void OnEnable()
        {
            attackLoopEmitter = Wait.InfinityLoop(attackSpeed, OnTactTicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            attackLoopEmitter.Kill();
        }

        protected void Attack(Transform target)
        {
            impactObject.transform.up = target.position - transform.position;
            impactObject.Emit();
        }

        public override void Update()
        {
            base.Update();
            
            if (findTargetComp.Find(radius, out var target))
            {
                moveComp.SetEnabled(false);
                if (canAttack)
                {
                    Attack(target);
                    canAttack = false;
                }
            }
            else
            {
                moveComp.SetEnabled(true);
            }
        }

        private void OnTactTicked() => canAttack = true;
    }
}