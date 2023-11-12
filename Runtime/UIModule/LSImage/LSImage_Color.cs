using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Mathf;

namespace LSCore
{
    public partial class LSImage
    {
        internal Vector2 minPoint;
        internal Vector2 maxPoint;
        internal bool isColorDirty;
        
        private Color DefaulColor(in Vector3 pos) => color;
        private float GetGradientValue(Vector2 pos)
        {
            pos -= currentRect.center;
            return pos.UnclampedInverseLerp(gradientStartPoint, gradientEndPoint);
        }

        private Color ColorEvaluate(in Vector3 pos) => gradient.Evaluate(GetGradientValue(pos));
        private Color Inverted_ColorEvaluate(in Vector3 pos) => gradient.Evaluate(1 - GetGradientValue(pos));
        private InFunc<Vector3, Color> GetLeftToRightColorEvaluate() => invert ? Inverted_ColorEvaluate : ColorEvaluate;
        
        
        private void CalculatePerpendicularPoints()
        {
            var rect = rectTransform.rect;
            float angleRad = (angle + rotateId * 90) * Deg2Rad;
            var direction = new Vector2(Cos(angleRad), Sin(angleRad));
            var radius = rect.CircumscribedCircleRadius();
            float distanceForMinPoint = radius;
            float distanceForMaxPoint = radius;
            
            Vector2 minPointRef = direction * -radius;
            Vector2 maxPointRef = direction * radius;
            
            minPoint = minPointRef;
            maxPoint = maxPointRef;
            var corners = rect.CornersRelativeCenter();

            for (int i = 0; i < corners.Length; i++)
            {
                CalculatePerpendicular(corners[i], minPointRef, ref minPoint, ref distanceForMinPoint);
            }
            
            for (int i = 0; i < corners.Length; i++)
            {
                CalculatePerpendicular(corners[i], maxPointRef, ref maxPoint, ref distanceForMaxPoint);
            }

            direction = maxPoint - minPoint;
            GradientDirection = direction;
            gradientStartPoint.x = minPoint.x + direction.x * gradientStart;
            gradientStartPoint.y = minPoint.y + direction.y * gradientStart;
            gradientEndPoint.x = maxPoint.x - direction.x * gradientEnd;
            gradientEndPoint.y = maxPoint.y - direction.y * gradientEnd;

            void CalculatePerpendicular(in Vector2 rectCorner, in Vector2 refPoint, ref Vector2 point, ref float maxDistance)
            {
                var perpendicular = Vector3.Project(rectCorner, direction);
                var distance = Vector3.Distance(perpendicular, refPoint);
                if (distance < maxDistance)
                {
                    point = perpendicular;
                    maxDistance = distance;
                }
            }
        }

        internal void SetColorDirty()
        {
            isColorDirty = true;
            SetVerticesDirty();
        }
        
        private void UpdateColorEvaluateFunc()
        {
            if (gradient.colorKeys.Length > 1)
            {
                colorEvaluate = GetLeftToRightColorEvaluate();
            }
            else
            {
                colorEvaluate = DefaulColor;
            }
        }
        
        private void UpdateMeshColors(LSVertexHelper vh)
        {
            UIVertex vert = new UIVertex();

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                vert.color = colorEvaluate(vert.position);
                vh.SetUIVertex(vert, i);
            }
        }
    }
}