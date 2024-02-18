using System;
using LSCore.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [TypeFrom]
    [HideReferenceObjectPicker]
    public abstract class Impact
    {
        public abstract void Apply(Transform target);
    }
}