using System;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Kill : OnTriggerHandler
    {
        public override void Handle(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            particle.startLifetime = 0;
            particle.remainingLifetime = 0;
        }
    }
}