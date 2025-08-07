using System;
using LSCore.Attributes;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class ConditionHandler : OnTriggerHandler
    {
        [SerializeReference] public TriggerIf @if;
        
        [SerializeReference] 
        [ExcludeType(typeof(ConditionHandler))] 
        public OnTriggerHandler handler;
        
        public override void Handle(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            @if.Prepare(ref particle, collider);
            
            if (@if)
            {
                handler.Handle(ref particle, collider);
            }
        }
    }
}