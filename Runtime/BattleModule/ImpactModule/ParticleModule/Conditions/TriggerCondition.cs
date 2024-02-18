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
    public abstract class TriggerCondition : Condition
    {
        public abstract void Init(ref ParticleSystem.Particle particle, Collider2D collider);
    }
}