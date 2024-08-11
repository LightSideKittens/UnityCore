using System;
using DG.Tweening;
using UnityEngine;

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
        [NonSerialized] public bool isImmune;

        protected override void OnRegister() => Reg(this);
        protected override void Init()
        {
            data.onInit += OnInit;
            data.reset += Reset;
            data.enable += OnEnable;
        }

        protected virtual void OnEnable()
        {
            var tween = OnAlive();
            if (tween == null) return;
            isImmune = true;
            OnAlive()?.OnComplete(() => isImmune = false);
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
            if (isImmune) return;
            if (isKilled) return;
            
            realHealth -= damage;
            OnDamageTaken(damage);
            
            if (realHealth <= 0)
            {
                isKilled = true;
                var killTween = OnKilled();
                if (killTween != null)
                {
                    killTween.OnComplete(() => transform.Get<Unit>().Release());
                }
                else
                {
                    transform.Get<Unit>().Release();
                }
            }
        }

        protected virtual void OnDamageTaken(float damage) { }
        protected virtual void OnHealTaken(float heal) { }
        protected virtual Tween OnAlive() => null;
        protected virtual Tween OnKilled() => null;
        
        public void Heal(int heal)
        {
            if (isImmune) return;
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