using System;
using LSCore.Attributes;
using LSCore.ConditionModule;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [TypeFrom]
    [HideReferenceObjectPicker]
    public abstract class TriggerIf : If
    {
        public abstract void Prepare(ref ParticleSystem.Particle particle, Collider2D collider);
    }
}