using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class Kill : OnTriggerHandler
    {
        public LayerMask mask;

        public override void Handle(ref ParticleSystem.Particle particle, Collider2D[] colliders)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                if (colliders[i].IsLayerInMask(mask))
                {
                    particle.startLifetime = 0;
                    particle.remainingLifetime = 0;
                }
            }
        }
    }
}