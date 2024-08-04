using System;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore
{
    [Serializable]
    public class CircleShape : BaseShape
    {
        public float radius = 1;
        
        public override Collider2D[] Overlap(in ContactFilter2D filter)
        {
            return Physics2DExt.FindAll(Position, radius, filter);
        }

        public override void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(Position, radius);
        }
    }
}