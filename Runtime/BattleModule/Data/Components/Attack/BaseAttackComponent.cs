using System;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    public abstract class BaseAttackComponent : BaseComp
    {
        public PSImpactObject impactObjectPrefab;
        [SerializeField] protected FindTargetFactory findTargetFactory;
        protected FindTargetComp findTargetComp;
        [NonSerialized] public PSImpactObject impactObject;
        public float attackSpeed;

        protected sealed override void Init()
        {
            data.onInit += OnInit;
            data.enable += Enable;
            data.disable += Disable;
            data.destroy += Destroy;
            findTargetComp = findTargetFactory.Create();
            findTargetComp.Init(transform);
            impactObject = Object.Instantiate(impactObjectPrefab, transform, false);
            impactObject.IgnoredCollider = transform.GetComponent<Collider2D>();
            impactObject.CanImpactChecker = findTargetComp.Check;
            impactObject.Initiator = transform;
        }
        
        protected virtual void OnInit(){}

        public void Enable() => OnEnable();
        protected virtual void OnEnable(){}

        public void Disable() => OnDisable();
        protected virtual void OnDisable(){}

        public void Destroy()
        {
            Disable();
        }
    }
}