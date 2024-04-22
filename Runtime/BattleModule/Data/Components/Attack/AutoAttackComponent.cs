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
        private MoveComp moveComp;
        protected Tween attackLoopEmitter;
        private bool canAttack;

        protected override void OnInit()
        {
            moveComp = transform.Get<MoveComp>();
            
            if (!manualMoveCompControl)
            {
                data.update += ControlMove;
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
            impactObject.LookAt(target);
            impactObject.Emit();
        }

        protected override void Update()
        {
            base.Update(); 
            InRadius = findTargetComp.Find(radius, out var target);
            
            if (InRadius)
            {
                if (canAttack)
                {
                    canAttack = false;
                    Attack(target);
                }
            }
        }
        
        private void OnTactTicked() => canAttack = true;
    }
}