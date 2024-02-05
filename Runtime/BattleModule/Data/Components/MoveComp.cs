﻿using System;
using LSCore.Async;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using LSCore.GameProperty;
using UnityEngine;
using static LSCore.BattleModule.ObjectsByTransforms<LSCore.BattleModule.MoveComp>;

namespace LSCore.BattleModule
{
    [Serializable]
    public class MoveComp : BaseComp
    {
        private FindTargetComp findTargetComp;
        private Transform transform;
        protected Rigidbody2D rigidbody;
        private CircleCollider2D collider;
        protected float speed;
        public float Speed => speed * Buffs;
        private static int mask = -1;
        private bool enabled = true;
        private Collider2D lastObstacles;
        public Buffs Buffs { get; private set; }

        public override void Init(CompData data)
        {
            Add(transform, this);
            transform = data.transform;

            rigidbody = transform.GetComponent<Rigidbody2D>();
            collider = rigidbody.GetComponent<CircleCollider2D>();
            speed = transform.GetValue<MoveSpeedGP>();
            Buffs = new Buffs();

            if (mask == -1)
            {
                mask = LayerMask.GetMask("Obstacles");
            }

            Wait.InfinityLoop(0.5f, () => lastObstacles = null);
            lastObstacles = null;
            
            data.onInit += OnInit;
            data.update += Update;
            data.reset += Buffs.Reset;
            data.destroy += Destroy;
        }

        private void OnInit()
        {
            findTargetComp = transform.Get<FindTargetComp>();
        }

        public void SetEnabled(bool enabled)
        {
            this.enabled = enabled;
        }
        
        private void TryByPass(Vector2 targetPos, ref Vector2 direction)
        {
            var pos = rigidbody.position;
            var trueRadius = collider.radius * rigidbody.transform.lossyScale.x;

            var distance = direction.magnitude;
            direction = direction.normalized;
            
            if (lastObstacles != null)
            {
                goto compute;
            }
            
            Vector2 perpendicular = Vector2.Perpendicular(direction) * trueRadius;
            var sideDirs = new[] { perpendicular, -perpendicular};
            
            var sidePoss = new Vector2[]
            {
                pos,
                pos + sideDirs[0],
                pos + sideDirs[1],
            };

            var dirs = new[]
            {
                direction * distance,
                targetPos - sidePoss[1],
                targetPos - sidePoss[2]
            };
            
            var hits = new[]
            {
                Physics2D.Raycast(sidePoss[0], dirs[0], dirs[0].magnitude, mask),
                Physics2D.Raycast(sidePoss[1], dirs[1], dirs[1].magnitude, mask),
                Physics2D.Raycast(sidePoss[2], dirs[2], dirs[2].magnitude, mask),
            };
            
            lastObstacles = hits.Min(x =>
            {
                if (x.collider == null) return float.MaxValue;
                return x.distance;
            }, out var index).collider;
            
            if (index == -1)
            {
                return;
            }
            
            compute:
            var bounds = lastObstacles.bounds;
            Vector2 center = bounds.center;
            var toObs = center - pos;
            var obsRadius = bounds.CircumscribedCircleRadius();
            var targetRadius = obsRadius + trueRadius;
            var diff = targetRadius - toObs.magnitude;
            var onLeft = PointLeftOrRight(pos, targetPos, center) < 0;
            
            if (diff >= 0)
            {
                toObs = toObs.normalized;
                perpendicular = Vector2.Perpendicular(toObs);
                direction = onLeft ? perpendicular - toObs : -perpendicular - toObs;
            }
            else
            {
                var tangentPoint = FindTangentPoint(center, targetRadius, pos, onLeft);
                direction = tangentPoint - pos;
            }
        }
        
        private static Vector2 FindTangentPoint(Vector2 circleCenter, float radius, Vector2 externalPoint, bool getLeft)
        {
            Vector2 centerToPoint = externalPoint - circleCenter;
            float distanceToCenter = centerToPoint.magnitude;
            float angleToExternalPoint = Mathf.Atan2(centerToPoint.y, centerToPoint.x);
            float tangentAngle = Mathf.Acos(radius / distanceToCenter);

            if (getLeft)
            {
                float angleToTangent2 = angleToExternalPoint - tangentAngle;
                Vector2 left = circleCenter + new Vector2(Mathf.Cos(angleToTangent2), Mathf.Sin(angleToTangent2)) * radius;
                return left;
            }

            float angleToTangent1 = angleToExternalPoint + tangentAngle;
            Vector2 right = circleCenter + new Vector2(Mathf.Cos(angleToTangent1), Mathf.Sin(angleToTangent1)) * radius;
            return right;
        }
        
        private static float PointLeftOrRight(Vector2 a, Vector2 b, Vector2 c)
        {
            var value = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            return value;
        }
        
        public virtual void Move()
        {
            if (enabled)
            {
                Buffs.Update();
                if (findTargetComp.Find(out var target))
                {
                    var position = rigidbody.position;
                    var targetPos = (Vector2)target.position;
                    var direction = targetPos - position;
                    
                    TryByPass(targetPos, ref direction);
                    
                    direction = direction.normalized;
                    position += direction * (Speed * Time.deltaTime);
                    transform.up = targetPos - position;
                    rigidbody.position = position;
                }
            }
        }
        
        private void Update()
        {
            Move();
        }

        private void Destroy()
        {
            Remove(transform);
        }
    }
}