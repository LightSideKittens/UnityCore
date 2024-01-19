using System;
using LSCore.LevelSystem;
using UnityEngine;
using static Battle.ObjectsByTransfroms<Battle.Data.Components.BaseHealthComponent>;

namespace Battle.Data.Components
{
    [Serializable]
    public class BaseHealthComponent
    {
        protected Transform transform;
        private bool isKilled;
        private bool isOpponent;
        protected float health;

        public virtual void Init(Transform transform, bool isOpponent)
        {
            this.transform = transform;
            health = transform.GetValue<HealthGP>();
            this.isOpponent = isOpponent;
            Add(transform, this);
        }
        
        public virtual void Reset()
        {
            isKilled = false;
            health = transform.GetValue<HealthGP>();
        }

        public void Destroy()
        { 
            Remove(transform);
        }

        public virtual void Update() { }

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