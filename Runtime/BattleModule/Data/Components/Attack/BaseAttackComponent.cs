using System;
using LSCore.LevelSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Scripting;
using static Battle.ObjectsByTransfroms<Battle.Data.Components.BaseAttackComponent>;

namespace Battle.Data.Components
{
    [Preserve, Serializable]
    internal abstract class BaseAttackComponent
    {
        protected float attackSpeed;
        protected float damage;
        protected float radius;
        protected Transform transform;
        protected float Damage => damage * Buffs;
        public Buffs Buffs { get; private set; }

        protected Tween attackTween;

        public void Init(Transform transform)
        {
            Add(transform, this);
            this.transform = transform;
            var unit = transform.Get<BaseUnit>();
            radius = unit.GetValue<RadiusGP>();
            damage = unit.GetValue<DamageGP>();
            attackSpeed = unit.GetValue<AttackSpeedGP>();
            Buffs = new Buffs();
            OnInit();
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
            if (target != null && target.TryGet<BaseHealthComponent>(out var health))
            {
                health.TakeDamage(Damage);
                return true;
            }

            return false;
        }
    }
}