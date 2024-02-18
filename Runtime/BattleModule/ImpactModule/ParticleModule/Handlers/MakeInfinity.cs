using System;
using UnityEngine;

namespace LSCore
{
    public class MakeInfinity : ParticlesIniter
    {
        private const int Infinity = 65535;

        public override void Init(ref ParticleSystem.Particle particle)
        {
            if (Math.Abs(particle.startLifetime - Infinity) > 0.0001f)
            {
                particle.startLifetime = Infinity;
                particle.remainingLifetime = Infinity;
            }
        }
    }
}