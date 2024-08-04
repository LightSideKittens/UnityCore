using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class HandlerConditionImpact : ConditionImpact
    {
        [SerializeReference] public OnTriggerHandler handler;

        protected override void OnApply(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            handler.Handle(ref particle, collider);
        }
    }
    
    [Serializable]
    public class ConditionImpact : ParticleImpact
    {
        [SerializeReference] public TriggerCondition condition;

        public sealed override void Apply(Transform initiator, ref ParticleSystem.Particle particle, Collider2D collider)
        {
            condition.Prepare(ref particle, collider);
            
            if (condition)
            {
                base.Apply(initiator, ref particle, collider);
                OnApply(ref particle, collider);
            }
        }
        
        protected virtual void OnApply(ref ParticleSystem.Particle particle, Collider2D collider){}
    }
}