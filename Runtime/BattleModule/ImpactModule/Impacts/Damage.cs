using System;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Damage : Impact
    {
        public int damage;
        
        public override void Apply(Transform initiator, Transform target)
        {
            target.Get<BaseHealthComp>().TakeDamage(damage);
        }
    }
}