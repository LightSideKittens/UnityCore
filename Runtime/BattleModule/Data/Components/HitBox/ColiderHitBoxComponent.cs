using System;
using UnityEngine;

namespace Battle.Data.Components.HitBox
{
    [Serializable]
    internal class ColiderHitBoxComponent : HitBoxComponent
    {
        private CircleCollider2D collider;

        protected override void OnInit()
        {
            collider = transform.GetComponent<CircleCollider2D>();
        }

        public override bool IsIntersected(in Vector2 position, in float radius, out Vector2 point)
        {
            var colliderRadius = collider.radius;
            Vector2 center = collider.bounds.center;
            Vector2 fullDistance = center - position;
            Vector2 direction = fullDistance.normalized;
            point = center - direction * colliderRadius;
            
            return (direction * radius).magnitude  + (direction * colliderRadius).magnitude > fullDistance.magnitude;
        }
    }
}