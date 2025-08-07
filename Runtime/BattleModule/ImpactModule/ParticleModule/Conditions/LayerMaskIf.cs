﻿using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class LayerMaskIf : TriggerIf
    {
        public LayerMask mask;
        private Collider2D collider;
        
        protected override bool Check() => collider.IsLayerInMask(mask);

        public override void Prepare(ref ParticleSystem.Particle particle, Collider2D collider)
        {
            this.collider = collider;
        }
    }
}