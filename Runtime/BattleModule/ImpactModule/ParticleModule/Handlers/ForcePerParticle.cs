using System;
using UnityEngine;

namespace LSCore
{
    public class ForcePerParticle : ParticlesHandler
    {
        [SerializeField] private Vector3 force;
        
        public override void Handle(ParticleSystem.Particle[] particles)
        {
            var f = force * Time.deltaTime;
            
            for (int i = 0; i < particles.Length; i++)
            {
                ref var particle = ref particles[i];
                var v = particle.velocity;
                var f2 = Quaternion.FromToRotation(Vector3.up, v.normalized) * f;
                v.x += f2.x;
                v.y += f2.y;
                v.z += f2.z;
                particle.velocity = v;
            }
        }
    }
}