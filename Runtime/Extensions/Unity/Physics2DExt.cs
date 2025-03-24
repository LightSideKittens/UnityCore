﻿using System.Collections.Generic;
using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Physics2DExt
    {
        private static Collider2D[] hitColliders = new Collider2D[100];

        public static void SetHitCollidersSize(int size) => hitColliders = new Collider2D[size];
        
        public static Collider2D[] FindAll(in Vector2 position)
        {
            var numColliders = Physics2D.OverlapPoint(position, default, hitColliders);

            return hitColliders[..numColliders];
        }
        
        public static Collider2D[] FindAll(in Vector2 position, in ContactFilter2D filter)
        {
            var numColliders = Physics2D.OverlapPoint(position, filter, hitColliders);

            return hitColliders[..numColliders];
        }
        
        public static Collider2D[] FindAll(in Vector2 position, float radius, in ContactFilter2D filter)
        {
            var numColliders = Physics2D.OverlapCircle(position, radius, filter, hitColliders);

            return hitColliders[..numColliders];
        }
        
        public static Collider2D[] FindAll(in Vector2 position, Vector2 boxSize, float angle, in ContactFilter2D filter)
        {
            var numColliders = Physics2D.OverlapBox(position, boxSize, angle, filter, hitColliders);

            return hitColliders[..numColliders];
        }
        
        public static Collider2D[] FindAll(in Rect rect, float angle, in ContactFilter2D filter)
        {
            var numColliders = Physics2D.OverlapBox(rect.center, rect.size, angle, filter, hitColliders);

            return hitColliders[..numColliders];
        }
        
        public static bool TryFindNearestCollider(in Vector2 position, in ContactFilter2D filter, out Collider2D closestCollider, float startRadius = 1, float maxRadius = 100)
        {
            int numColliders;
            while (true)
            {
                numColliders = Physics2D.OverlapCircle(position, startRadius, filter, hitColliders);

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

            return TryFindNearestCollider(position, hitColliders[..numColliders], out closestCollider);
        }
        
        public static bool TryFindNearestCollider(Collider2D sourceCollider, out Collider2D closestCollider, in ContactFilter2D filter, float startRadius = 1, float maxRadius = 100)
        {
            int numColliders;
            Vector2 position = sourceCollider.bounds.center;
            sourceCollider.enabled = false;
            while (true)
            {
                numColliders = Physics2D.OverlapCircle(position, startRadius, filter, hitColliders);

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
            return TryFindNearestCollider(sourceCollider, hitColliders[..numColliders], out closestCollider);
        }

        public static bool TryFindNearestCollider(Collider2D sourceCollider, IEnumerable<Collider2D> colliders, out Collider2D closestCollider)
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
        
        public static bool TryFindNearestCollider(in Vector2 position, IEnumerable<Collider2D> colliders, out Collider2D closestCollider)
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