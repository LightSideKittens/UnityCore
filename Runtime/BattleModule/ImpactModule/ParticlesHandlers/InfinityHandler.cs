using System;
using UnityEngine;

namespace LSCore
{
    public class InfinityHandler : ParticlesHandler
    {
        private const int Infinity = 65535;
        
        public override void Handle(ParticleSystem.Particle[] particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                ref var particle = ref particles[i];
                if (Math.Abs(particle.startLifetime - Infinity) > 0.0001f)
                {
                    particle.startLifetime = Infinity;
                    particle.remainingLifetime = Infinity;
                }
            }
        }
    }
}