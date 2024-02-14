using System;
using UnityEngine;
using static LSCore.BattleModule.ObjectTo<LSCore.BattleModule.BaseHealthComp>;

namespace LSCore.BattleModule
{
    [Serializable]
    public class BaseHealthComp : BaseComp
    {
        [SerializeField] protected int health;
        private bool isKilled;
        protected AffiliationType affiliation;
        public int Health => health;

        protected override void OnRegister() => Add(transform, this);
        public override void UnRegister() => Remove(transform);
        protected override void Init()
        {
            data.onInit += OnInit;
            data.reset += Reset;
        }

        protected virtual void OnInit()
        {
            affiliation = transform.Get<Unit>().Affiliation;
        }

        protected virtual void Reset()
        {
            isKilled = false;
        }

        public void Kill()
        {
            TakeDamage(health);
        }

        public void TakeDamage(int damage)
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