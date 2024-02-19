using System;
using LSCore.Attributes;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class ConditionHandler : OnTriggerHandler
    {
        [SerializeReference] public TriggerCondition condition;
        
        [SerializeReference] 
        [ExcludeType(typeof(ConditionHandler))] 
        public OnTriggerHandler handler;
        
        public override void Handle(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            condition.Prepare(ref particle, collider);
            
            if (condition)
            {
                handler.Handle(ref particle, collider);
            }
        }
    }
}