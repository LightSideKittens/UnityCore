using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Physics2DExt
    {
        private static Collider2D[] hitColliders = new Collider2D[10];

        public static void SetHitCollidersSize(int size) => hitColliders = new Collider2D[size];

        public static Collider2D[] FindAll(in Vector2 position, float radius, LayerMask mask)
        {
            var numColliders = Physics2D.OverlapCircleNonAlloc(position, radius, hitColliders, mask);

            return hitColliders[..numColliders];
        }
        
        public static bool TryFindNearestCollider(in Vector2 position, LayerMask mask, out Collider2D closestCollider, float startRadius = 1, float maxRadius = 100)
        {
            int numColliders;
            while (true)
            {
                numColliders = Physics2D.OverlapCircleNonAlloc(position, startRadius, hitColliders, mask);

                if (numColliders > 0)
                {
                    if (numColliders == 1)
                    {
                        closestCollider = hitColliders[0];
                        return true;
                    }
                    break;
                }
                
                startRadius *= 2;

                if (maxRadius < startRadius)
                {
                    closestCollider = null;
                    return false;
                }
            }

            return TryFindNearestCollider(position, hitColliders[..numColliders], out closestCollider, mask);
        }
        
        public static bool TryFindNearestCollider(Collider2D sourceCollider, out Collider2D closestCollider, LayerMask mask, float startRadius = 1, float maxRadius = 100)
        {
            int numColliders;
            Vector2 position = sourceCollider.bounds.center;
            sourceCollider.enabled = false;
            while (true)
            {
                numColliders = Physics2D.OverlapCircleNonAlloc(position, startRadius, hitColliders, mask);

                if (numColliders > 0)
                {
                    if (numColliders == 1)
                    {
                        closestCollider = hitColliders[0];
                        return true;
                    }
                    break;
                }
                
                startRadius *= 2;

                if (maxRadius < startRadius)
                {
                    closestCollider = null;
                    return false;
                }
            }
            sourceCollider.enabled = true;
            return TryFindNearestCollider(sourceCollider, hitColliders[..numColliders], out closestCollider, mask);
        }

        public static bool TryFindNearestCollider(Collider2D sourceCollider, IEnumerable<Collider2D> colliders, out Collider2D closestCollider, LayerMask mask)
        {
            float closestDistance = float.MaxValue;
            closestCollider = null;

            foreach (var targetCollider in colliders)
            {
                var distance = Physics2D.Distance(sourceCollider, targetCollider).distance;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = targetCollider;
                }
            }

            return closestCollider != null;
        }
        
        public static bool TryFindNearestCollider(in Vector2 position, IEnumerable<Collider2D> colliders, out Collider2D closestCollider, LayerMask mask)
        {
            float closestDistance = float.MaxValue;
            closestCollider = null;

            foreach (var targetCollider in colliders)
            {
                Vector2 closestPoint = Physics2D.ClosestPoint(position, targetCollider);
                float distance = (closestPoint - position).sqrMagnitude;

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCollider = targetCollider;
                }
            }

            return closestCollider != null;
        }
    }
}