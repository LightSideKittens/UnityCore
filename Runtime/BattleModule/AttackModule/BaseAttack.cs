using System;
using System.Collections.Generic;
using DG.Tweening;
using LSCore.Async;
using LSCore.Attributes;
using LSCore.BattleModule;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public abstract class BaseAttack
    {
        [SerializeField] public List<BaseImpactObject> impactObjects;
        [SerializeField] public float cooldown;
        [NonSerialized] public Transform transform;
        [NonSerialized] public FindTargetComp findTargetComp;

        [NonSerialized] public bool isAttacking;
        public bool CanAttack => !isAttacking && AttackCondition; 
        public abstract bool AttackCondition { get; }

        public void Init(Transform transform, FindTargetComp findTargetComp)
        {
            this.transform = transform;
            this.findTargetComp = findTargetComp;
            InitImpactObjects(impactObjects);
            OnInit();
        }

        protected virtual void OnInit(){}
        public virtual void DeInit(){}

        public bool TryAttack(out Tween tween)
        {
            var canAttack = CanAttack;
            
            if (canAttack)
            {
                isAttacking = true;
                tween = Attack();
                if (tween != null)
                {
                    tween.OnComplete(OnTweenCompleted);
                }
                else
                {
                    OnTweenCompleted();
                }
            }

            tween = null;
            return canAttack;
            
            void OnTweenCompleted()
            {
                isAttacking = false;
                OnAttackCompleted();
            }
        }

        public (Tween attack, Tween cooldown) Trigger(float additionalCooldown)
        {
            var attackTween = Attack();
            Tween cooldownTween = null;
            
            if (attackTween != null)
            {
                var sequence = DOTween.Sequence().Append(attackTween);
                attackTween = sequence;
                var totalCooldown = cooldown + additionalCooldown;
                
                if (totalCooldown > 0.005f)
                {
                    cooldownTween = Wait.Delay(totalCooldown);
                    sequence.Append(cooldownTween);
                }
            }

            return (attackTween, cooldownTween);
        }
        
        protected abstract Tween Attack();
        public abstract void Stop();
        
        public virtual void OnAttackCompleted() { }

        public void InitImpactObject<T>(T impactObject, Collider2D collider) where T : BaseImpactObject
        {
            impactObject.IgnoredCollider = collider;
            impactObject.CanImpactChecker = findTargetComp.Check;
            impactObject.Initiator = transform;
        }
        
        public void InitImpactObjects<T>(IEnumerable<T> impactObjects) where T : BaseImpactObject
        {
            var collider = transform.GetComponent<Collider2D>();
            
            foreach (var impactObject in impactObjects)
            {
                InitImpactObject(impactObject, collider);
            }
        }
    }
}