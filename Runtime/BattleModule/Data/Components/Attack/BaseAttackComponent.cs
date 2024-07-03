using System;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    public abstract class BaseAttackComponent : BaseComp
    {
        public ImpactObject impactObjectPrefab;
        [SerializeField] protected FindTargetComp findTargetComp;
        [NonSerialized] public ImpactObject impactObject;
        public float attackSpeed;

        protected override void Init()
        {
            useUpdate = true;
            data.onInit += OnInit;
            data.enable += Enable;
            data.disable += Disable;
            data.update += Update;
            data.destroy += Destroy;
            impactObject = Object.Instantiate(impactObjectPrefab, transform, false);
            impactObject.IgnoredCollider = transform.GetComponent<Collider2D>();
            findTargetComp.Init(transform);
            impactObject.CanImpactChecker = findTargetComp.Check;
        }
        
        protected virtual void OnInit(){}

        public void Enable() => OnEnable();

        protected virtual void OnEnable(){}

        public void Disable()
        {
            OnDisable();
        }
        
        protected virtual void OnDisable(){}

        public void Destroy()
        {
            Disable();
        }
    }
}