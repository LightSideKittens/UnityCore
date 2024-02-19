using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;
using Object = UnityEngine.Object;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    public abstract class BaseAttackComponent : BaseComp
    {
        public ImpactObject impactObjectPrefab;
        [NonSerialized] public ImpactObject impactObject;
        public float attackSpeed;
        public float radius;
        
        protected float damage;
        protected int Damage => (int)(damage * Buffs);
        public Buffs Buffs { get; private set; }

        protected Tween attackTween;

        protected override void OnRegister() => Reg(this);

        protected override void Init()
        {
            Buffs = new Buffs();
            
            data.onInit += OnInit;
            data.enable += Enable;
            data.disable += Disable;
            data.update += Update;
            data.reset += Buffs.Reset;
            data.destroy += Destroy;
            impactObject = Object.Instantiate(impactObjectPrefab, transform, false);
            impactObject.IgnoredCollider = transform.GetComponent<Collider2D>();
        }
        
        protected virtual void OnInit(){}

        public void Enable() => OnEnable();

        protected virtual void OnEnable(){}

        public void Disable()
        {
            OnDisable();
            attackTween.Kill();
        }
        
        protected virtual void OnDisable(){}

        public virtual void Update()
        {
            Buffs.Update();
        }

        public void Destroy()
        {
            Disable();
        }
    }
}