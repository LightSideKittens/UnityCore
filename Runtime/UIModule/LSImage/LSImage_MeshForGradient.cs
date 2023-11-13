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
            var center = (Vector3)rectTransform.rect.center;
            
            var cuts = gradient.alphaKeys.Select(x => x.time);
            cuts = cuts.Union(gradient.colorKeys.Select(x => x.time));

            foreach (var item in cuts)
            {
                list.Clear();
                
                if (item < 0.001 || item > 0.999)
                {
                    continue;
                }

                for (int j = 0; j < tris.Count; j += 3)
                {
                    CutTriangle(tris, j, list, d, center);
                }
                
                tris.Clear();
                tris.AddRange(list);
            }
            
            vh.AddUIVertexTriangleStream(tris);
        }

        void CutTriangle(List<UIVertex> tris, int idx, List<UIVertex> list, in Vector2 cutDirection, in Vector3 center)
        {
            var a = tris[idx];
            var b = tris[idx + 1];
            var c = tris[idx + 2];
            
            var aCenter = a.position - center;
            var bCenter = b.position - center;
            var cCenter = c.position - center;
            
            float bc = OnLine(bCenter, cCenter, cutDirection);
            float ab = OnLine(aCenter, bCenter, cutDirection);
            float ca = OnLine(cCenter, aCenter, cutDirection);

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

        float OnLine(in Vector3 p1, in Vector3 p2, in Vector2 dir)
        {
            float tmp = (p2.x - p1.x) * dir.y - (p2.y - p1.y) * dir.x;
            if (tmp == 0)
            {
                return -1;
            }
            float mu = (-p1.x * dir.y + p1.y * dir.x) / tmp;
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