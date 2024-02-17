using UnityEngine;

namespace LSCore
{
    public class ColorHandler : ParticlesHandler
    {
        [SerializeField] private Color32 firstColor;
        [SerializeField] private Color32 secondColor;
        [SerializeField] private AnimationCurve curve;
        
        public override void Handle(ParticleSystem.Particle[] particles)
        {
            for (int i = 0; i < particles.Length; i++)
            {
                ref var particle = ref particles[i];
                var currentLifetime = particle.startLifetime - particle.remainingLifetime;
                particle.startColor = Color32.Lerp(firstColor, secondColor, curve.Evaluate(currentLifetime));
            }
        }
    }
}