using System;
using LSCore.LevelSystem;
using UnityEngine;
using static LSCore.BattleModule.ObjectsByTransforms<LSCore.BattleModule.BaseHealthComp>;

namespace LSCore.BattleModule
{
    [Serializable]
    public class BaseHealthComp : BaseComp
    {
        protected Transform transform;
        private bool isKilled;
        protected AffiliationType affiliation;
        protected float health;

        public override void Init(CompData data)
        {
            transform = data.transform;
            data.onInit += OnInit;
            data.reset += Reset;
            data.destroy += Destroy;
            health = transform.GetValue<HealthGP>();
            Add(transform, this);
        }

        private void OnInit()
        {
            affiliation = transform.Get<Unit>().Affiliation;
        }

        protected virtual void Reset()
        {
            isKilled = false;
            health = transform.GetValue<HealthGP>();
        }

        private void Destroy()
        { 
            Remove(transform);
        }

        public void Kill()
        {
            TakeDamage(health);
        }

        public void TakeDamage(float damage)
        {
            if (isKilled) return;
            
            health -= damage;
            OnDamageTaken(damage);
            
            if (health <= 0)
            {
                isKilled = true;
                transform.Get<Unit>().Kill();
                OnKilled();
            }
        }

        protected virtual void OnDamageTaken(float damage) { }
        protected virtual void OnKilled() { }
    }
}