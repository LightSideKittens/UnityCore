using System;
using LSCore.Attributes;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    [Unwrap]
    public struct LayerMaskContactFilter
    {
        public LayerMask layerMask;

        public static implicit operator ContactFilter2D(LayerMaskContactFilter filter)
        {
            return new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = filter.layerMask
            };
        }
    }
}