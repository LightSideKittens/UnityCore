using System;
using LSCore.BattleModule;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class GroupCondition : TriggerCondition
    {
        [IdGroup] public IdGroup group;
        private Id id;

        public override void Prepare(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            var transform = collider.transform;
            id = transform.Get<Unit>().Id;
        }
        
        protected override bool Check()
        {
            var check = group == null || group.Contains(id);
            return check;
        }
    }
}