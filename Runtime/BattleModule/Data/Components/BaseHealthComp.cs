using System;
using DG.Tweening;
using LSCore.BattleModule.States;
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
        public event Action Killed;

        [SerializeReference] private BaseUnitAnim killAnim;
        [SerializeField] private State killState;
        [SerializeReference] private BaseUnitAnim aliveAnim;
        [SerializeField] private State aliveState;
        protected UnitStates unitStates; 

        protected override void OnRegister() => Reg(this);
        protected override void Init()
        {
            data.onInit += OnInit;
            data.reset += Reset;
            data.enable += OnEnable;
            unitStates = transform.Get<UnitStates>();
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
            Killed = null;
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
                Killed?.Invoke();
                
                var unit = transform.Get<Unit>();
                
                if (killTween != null)
                {
                    killTween.OnComplete(unit.Release);
                }
                else
                {
                    unit.Release();
                }
            }
        }

        protected virtual void OnDamageTaken(float damage) { }
        protected virtual void OnHealTaken(float heal) { }
        protected virtual Tween OnAlive()
        {
            unitStates.RemoveState(killState);
            var tween = aliveAnim?.Animate();
            if (tween != null)
            {
                unitStates.TrySetState(aliveState);
                tween.onComplete += () => { unitStates.RemoveState(aliveState); };
            }
            killAnim?.Stop();
            return tween;
        }

        protected virtual Tween OnKilled()
        {
            unitStates.RemoveState(aliveState);
            var tween = killAnim?.Animate();
            if (tween != null)
            {
                unitStates.TrySetState(killState);
            }
            aliveAnim?.Stop();
            return tween;
        }

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