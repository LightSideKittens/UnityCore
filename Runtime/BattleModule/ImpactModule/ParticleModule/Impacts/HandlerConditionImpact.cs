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
        [SerializeReference] public TriggerIf @if;

        public sealed override void Apply(Transform initiator, ref ParticleSystem.Particle particle, Collider2D collider)
        {
            @if.Prepare(ref particle, collider);
            
            if (@if)
            {
                base.Apply(initiator, ref particle, collider);
                OnApply(ref particle, collider);
            }
        }
        
        protected virtual void OnApply(ref ParticleSystem.Particle particle, Collider2D collider){}
    }
}