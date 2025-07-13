using LSCore.DataStructs;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace LSCore.Extensions.Unity
{
    public static class ParticleSystemExtensions
    {
        private static Particle[] particles = new Particle[1000];

        public static void SetParticlesSize(int size) => particles = new Particle[size];

        public static ArraySlice<Particle> GetParticles(this ParticleSystem ps)
        {
            var num = ps.GetParticles(particles);
            return particles.Slice(..num);
        }
        
        public static void SetParticles(this ParticleSystem ps, ArraySlice<Particle> particles)
        {
            ps.SetParticles(particles.array, particles.Length);
        }
    }
}