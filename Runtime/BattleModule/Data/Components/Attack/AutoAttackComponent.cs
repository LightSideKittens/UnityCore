using System;
using DG.Tweening;
using LSCore.Async;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.Serialization;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    public class AutoAttackComponent : BaseAttackComponent
    {
        [SerializeField] private FindTargetComp findTargetComp;
        [SerializeField] private bool stillMoveIfTargetInRadius;
        private MoveComp moveComp;
        protected Tween attackLoopEmitter;
        private bool canAttack;

        protected override void OnInit()
        {
            findTargetComp.Init(transform);
            impactObject.canImpactChecker = findTargetComp.Check;
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
                moveComp.enabled = stillMoveIfTargetInRadius;
                
                if (canAttack)
                {
                    canAttack = false;
                    Attack(target);
                }
            }
            else
            {
                moveComp.enabled = true;
            }
        }

        private void OnTactTicked() => canAttack = true;
    }
}