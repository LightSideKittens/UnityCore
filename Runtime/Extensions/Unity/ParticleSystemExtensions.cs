using LSCore.DataStructs;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LSCore.Extensions.Unity
{
    public static class ParticleSystemExtensions
    {
        private static Particle[] particles = new Particle[100];

        public static void SetParticlesSize(int size) => particles = new Particle[size];

        public static ArraySpan<Particle> GetParticles(this ParticleSystem ps)
        {
            var num = ps.GetParticles(particles);
            return particles.AsSpan(..num);
        }
        
        public static void SetParticles(this ParticleSystem ps, ArraySpan<Particle> particles)
        {
            ps.SetParticles(particles.array, particles.Length);
        }
    }
}