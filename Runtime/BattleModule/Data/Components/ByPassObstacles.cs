using System;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace LSCore.BattleModule
{
    [ExecuteAlways]
    public class ByPassObstacles : MonoBehaviour
    {
        [SerializeField] public Transform player;
        [SerializeField] public Transform rigidbody;
        [SerializeField] public CircleCollider2D collider;
        [SerializeField] public LayerMask obstaclesMask;
        [SerializeField] public ContactFilter2D contactFilter;
        
        private readonly Vector2[] sidePoses = new Vector2[3];
        private readonly Vector2[] dirs = new Vector2[3];
        private readonly RaycastHit2D[] hits = new RaycastHit2D[3];
        private bool drawGizmos = false;
        
        private void Update()
        {
            foreach (var result in Physics2DExt.FindAll(transform.position, 1000, contactFilter))
            {
                UnityEngine.Debug.Log(result.name);
            }
            
            drawGizmos = false;
            ByPass(player.position, out var dir);
            transform.position += (Vector3)dir * Time.deltaTime;
        }

        private void OnDrawGizmos()
        {
            drawGizmos = true;
            ByPass(player.position, out var dir);
        }

        public void ByPass(in Vector2 targetPos, out Vector2 byPassedDirection)
        {
            var pos = (Vector2)rigidbody.position;
            var direction = targetPos - pos;
            var trueRadius = collider.radius * rigidbody.transform.lossyScale.x;
            var distance = direction.magnitude;
            direction = direction.normalized;
            
            var perpendicular = Vector2.Perpendicular(direction) * trueRadius;

            sidePoses[0] = pos;
            sidePoses[1] = pos + perpendicular;
            sidePoses[2] = pos + -perpendicular;
            dirs[0] = direction * distance;
            dirs[1] = targetPos - sidePoses[1];
            dirs[2] = targetPos - sidePoses[2];
            
            for (var i = 0; i < 3; i++)
            {
                hits[i] = Physics2D.Raycast(sidePoses[i], dirs[i], float.MaxValue, obstaclesMask);
            }
            
            for (var i = 0; drawGizmos && i < 3; i++)
            {
                Gizmos.DrawLine(sidePoses[i], sidePoses[i] + dirs[i].magnitude * dirs[i].normalized);
            }

            var hit = hits.Min(x =>
            {
                if (x.collider == null) return float.MaxValue;
                return x.distance;
            }, out var index);
            if (index == -1)
            {
                byPassedDirection = direction;
                return;
            }
            
            var bounds = hit.collider.bounds;
            Vector2 center = bounds.center;
            var toObs = center - pos;
            var obsRadius = bounds.CircumscribedCircleRadius();
            var targetRadius = obsRadius + trueRadius;
            var diff = targetRadius - toObs.magnitude;
            
            if (diff >= 0)
            {
                toObs = toObs.normalized;
                perpendicular = Vector2.Perpendicular(toObs);
                direction = PointLeftOrRight(pos, targetPos, center) < 0 ? perpendicular - toObs : -perpendicular - toObs;
            }
            else
            {
                var tangentPoint = FindTangentPoint(center, targetRadius, pos, PointLeftOrRight(pos, targetPos, center) < 0);
                direction = tangentPoint - pos;

                if (drawGizmos)
                {
                    Gizmos.color = Color.green;
                    Gizmos.DrawWireSphere(tangentPoint, 0.1f);
                    Gizmos.color = Color.white;
                }
            }
            
            if (drawGizmos)
            {
                Gizmos.DrawWireSphere(sidePoses[1], 0.1f);
                Gizmos.DrawWireSphere(sidePoses[2], 0.1f);

                perpendicular = Vector2.Perpendicular(direction.normalized) * trueRadius;
                Gizmos.DrawWireSphere(center, obsRadius);
                Gizmos.DrawWireSphere(center, obsRadius + trueRadius);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, pos + direction.normalized * direction.magnitude);
                Gizmos.DrawLine(pos + perpendicular,
                    (pos + perpendicular) + direction.normalized * direction.magnitude);
                Gizmos.DrawLine(pos - perpendicular,
                    (pos - perpendicular) + direction.normalized * direction.magnitude);
                Gizmos.color = Color.white;
            }

            byPassedDirection = direction;
        }

        private static Vector2 FindTangentPoint(in Vector2 circleCenter, float radius, in Vector2 externalPoint, bool getLeft)
        {
            var centerToPoint = externalPoint - circleCenter;
            var distanceToCenter = centerToPoint.magnitude;
            var angleToExternalPoint = Mathf.Atan2(centerToPoint.y, centerToPoint.x);
            var tangentAngle = Mathf.Acos(radius / distanceToCenter);

            if (getLeft)
            {
                var angleToTangent2 = angleToExternalPoint - tangentAngle;
                var left = circleCenter + new Vector2(Mathf.Cos(angleToTangent2), Mathf.Sin(angleToTangent2)) * radius;
                return left;
            }

            var angleToTangent1 = angleToExternalPoint + tangentAngle;
            var right = circleCenter + new Vector2(Mathf.Cos(angleToTangent1), Mathf.Sin(angleToTangent1)) * radius;
            return right;
        }


        private static float PointLeftOrRight(in Vector2 a, in Vector2 b, in Vector2 c)
        {
            var value = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            return value;
        }
    }
}