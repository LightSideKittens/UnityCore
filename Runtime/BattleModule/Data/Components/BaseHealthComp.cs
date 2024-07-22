using System;
using UnityEngine;
using static LSCore.BattleModule.TransformDict<LSCore.BattleModule.BaseHealthComp>;

namespace LSCore.BattleModule
{
    [Serializable]
    public class BaseHealthComp : BaseComp
    {
        [SerializeField] protected int health;
        protected int realHealth;
        private bool isKilled;
        protected AffiliationType affiliation;
        public int Health => health;

        protected override void OnRegister() => Reg(this);
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
            realHealth = health;
            isKilled = false;
        }

        public void Kill()
        {
            TakeDamage(realHealth);
        }

        public void TakeDamage(int damage)
        {
            if (isKilled) return;
            
            realHealth -= damage;
            OnDamageTaken(damage);
            
            if (realHealth <= 0)
            {
                isKilled = true;
                transform.Get<Unit>().Kill();
                OnKilled();
            }
        }

        protected virtual void OnDamageTaken(float damage) { }
        protected virtual void OnHealTaken(float heal) { }
        protected virtual void OnKilled() { }
        
        public void Heal(int heal)
        {
            if (isKilled) return;
            
            realHealth += heal;
            OnHealTaken(heal);
            
            if (realHealth >= health)
            {
                realHealth = health;
            }
        }
    }
}