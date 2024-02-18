using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class HandlerImpact : ParticleImpact
    {
        [SerializeReference] public OnTriggerHandler handler;

        public override void Apply(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            base.Apply(ref particle, collider);
            handler.Handle(ref particle, collider);
        }
    }
}