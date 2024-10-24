using System;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Knockback : Impact
    {
        public float force = 10;
        
        public override void Apply(Transform initiator, Transform target)
        {
            var rb = target.Get<BaseMoveComp>().rigidbody;
            var direction = target.position - initiator.position;
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }
    }
}