using System;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public class ParticleImpact
    {
        [SerializeReference] public Impact impact;

        public virtual void Apply(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            impact.Apply(collider.transform);
        }
    }
}