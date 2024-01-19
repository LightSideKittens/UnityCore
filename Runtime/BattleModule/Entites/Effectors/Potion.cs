using System;
using Battle.Data.Components;
using LSCore.LevelSystem;
using LSCore.Async;
using UnityEngine;

namespace Battle.Data
{
    [Serializable]
    internal class Potion : BaseEffector
    {
        private float duration;
        private float damage;

        protected override void OnInit()
        {
            duration = GetValue<HealthGP>();
            damage = GetValue<DamageGP>();
        }
        
        protected override void OnApply()
        {
            radiusRenderer.color = new Color(1f, 0.07f, 0.13f, 0.39f);
            Wait.Delay(duration, () =>
            {
                radiusRenderer.gameObject.SetActive(false);
                isApplied = false;
            });
        }

        private void OnTicked()
        {
            foreach (var target in findTargetComponent.FindAll(radius))
            {
                target.Get<BaseHealthComponent>().TakeDamage(damage);
            }
        }
    }
}