using System;
using static UnityEngine.ParticleSystem;

namespace LSCore
{
    [Serializable]
    public abstract class ParticlesHandler
    {
        public abstract void Handle(Particle[] particles);
    }
        
    [Serializable]
    public abstract class ParticleHandler
    {
        public abstract void Handle(ref Particle particle);
    }
}