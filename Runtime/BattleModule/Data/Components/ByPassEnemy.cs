using LSCore.Extensions;
using LSCore.Extensions.Unity;
using UnityEngine;

namespace Battle.Data.Components
{
    public class ByPassEnemy : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D player;
        [SerializeField] private Rigidbody2D rigidbody;
        [SerializeField] private CircleCollider2D collider;
        [SerializeField] private LayerMask mask;

        private void OnDrawGizmos()
        {
            var pos = rigidbody.position;
            var targetPos = player.position;
            var direction = targetPos - pos;
            
            var trueRadius = collider.radius * rigidbody.transform.lossyScale.x;
            var distance = direction.magnitude;
            direction = direction.normalized;
            
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
                Physics2D.Raycast(sidePoss[0], dirs[0], float.MaxValue, mask),
                Physics2D.Raycast(sidePoss[1], dirs[1], float.MaxValue, mask),
                Physics2D.Raycast(sidePoss[2], dirs[2], float.MaxValue, mask),
            };
            
            
            
            for (int i = 0; i < 3; i++)
            {
                Gizmos.DrawLine(sidePoss[i], sidePoss[i] + dirs[i].magnitude * dirs[i].normalized);
            }
            
            
            var hit = hits.Min(x =>
            {
                if (x.collider == null) return float.MaxValue;
                return x.distance;
            }, out var index);
            
            if (index == -1)
            {
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
                Gizmos.DrawWireSphere(tangentPoint, 0.1f);
            }
            
            Gizmos.DrawWireSphere(sidePoss[1], 0.1f);
            Gizmos.DrawWireSphere(sidePoss[2], 0.1f);


            perpendicular = Vector2.Perpendicular(direction.normalized) * trueRadius;
            Gizmos.DrawWireSphere(center, obsRadius);
            Gizmos.DrawWireSphere(center, obsRadius + trueRadius);
            Gizmos.DrawLine(pos, pos + direction.normalized * direction.magnitude);
            Gizmos.DrawLine(pos + perpendicular, (pos + perpendicular) + direction.normalized * direction.magnitude);
            Gizmos.DrawLine(pos - perpendicular, (pos - perpendicular) + direction.normalized * direction.magnitude);
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
            else
            {        
                float angleToTangent1 = angleToExternalPoint + tangentAngle;
                Vector2 right = circleCenter + new Vector2(Mathf.Cos(angleToTangent1), Mathf.Sin(angleToTangent1)) * radius;
                return right;
            }
        }


        private static float PointLeftOrRight(Vector2 a, Vector2 b, Vector2 c)
        {
            var value = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
            return value;
        }
    }
}