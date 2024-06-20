using System;
using LSCore.Async;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Poisoning : Impact
    {
        public int cycles = 3;
        public float delay = 1;
        public int damage;
        
        public override void Apply(Transform target)
        {
            var health = target.Get<BaseHealthComp>();
            Wait.Cycles(delay, cycles, () =>
            {
                health.TakeDamage(damage);
            });
        }
    }
}