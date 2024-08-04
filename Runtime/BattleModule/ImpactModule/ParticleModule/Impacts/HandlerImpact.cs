using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class HandlerImpact : ParticleImpact
    {
        [SerializeReference] public OnTriggerHandler handler;

        public override void Apply(Transform initiator, ref ParticleSystem.Particle particle, Collider2D collider)
        {
            base.Apply(initiator, ref particle, collider);
            handler.Handle(ref particle, collider);
        }
    }
}