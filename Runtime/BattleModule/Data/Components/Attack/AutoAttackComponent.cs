using System;
using DG.Tweening;
using LSCore.Async;
using UnityEngine;
using UnityEngine.Scripting;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    internal abstract class AutoAttackComponent : BaseAttackComponent
    {
        private FindTargetComp findTargetComp;
        private MoveComp moveComp;
        protected Tween attackLoopEmiter;
        private bool canAttack;

        protected override void OnInit()
        {
            findTargetComp = transform.Get<FindTargetComp>();
            moveComp = transform.Get<MoveComp>();
        }

        protected override void OnEnable()
        {
            attackLoopEmiter = Wait.InfinityLoop(attackSpeed, OnTactTicked);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            attackLoopEmiter.Kill();
        }

        protected abstract void Attack(Transform target);

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