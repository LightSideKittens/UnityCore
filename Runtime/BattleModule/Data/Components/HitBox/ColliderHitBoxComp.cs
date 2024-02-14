using System;
using UnityEngine;
using static LSCore.BattleModule.ObjectTo<LSCore.BattleModule.ColliderHitBoxComp>;

namespace LSCore.BattleModule
{
    [Serializable]
    internal class ColliderHitBoxComp : HitBoxComponent
    {
        private CircleCollider2D collider;
        
        protected override void OnRegister() => Add(transform, this);
        public override void UnRegister() => Remove(transform);
        protected override void Init()
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