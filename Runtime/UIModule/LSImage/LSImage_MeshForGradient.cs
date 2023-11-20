using System.Collections.Generic;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public partial class LSImage
    {
        private void CutMeshForGradient(LSVertexHelper vh)
        {
            var tris = new List<UIVertex>();

            vh.GetUIVertexStream(tris);
            vh.Clear();

            var list = new List<UIVertex>();

            var dirNorm = GradientDirection.normalized;
            var d = new Vector2(-dirNorm.y, dirNorm.x);
            var rect = rt.rect;
            var center = rect.center;
            var size = rect.size;
            var start = gradientStart;
            var end = gradientEnd;
            
            if (rotateId % 2 == 1)
            {
                start = 1 - gradientStart;
            }
            else
            {
                end = 1 - gradientEnd;
            }
            
            var v = d.Rotate(-90);
            newAngle = (angle + 360 - rotateId * 90) % 360;
            
            if (newAngle % 180 < 90)
            {
                size *= 0.5f;
                p1 = size.Scalee(downLeft).Project(v);
                p2 = size.Scalee(upRight).Project(v);
            }
            else
            {
                size *= -0.5f;
                p1 = size.Scalee(upLeft).Project(v);
                p2 = size.Scalee(downRight).Project(v);
            }
            
            foreach (var item in gradient.Positions)
            {
                var pos = Mathf.Lerp(start, end, item);
                list.Clear();
                
                if (pos < 0.001 || pos > 0.999)
                {
                    continue;
                }

                var point = GetCutOrigin(pos);
                for (int j = 0; j < tris.Count; j += 3)
                {
                    CutTriangle(tris, j, list, d, center, point);
                }
                
                tris.Clear();
                tris.AddRange(list);
            }
            
            vh.AddUIVertexTriangleStream(tris);
        }

        private static readonly Vector2 downLeft = Vector2.down + Vector2.left;
        private static readonly Vector2 upRight = Vector2.up + Vector2.right;
        private static readonly Vector2 upLeft = Vector2.up + Vector2.left;
        private static readonly Vector2 downRight = Vector2.down + Vector2.right;
        private Vector2 p1;
        private Vector2 p2;
        private float newAngle;


        private Vector2 GetCutOrigin(float f)
        {
            if (newAngle >= 180)
            {
                return p2.Lerp(p1, f);
            }

            return p1.Lerp(p2, f);
        }

        void CutTriangle(List<UIVertex> tris, int idx, List<UIVertex> list, in Vector2 cutDirection, in Vector2 center, in Vector2 origin)
        {
            var a = tris[idx];
            var b = tris[idx + 1];
            var c = tris[idx + 2];
            
            var aCenter = (Vector2)a.position - center;
            var bCenter = (Vector2)b.position - center;
            var cCenter = (Vector2)c.position - center;
            
            float bc = OnLine(bCenter, cCenter, origin, cutDirection);
            float ab = OnLine(aCenter, bCenter, origin, cutDirection);
            float ca = OnLine(cCenter, aCenter, origin, cutDirection);

            if (IsOnLine(ab))
            {
                if (IsOnLine(bc))
                {
                    var pab = UIVertexLerp(a, b, ab);
                    var pbc = UIVertexLerp(b, c, bc);
                    list.AddRange(new List<UIVertex>() { a, pab, c, pab, pbc, c, pab, b, pbc });
                }
                else
                {
                    var pab = UIVertexLerp(a, b, ab);
                    var pca = UIVertexLerp(c, a, ca);
                    list.AddRange(new List<UIVertex>() { c, pca, b, pca, pab, b, pca, a, pab });
                }
            }
            else if (IsOnLine(bc))
            {
                var pbc = UIVertexLerp(b, c, bc);
                var pca = UIVertexLerp(c, a, ca);
                list.AddRange(new List<UIVertex>() { b, pbc, a, pbc, pca, a, pbc, c, pca });
            }
            else
            {
                list.AddRange(tris.GetRange(idx, 3));
            }
        }

        float OnLine(in Vector2 p1, in Vector2 p2, in Vector2 o, in Vector2 dir)
        {
            float tmp = (p2.x - p1.x) * dir.y - (p2.y - p1.y) * dir.x;
            if (tmp == 0)
            {
                return -1;
            }
            float mu = ((o.x - p1.x) * dir.y - (o.y - p1.y) * dir.x) / tmp;
            return mu;
        }

        private static bool IsOnLine(in float f) => f <= 1 && f > 0;

        private static UIVertex UIVertexLerp(in UIVertex v1, in UIVertex v2, in float f)
        {
            UIVertex vert = new UIVertex();

            vert.position = Vector3.Lerp(v1.position, v2.position, f);
            vert.color = Color32.Lerp(v1.color, v2.color, f);
            vert.uv0 = Vector4.Lerp(v1.uv0, v2.uv0, f);

            return vert;
        }
    }
}