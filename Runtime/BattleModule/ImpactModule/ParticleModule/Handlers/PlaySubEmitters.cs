using UnityEngine;

namespace LSCore
{
    public class PlaySubEmitters : ParticleHandler
    {
        [SerializeField] private int index;
        [SerializeField] private ParticleSystem self;
        
        public override void Handle(ref ParticleSystem.Particle particle)
        {
            self.TriggerSubEmitter(index, ref particle);
        }
    }
}