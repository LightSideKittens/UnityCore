using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;
using static LSCore.BattleModule.ObjectTo<LSCore.BattleModule.BaseAttackComponent>;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    public abstract class BaseAttackComponent : BaseComp
    {
        public ImpactObject impactObject;
        protected float attackSpeed;
        protected float damage;
        protected float radius;
        protected int Damage => (int)(damage * Buffs);
        public Buffs Buffs { get; private set; }

        protected Tween attackTween;
        
        protected override void OnRegister() => Add(transform, this);
        public override void UnRegister() => Remove(transform);
        protected override void Init()
        {
            Buffs = new Buffs();
            
            data.onInit += OnInit;
            data.enable += Enable;
            data.disable += Disable;
            data.update += Update;
            data.reset += Buffs.Reset;
            data.destroy += Destroy;
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

        protected bool TryApplyDamage(Transform target)
        {
            if (target != null && target.TryGet<BaseHealthComp>(out var health))
            {
                health.TakeDamage(Damage);
                return true;
            }

            return false;
        }
    }
}