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
        public int damage;
        
        public override void Apply(Transform target)
        {
            var health = target.Get<BaseHealthComp>();
            Wait.Cycles(1, cycles, () =>
            {
                health.TakeDamage(damage);
            });
        }
    }
}