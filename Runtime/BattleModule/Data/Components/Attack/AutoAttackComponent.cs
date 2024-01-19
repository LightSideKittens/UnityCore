using System;
using LSCore.LevelSystem;
using DG.Tweening;
using LSCore.Async;
using UnityEngine;
using UnityEngine.Scripting;
using static Battle.ObjectsByTransfroms<Battle.Data.Components.AutoAttackComponent>;

namespace Battle.Data.Components
{
    [Preserve, Serializable]
    internal abstract class AutoAttackComponent : BaseAttackComponent
    {
        private FindTargetComponent findTargetComponent;
        private MoveComponent moveComponent;
        protected Tween attackLoopEmiter;
        private bool canAttack;

        protected override void OnInit()
        {
            findTargetComponent = transform.Get<FindTargetComponent>();
            moveComponent = transform.Get<MoveComponent>();
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
            
            if (findTargetComponent.Find(radius, out var target))
            {
                moveComponent.SetEnabled(false);
                if (canAttack)
                {
                    Attack(target);
                    canAttack = false;
                }
            }
            else
            {
                moveComponent.SetEnabled(true);
            }
        }

        private void OnTactTicked() => canAttack = true;
    }
}