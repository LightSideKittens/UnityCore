using System;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [TypeFrom]
    [HideReferenceObjectPicker]
    public abstract class OnTriggerHandler
    {
        public abstract void Handle(ref ParticleSystem.Particle particle, Collider2D collider);
    }
}