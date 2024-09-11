using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class CircleShape : BaseShape
    {
        public float radius = 1;
        private float Radius => radius * ((Vector2)pivot.lossyScale).magnitude / LSVector2.oneMagnitude;
        
        public override Collider2D[] Overlap(in ContactFilter2D filter)
        {
            return Physics2DExt.FindAll(Position, Radius, filter);
        }

        public override void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(Position, Radius);
        }
    }
}