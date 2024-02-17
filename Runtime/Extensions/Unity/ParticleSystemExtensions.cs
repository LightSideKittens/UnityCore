using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LSCore.Extensions.Unity
{
    public static class ParticleSystemExtensions
    {
        private static Particle[] particles = new Particle[10];

        public static void SetParticlesSize(int size) => particles = new Particle[size];

        public static Particle[] GetParticles(this ParticleSystem ps)
        {
            var num = ps.GetParticles(particles);
            return particles[..num];
        }
    }
}