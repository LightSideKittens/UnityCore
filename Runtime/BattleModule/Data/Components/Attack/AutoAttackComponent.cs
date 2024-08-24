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
        [SerializeField] private bool manualMoveCompControl;
        
        public bool InRadius { get; private set; }
        public float radius;
        private BaseMoveComp moveComp;
        protected Tween attackLoopEmitter;
        private bool canAttack;

        protected override void OnInit()
        {
            useFixedUpdate = true;
            IsRunning = true;
            moveComp = transform.Get<BaseMoveComp>();
            
            if (!manualMoveCompControl)
            {
                data.fixedUpdate += ControlMove;
            }
        }

        private void ControlMove()
        {
            moveComp.IsRunning = !InRadius;
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

        private void Attack(Transform target)
        {
            canAttack = false;
            impactObject.LookAt(target);
            impactObject.Emit();
        }

        protected override void FixedUpdate()
        {
            base.FixedUpdate();
            if (canAttack)
            {
                InRadius = findTargetComp.Find(radius, out var target);

                if (InRadius)
                {
                    Attack(target);
                }
            }
        }
        
        private void OnTactTicked() => canAttack = true;
    }
}