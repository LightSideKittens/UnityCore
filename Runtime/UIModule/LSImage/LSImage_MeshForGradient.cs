using System.Collections.Generic;
using System.Linq;
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
            var rect = rectTransform.rect;
            var center = rect.center;
            var size = rect.size;
            
            var cuts = gradient.alphaKeys.Select(x => x.time);
            cuts = cuts.Union(gradient.colorKeys.Select(x => x.time));

            foreach (var item in cuts)
            {
                var pos = Mathf.Lerp(gradientStart, 1 - gradientEnd, item);
                
                list.Clear();
                
                if (pos < 0.001 || pos > 0.999)
                {
                    continue;
                }

                var point = GetCutOrigin(pos, d, size);
                for (int j = 0; j < tris.Count; j += 3)
                {
                    CutTriangle(tris, j, list, d, center, point);
                }
                
                tris.Clear();
                tris.AddRange(list);
            }
            
            vh.AddUIVertexTriangleStream(tris);
        }
        
        Vector2 GetCutOrigin(float f, Vector2 v, Vector2 size)
        {
            Vector2 p1, p2;

            v = v.Rotate(-90);
            angle = (angle + 360) % 360;
            
            if (angle % 180 < 90)
            {
                p1 = (Vector2.Scale(size * 0.5f, Vector2.down + Vector2.left)).Project(v);
                p2 = (Vector2.Scale(size * 0.5f,Vector2.up + Vector2.right)).Project(v);
            }
            else
            {
                p1 = (Vector2.Scale(size * -0.5f,Vector2.up + Vector2.left)).Project(v);
                p2 = (Vector2.Scale(size * -0.5f,Vector2.down + Vector2.right)).Project(v);
            }
            
            if (angle >= 180)
            {
                return Vector2.Lerp(p2, p1, f);
            }

            return Vector2.Lerp(p1, p2, f);
        }

        void CutTriangle(List<UIVertex> tris, int idx, List<UIVertex> list, Vector2 cutDirection, Vector2 center, Vector2 origin)
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