using System;
using System.Collections.Generic;
using DG.Tweening;
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
        [NonSerialized] public Transform transform;
        [NonSerialized] public FindTargetComp findTargetComp;

        [NonSerialized] public bool isAttacking;
        public bool CanAttack => !isAttacking && AttackCondition; 
        public abstract bool AttackCondition { get; }

        public void Init(Transform transform, FindTargetComp findTargetComp)
        {
            this.transform = transform;
            this.findTargetComp = findTargetComp;
            SetupImpactObjects(impactObjects);
            OnInit();
        }

        protected virtual void OnInit(){}

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
        
        public abstract Tween Attack();
        
        public virtual void OnAttackCompleted() { }

        public void SetupImpactObject<T>(T impactObject) where T : BaseImpactObject
        {
            impactObject.IgnoredCollider = transform.GetComponent<Collider2D>();
            impactObject.CanImpactChecker = findTargetComp.Check;
            impactObject.Initiator = transform;
        }
        
        public void SetupImpactObjects<T>(IEnumerable<T> impactObjects) where T : BaseImpactObject
        {
            foreach (var impactObject in impactObjects)
            {
                SetupImpactObject(impactObject);
            }
        }
    }
}