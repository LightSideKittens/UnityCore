using System.Collections.Generic;
using System.Linq;
using LSCore.Extensions.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public partial class LSImage
    {
        private void CutMesh(LSVertexHelper vh)
        {
            var tris = new List<UIVertex>();

            vh.GetUIVertexStream(tris);

            vh.Clear();

            var list = new List<UIVertex>();

            var d = GetCutDirection();

            var cuts = gradient.alphaKeys.Select(x => x.time);
            cuts = cuts.Union(gradient.colorKeys.Select(x => x.time));

            foreach (var item in cuts)
            {
                list.Clear();
                var point = GetCutOrigin(item);
                if (item < 0.001 || item > 0.999)
                {
                    continue;
                }
                else
                {
                    for (int j = 0; j < tris.Count; j += 3)
                    {
                        CutTriangle(tris, j, list, d, point);
                    }
                }
                tris.Clear();
                tris.AddRange(list);
            }
            vh.AddUIVertexTriangleStream(tris);
        }

        Vector2 GetCutDirection()
        {
            var v = Vector2.up.Rotate(-angle);
            v = new Vector2(v.x / this.currentRect.size.x,v.y / this.currentRect.size.y);
            return v.Rotate(90);
        }

        Vector2 GetCutOrigin(in float f)
        {
            var v = Vector2.up.Rotate(-angle);

            v = new Vector2(v.x / currentRect.size.x,v.y / currentRect.size.y);

            Vector2 p1, p2;

            if (angle % 180 < 90)
            {
                p1 = (Vector2.Scale(currentRect.size, Vector2.down + Vector2.left) * 0.5f).Project(v);
                p2 = (Vector2.Scale(currentRect.size,Vector2.up + Vector2.right) * 0.5f).Project(v);
            }
            else
            {
                p1 = (Vector2.Scale(currentRect.size,Vector2.up + Vector2.left) * 0.5f).Project(v);
                p2 = (Vector2.Scale(currentRect.size,Vector2.down + Vector2.right) * 0.5f).Project(v);
            }
            if (angle % 360 >= 180)
            {
                return Vector2.Lerp(p2, p1, f) + currentRect.center;
            }

            return Vector2.Lerp(p1, p2, f) + currentRect.center;
        }

        void CutTriangle(List<UIVertex> tris, int idx, List<UIVertex> list, in Vector2 cutDirection, in Vector2 point)
        {
            var a = tris[idx];
            var b = tris[idx + 1];
            var c = tris[idx + 2];

            float bc = OnLine(b.position, c.position, point, cutDirection);
            float ab = OnLine(a.position, b.position, point, cutDirection);
            float ca = OnLine(c.position, a.position, point, cutDirection);

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
            vert.color = Color.Lerp(v1.color, v2.color, f);
            vert.uv0 = Vector2.Lerp(v1.uv0, v2.uv0, f);
            vert.uv1 = Vector2.Lerp(v1.uv1, v2.uv1, f);
            vert.uv2 = Vector2.Lerp(v1.uv2, v2.uv2, f);
            vert.uv3 = Vector2.Lerp(v1.uv3, v2.uv3, f);

            return vert;
        }
    }
}