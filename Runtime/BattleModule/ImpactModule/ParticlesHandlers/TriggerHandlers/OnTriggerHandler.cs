using System;
using System.Collections.Generic;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public abstract class OnTriggerHandler
    {
        public abstract void Handle(ref ParticleSystem.Particle particle, Collider2D[] collider);
    }
}