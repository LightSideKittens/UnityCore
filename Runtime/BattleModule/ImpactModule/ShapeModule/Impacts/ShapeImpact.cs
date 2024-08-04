using System;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [HideReferenceObjectPicker]
    [TypeFrom]
    public class ShapeImpact
    {
        [SerializeReference] public Impact impact;

        public virtual void Apply(Transform initiator, Collider2D collider)
        {
            impact.Apply(initiator, collider.transform);
        }
    }
}