using System;
using LSCore.LevelSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;
using static LSCore.BattleModule.ObjectsByTransforms<LSCore.BattleModule.BaseAttackComponent>;

namespace LSCore.BattleModule
{
    [Preserve, Serializable]
    internal abstract class BaseAttackComponent : BaseComp
    {
        protected float attackSpeed;
        protected float damage;
        protected float radius;
        protected Transform transform;
        protected float Damage => damage * Buffs;
        public Buffs Buffs { get; private set; }

        protected Tween attackTween;

        public override void Init(CompData data)
        {
            Add(transform, this);
            transform = data.transform;
            Buffs = new Buffs();
            var unit = transform.Get<BaseUnit>();
            radius = unit.GetValue<RadiusGP>();
            damage = unit.GetValue<DamageGP>();
            attackSpeed = unit.GetValue<AttackSpeedGP>();
            
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
            Remove(transform);
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