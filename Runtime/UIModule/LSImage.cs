using System;
using System.Collections.Generic;
using System.Linq;
using LSCore.Extensions.Unity;
using UnityEditor;
#if UNITY_EDITOR
using System.Reflection;
using UnityEditor.UI;
#endif
using UnityEngine;
using UnityEngine.Sprites;
using UnityEngine.UI;
using UnityToolbarExtender;
using static UnityEngine.Mathf;

namespace LSCore
{
    public class LSImage : Image
    {
        [SerializeField] private int rotateId = 0;
        [SerializeField] private bool invert;
        [SerializeField] private Gradient gradient;
        [SerializeField] private float angle = 45;
        [SerializeField] private float gradientStart;
        [SerializeField] private float gradientEnd;
        private Vector2 gradientStartPoint;
        private Vector2 gradientEndPoint;

        public int RotateId
        {
            get => rotateId;
            set => rotateId = value;
        }
        
        public bool Invert
        {
            get => invert;
            set
            {
                invert = value;
                UpdateColorEvaluateFunc();
            }
        }
        public Gradient Gradient
        {
            get => gradient;
            set
            {
                gradient = value;
                SetVerticesDirty();
            }
        }
        
        public float Angle
        {
            get => angle;
            set
            {
                angle = value;
                CalculatePerpendicularPoints(currentRect);
                var direction = GradientDirection;
                gradientStartPoint.x = minPoint.x + direction.x * gradientStart;
                gradientStartPoint.y = minPoint.y + direction.y * gradientStart;
                gradientEndPoint.x = maxPoint.x - direction.x * gradientEnd;
                gradientEndPoint.y = maxPoint.y - direction.y * gradientEnd;

                if (gradient.colorKeys.Length > 2 || gradient.alphaKeys.Length > 2)
                {
                    SetVerticesDirty();
                }
                else
                {
                    UpdateMeshColors();
                }
            }
        }
        
        public float GradientStart
        {
            get => gradientStart;
            set
            {
                gradientStart = value;
                var direction = GradientDirection;
                gradientStartPoint.x = minPoint.x + direction.x * gradientStart;
                gradientStartPoint.y = minPoint.y + direction.y * gradientStart;
                UpdateMeshColors();
            }
        }
        
        public float GradientEnd
        {
            get => gradientEnd;
            set
            {
                gradientEnd = value;
                var direction = GradientDirection;
                gradientEndPoint.x = maxPoint.x - direction.x * gradientEnd;
                gradientEndPoint.y = maxPoint.y - direction.y * gradientEnd;
                UpdateMeshColors();
            }
        }
        
        internal Vector2 GradientStartPoint
        {
            get => gradientStartPoint;
            set
            {
                gradientStartPoint = value;
                UpdateMeshColors();
            }
        }
        
        internal Vector2 GradientEndPoint
        {
            get => gradientEndPoint;
            set
            {
                gradientEndPoint = value;
                UpdateMeshColors();
            }
        }

        internal Vector2 GradientDirection { get; private set; }

        private static readonly Vector2[] vertScratch = new Vector2[4];
        private static readonly Vector2[] uVScratch = new Vector2[4];
        
        private static readonly Vector3[] s_Xy = new Vector3[4];
        private static readonly Vector3[] s_Uv = new Vector3[4];
        private InFunc<Vector3, Color> colorEvaluate;
        
        private Color DefaulColor(in Vector3 pos) => color;
        private float GetGradientValue(Vector2 pos)
        {
            pos -= currentRect.center;
            return pos.UnclampedInverseLerp(gradientStartPoint, gradientEndPoint);
        }

        private Color ColorEvaluate(in Vector3 pos) => gradient.Evaluate(GetGradientValue(pos));
        private Color Inverted_ColorEvaluate(in Vector3 pos) => gradient.Evaluate(1 - GetGradientValue(pos));
        private InFunc<Vector3, Color> GetLeftToRightColorEvaluate() => invert ? Inverted_ColorEvaluate : ColorEvaluate;
        
        
        private void CalculatePerpendicularPoints(in Rect rect)
        {
            float angleRad = angle * Deg2Rad;
            var direction = new Vector2(Cos(angleRad), Sin(angleRad));
            var radius = rect.CircumscribedCircleRadius();
            float distanceForMinPoint = radius;
            float distanceForMaxPoint = radius;
            
            Vector2 minPointRef = direction * -radius;
            Vector2 maxPointRef = direction * radius;
            
            minPoint = minPointRef;
            maxPoint = maxPointRef;
            var corners = rect.Corners();

            for (int i = 0; i < corners.Length; i++)
            {
                corners[i] -= rect.center;
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

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            UpdateColorEvaluateFunc();
        }
#endif

        protected override void Reset()
        {
            base.Reset();
            gradient = new Gradient();
            var rect = GetPixelAdjustedRect();
            CalculatePerpendicularPoints(rect);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            gradient ??= new Gradient();
            UpdateColorEvaluateFunc();
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

        internal void UpdateMeshColors()
        {
            var vertices = workerMesh.vertices;
            var colors = workerMesh.colors;

            for (int i = 0; i < vertices.Length; i++)
            {
                colors[i] = colorEvaluate(vertices[i]);
            }
            
            workerMesh.colors = colors;
            canvasRenderer.SetMesh(workerMesh);
        }
        
        private void UpdateMeshColors(VertexHelper vh)
        {
            UIVertex vert = new UIVertex();

            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                vert.color = colorEvaluate(vert.position);
                vh.SetUIVertex(vert, i);
            }
        }
        
        protected override void UpdateGeometry()
        {
            base.UpdateGeometry();
            if(rotateId == 0) return;
            
            var vertices = workerMesh.vertices;

            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] -= Vector3.zero;

                switch (rotateId)
                {
                    case 1:
                        vertices[i] = new Vector3(-vertices[i].y, vertices[i].x, vertices[i].z);
                        break;

                    case 2:
                        vertices[i] = new Vector3(-vertices[i].x, -vertices[i].y, vertices[i].z);
                        break;

                    case 3:
                        vertices[i] = new Vector3(vertices[i].y, -vertices[i].x, vertices[i].z);
                        break;
                }
                
                // Translate vertex back to the original position based on the rotation point
                vertices[i] += Vector3.zero;
            }

            workerMesh.vertices = vertices;
            workerMesh.RecalculateBounds();
            workerMesh.RecalculateNormals();
            canvasRenderer.SetMesh(workerMesh);
        }

        private void GenerateDefaultSprite(VertexHelper vh)
        {
            var rect = GetPixelAdjustedRect();
            TryRotateRect(ref rect);
            currentRect = rect;
            var v = new Vector4(rect.x, rect.y, rect.x + rect.width, rect.y + rect.height);
            
            vh.Clear();
            AddVert(vh, new Vector3(v.x, v.y), new Vector2(0f, 0f));
            AddVert(vh, new Vector3(v.x, v.w), new Vector2(0f, 1f));
            AddVert(vh, new Vector3(v.z, v.w), new Vector2(1f, 1f));
            AddVert(vh, new Vector3(v.z, v.y), new Vector2(1f, 0f));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            var activeSprite = overrideSprite;
            if (activeSprite == null)
            {
                GenerateDefaultSprite(toFill);
                CutMeshByGradient(toFill);
                return;
            }

            switch (type)
            {
                case Type.Simple:
                    if (!useSpriteMesh)
                        GenerateSimpleSprite(toFill, preserveAspect);
                    else
                        GenerateSprite(toFill, preserveAspect);
                    break;
                case Type.Sliced:
                    GenerateSlicedSprite(toFill);
                    break;
                case Type.Tiled:
                    GenerateTiledSprite(toFill);
                    break;
                case Type.Filled:
                    GenerateFilledSprite(toFill, preserveAspect);
                    break;
            }
            
            CutMeshByGradient(toFill);
        }

        private void CutMeshByGradient(VertexHelper vh)
        {
            CutMesh(vh);
            UpdateMeshColors(vh);
        }
        
        private void CutMesh(VertexHelper vh)
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

        private Rect currentRect;
        internal Vector2 minPoint;
        internal Vector2 maxPoint;


        private void GenerateSlicedSprite(VertexHelper toFill)
        {
            if (!hasBorder)
            {
                GenerateSimpleSprite(toFill, false);
                return;
            }

            var activeSprite = overrideSprite;
            Vector4 outer, inner, padding, border;

            if (activeSprite != null)
            {
                outer = DataUtility.GetOuterUV(activeSprite);
                inner = DataUtility.GetInnerUV(activeSprite);
                padding = DataUtility.GetPadding(activeSprite);
                border = activeSprite.border;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                padding = Vector4.zero;
                border = Vector4.zero;
            }

            Rect rect = GetPixelAdjustedRect();
            Vector4 adjustedBorders = GetAdjustedBorders(border / multipliedPixelsPerUnit, rect);
            padding = padding / multipliedPixelsPerUnit;
            
            TryRotateRect(ref rect);
            currentRect = rect;
            
            vertScratch[0] = new Vector2(padding.x, padding.y);
            vertScratch[3] = new Vector2(rect.width - padding.z, rect.height - padding.w);

            vertScratch[1].x = adjustedBorders.x;
            vertScratch[1].y = adjustedBorders.y;

            vertScratch[2].x = rect.width - adjustedBorders.z;
            vertScratch[2].y = rect.height - adjustedBorders.w;
                
            for (int i = 0; i < 4; ++i)
            {
                vertScratch[i].x += rect.x;
                vertScratch[i].y += rect.y;
            }

            uVScratch[0] = new Vector2(outer.x, outer.y);
            uVScratch[1] = new Vector2(inner.x, inner.y);
            uVScratch[2] = new Vector2(inner.z, inner.w);
            uVScratch[3] = new Vector2(outer.z, outer.w);

            toFill.Clear();

            for (int x = 0; x < 3; ++x)
            {
                int x2 = x + 1;

                for (int y = 0; y < 3; ++y)
                {
                    if (!fillCenter && x == 1 && y == 1)
                        continue;

                    int y2 = y + 1;


                    AddQuad(toFill,
                        new Vector2(vertScratch[x].x, vertScratch[y].y),
                        new Vector2(vertScratch[x2].x, vertScratch[y2].y),
                        
                        new Vector2(uVScratch[x].x, uVScratch[y].y),
                        new Vector2(uVScratch[x2].x, uVScratch[y2].y));
                }
            }
        }
        
        void GenerateSimpleSprite(VertexHelper vh, bool lPreserveAspect)
        {
            var activeSprite = overrideSprite;
            Vector4 v = GetDrawingDimensions(lPreserveAspect);
            var uv = activeSprite != null ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            
            vh.Clear();
            
            AddVert(vh, new Vector3(v.x, v.y), new Vector2(uv.x, uv.y));
            AddVert(vh, new Vector3(v.x, v.w), new Vector2(uv.x, uv.w));
            AddVert(vh, new Vector3(v.z, v.w), new Vector2(uv.z, uv.w));
            AddVert(vh, new Vector3(v.z, v.y), new Vector2(uv.z, uv.y));

            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(2, 3, 0);
        }
        
        private Vector4 GetDrawingDimensions(bool shouldPreserveAspect)
        {
            var activeSprite = overrideSprite;
            var padding = activeSprite == null ? Vector4.zero : DataUtility.GetPadding(activeSprite);
            var size = activeSprite == null ? Vector2.zero : new Vector2(activeSprite.rect.width, activeSprite.rect.height);

            Rect rect = GetPixelAdjustedRect();
            // Debug.Log(string.Format("r:{2}, size:{0}, padding:{1}", size, padding, r));
            
            int spriteW = RoundToInt(size.x);
            int spriteH = RoundToInt(size.y);

            var v = new Vector4(
                padding.x / spriteW,
                padding.y / spriteH,
                (spriteW - padding.z) / spriteW,
                (spriteH - padding.w) / spriteH);

            if (shouldPreserveAspect && size.sqrMagnitude > 0.0f)
            {
                PreserveSpriteAspectRatio(ref rect, size);
            }
            
            TryRotateRect(ref rect);
            currentRect = rect;
            
            v = new Vector4(
                rect.x + rect.width * v.x,
                rect.y + rect.height * v.y,
                rect.x + rect.width * v.z,
                rect.y + rect.height * v.w
            );

            return v;
        }

        private void TryRotateRect(ref Rect rect)
        {
            if (rotateId % 2 == 1)
            {
                (rect.width, rect.height) = (rect.height, rect.width);
                var pos = rect.position;
                (pos.x, pos.y) = (pos.y, pos.x);
                rect.position = pos;
            }

            CalculatePerpendicularPoints(rect);
        }
        
        private void GenerateSprite(VertexHelper vh, bool lPreserveAspect)
        {
            var activeSprite = overrideSprite;
            var spriteSize = new Vector2(activeSprite.rect.width, activeSprite.rect.height);
            
            // Covert sprite pivot into normalized space.
            var spritePivot = activeSprite.pivot / spriteSize;
            var rectPivot = rectTransform.pivot;
            Rect rect = GetPixelAdjustedRect();
            
            if (lPreserveAspect & spriteSize.sqrMagnitude > 0.0f)
            {
                PreserveSpriteAspectRatio(ref rect, spriteSize);
            }
            
            TryRotateRect(ref rect);
            currentRect = rect;
            
            var drawingSize = new Vector2(rect.width, rect.height);
            var spriteBoundSize = activeSprite.bounds.size;

            // Calculate the drawing offset based on the difference between the two pivots.
            var drawOffset = (rectPivot - spritePivot) * drawingSize;
            
            vh.Clear();

            Vector2[] vertices = activeSprite.vertices;
            Vector2[] uvs = activeSprite.uv;
            for (int i = 0; i < vertices.Length; ++i)
            {
                AddVert(vh, new Vector3(vertices[i].x / spriteBoundSize.x * drawingSize.x - drawOffset.x, vertices[i].y / spriteBoundSize.y * drawingSize.y - drawOffset.y), new Vector2(uvs[i].x, uvs[i].y));
            }

            UInt16[] triangles = activeSprite.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                vh.AddTriangle(triangles[i + 0], triangles[i + 1], triangles[i + 2]);
            }
        }
        
        private void PreserveSpriteAspectRatio(ref Rect rect, Vector2 spriteSize)
        {
            var spriteRatio = spriteSize.x / spriteSize.y;
            var rectRatio = rect.width / rect.height;

            if (spriteRatio > rectRatio)
            {
                var oldHeight = rect.height;
                rect.height = rect.width * (1.0f / spriteRatio);
                rect.y += (oldHeight - rect.height) * rectTransform.pivot.y;
            }
            else
            {
                var oldWidth = rect.width;
                rect.width = rect.height * spriteRatio;
                rect.x += (oldWidth - rect.width) * rectTransform.pivot.x;
            }
        }
        
        private Vector4 GetAdjustedBorders(Vector4 border, Rect adjustedRect)
        {
            Rect originalRect = currentRect;

            for (int axis = 0; axis <= 1; axis++)
            {
                float borderScaleRatio;

                // The adjusted rect (adjusted for pixel correctness)
                // may be slightly larger than the original rect.
                // Adjust the border to match the adjustedRect to avoid
                // small gaps between borders (case 833201).
                if (originalRect.size[axis] != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / originalRect.size[axis];
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }

                // If the rect is smaller than the combined borders, then there's not room for the borders at their normal size.
                // In order to avoid artefacts with overlapping borders, we scale the borders down to fit.
                float combinedBorders = border[axis] + border[axis + 2];
                if (adjustedRect.size[axis] < combinedBorders && combinedBorders != 0)
                {
                    borderScaleRatio = adjustedRect.size[axis] / combinedBorders;
                    border[axis] *= borderScaleRatio;
                    border[axis + 2] *= borderScaleRatio;
                }
            }
            return border;
        }
        
        void AddQuad(VertexHelper vertexHelper, Vector3[] quadPositions, Vector3[] quadUVs)
        {
            int startIndex = vertexHelper.currentVertCount;

            for (int i = 0; i < 4; ++i)
                AddVert(vertexHelper, quadPositions[i], quadUVs[i]);

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax)
        {
            int startIndex = vertexHelper.currentVertCount;

            AddVert(vertexHelper, new Vector3(posMin.x, posMin.y, 0), new Vector2(uvMin.x, uvMin.y));
            AddVert(vertexHelper, new Vector3(posMin.x, posMax.y, 0), new Vector2(uvMin.x, uvMax.y));
            AddVert(vertexHelper, new Vector3(posMax.x, posMax.y, 0), new Vector2(uvMax.x, uvMax.y));
            AddVert(vertexHelper, new Vector3(posMax.x, posMin.y, 0), new Vector2(uvMax.x, uvMin.y));

            vertexHelper.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
            vertexHelper.AddTriangle(startIndex + 2, startIndex + 3, startIndex);
        }

        private void AddVert(in VertexHelper vertexHelper, in Vector3 position, in Vector4 uv0)
        {
            vertexHelper.AddVert(position, colorEvaluate(position), uv0);
        }
        
        void GenerateTiledSprite(VertexHelper toFill)
        {
            Vector4 outer, inner, border;
            Vector2 spriteSize;
            var activeSprite = overrideSprite;

            if (activeSprite != null)
            {
                outer = DataUtility.GetOuterUV(activeSprite);
                inner = DataUtility.GetInnerUV(activeSprite);
                border = activeSprite.border;
                spriteSize = activeSprite.rect.size;
            }
            else
            {
                outer = Vector4.zero;
                inner = Vector4.zero;
                border = Vector4.zero;
                spriteSize = Vector2.one * 100;
            }

            Rect rect = GetPixelAdjustedRect();

            float tileWidth = (spriteSize.x - border.x - border.z) / multipliedPixelsPerUnit;
            float tileHeight = (spriteSize.y - border.y - border.w) / multipliedPixelsPerUnit;
            
            border = GetAdjustedBorders(border / multipliedPixelsPerUnit, rect);
            
            TryRotateRect(ref rect);
            currentRect = rect;
            
            var uvMin = new Vector2(inner.x, inner.y);
            var uvMax = new Vector2(inner.z, inner.w);

            // Min to max max range for tiled region in coordinates relative to lower left corner.
            float xMin = border.x;
            float xMax = rect.width - border.z;
            float yMin = border.y;
            float yMax = rect.height - border.w;

            toFill.Clear();
            var clipped = uvMax;

            // if either width is zero we cant tile so just assume it was the full width.
            if (tileWidth <= 0)
                tileWidth = xMax - xMin;

            if (tileHeight <= 0)
                tileHeight = yMax - yMin;

            if (activeSprite != null && (hasBorder || activeSprite.packed || activeSprite.texture != null && activeSprite.texture.wrapMode != TextureWrapMode.Repeat))
            {
                // Sprite has border, or is not in repeat mode, or cannot be repeated because of packing.
                // We cannot use texture tiling so we will generate a mesh of quads to tile the texture.

                // Evaluate how many vertices we will generate. Limit this number to something sane,
                // especially since meshes can not have more than 65000 vertices.

                long nTilesW = 0;
                long nTilesH = 0;
                if (fillCenter)
                {
                    nTilesW = (long)Math.Ceiling((xMax - xMin) / tileWidth);
                    nTilesH = (long)Math.Ceiling((yMax - yMin) / tileHeight);

                    double nVertices = 0;
                    if (hasBorder)
                    {
                        nVertices = (nTilesW + 2.0) * (nTilesH + 2.0) * 4.0; // 4 vertices per tile
                    }
                    else
                    {
                        nVertices = nTilesW * nTilesH * 4.0; // 4 vertices per tile
                    }

                    if (nVertices > 65000.0)
                    {
                        Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, set the Wrap mode to Repeat in the Image Import Settings", this);

                        double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                        double imageRatio;
                        if (hasBorder)
                        {
                            imageRatio = (nTilesW + 2.0) / (nTilesH + 2.0);
                        }
                        else
                        {
                            imageRatio = (double)nTilesW / nTilesH;
                        }

                        double targetTilesW = Math.Sqrt(maxTiles / imageRatio);
                        double targetTilesH = targetTilesW * imageRatio;
                        if (hasBorder)
                        {
                            targetTilesW -= 2;
                            targetTilesH -= 2;
                        }

                        nTilesW = (long)Math.Floor(targetTilesW);
                        nTilesH = (long)Math.Floor(targetTilesH);
                        tileWidth = (xMax - xMin) / nTilesW;
                        tileHeight = (yMax - yMin) / nTilesH;
                    }
                }
                else
                {
                    if (hasBorder)
                    {
                        // Texture on the border is repeated only in one direction.
                        nTilesW = (long)Math.Ceiling((xMax - xMin) / tileWidth);
                        nTilesH = (long)Math.Ceiling((yMax - yMin) / tileHeight);
                        double nVertices = (nTilesH + nTilesW + 2.0 /*corners*/) * 2.0 /*sides*/ * 4.0 /*vertices per tile*/;
                        if (nVertices > 65000.0)
                        {
                            Debug.LogError("Too many sprite tiles on Image \"" + name + "\". The tile size will be increased. To remove the limit on the number of tiles, set the Wrap mode to Repeat in the Image Import Settings", this);

                            double maxTiles = 65000.0 / 4.0; // Max number of vertices is 65000; 4 vertices per tile.
                            double imageRatio = (double)nTilesW / nTilesH;
                            double targetTilesW = (maxTiles - 4 /*corners*/) / (2 * (1.0 + imageRatio));
                            double targetTilesH = targetTilesW * imageRatio;

                            nTilesW = (long)Math.Floor(targetTilesW);
                            nTilesH = (long)Math.Floor(targetTilesH);
                            tileWidth = (xMax - xMin) / nTilesW;
                            tileHeight = (yMax - yMin) / nTilesH;
                        }
                    }
                    else
                    {
                        nTilesH = nTilesW = 0;
                    }
                }

                if (fillCenter)
                {
                    // TODO: we could share vertices between quads. If vertex sharing is implemented. update the computation for the number of vertices accordingly.
                    for (long j = 0; j < nTilesH; j++)
                    {
                        float y1 = yMin + j * tileHeight;
                        float y2 = yMin + (j + 1) * tileHeight;
                        if (y2 > yMax)
                        {
                            clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                            y2 = yMax;
                        }
                        clipped.x = uvMax.x;
                        for (long i = 0; i < nTilesW; i++)
                        {
                            float x1 = xMin + i * tileWidth;
                            float x2 = xMin + (i + 1) * tileWidth;
                            if (x2 > xMax)
                            {
                                clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                                x2 = xMax;
                            }
                            AddQuad(toFill, new Vector2(x1, y1) + rect.position, new Vector2(x2, y2) + rect.position,  uvMin, clipped);
                        }
                    }
                }
                if (hasBorder)
                {
                    clipped = uvMax;
                    for (long j = 0; j < nTilesH; j++)
                    {
                        float y1 = yMin + j * tileHeight;
                        float y2 = yMin + (j + 1) * tileHeight;
                        if (y2 > yMax)
                        {
                            clipped.y = uvMin.y + (uvMax.y - uvMin.y) * (yMax - y1) / (y2 - y1);
                            y2 = yMax;
                        }
                        AddQuad(toFill,
                            new Vector2(0, y1) + rect.position,
                            new Vector2(xMin, y2) + rect.position,
                            
                            new Vector2(outer.x, uvMin.y),
                            new Vector2(uvMin.x, clipped.y));
                        AddQuad(toFill,
                            new Vector2(xMax, y1) + rect.position,
                            new Vector2(rect.width, y2) + rect.position,
                            
                            new Vector2(uvMax.x, uvMin.y),
                            new Vector2(outer.z, clipped.y));
                    }

                    // Bottom and top tiled border
                    clipped = uvMax;
                    for (long i = 0; i < nTilesW; i++)
                    {
                        float x1 = xMin + i * tileWidth;
                        float x2 = xMin + (i + 1) * tileWidth;
                        if (x2 > xMax)
                        {
                            clipped.x = uvMin.x + (uvMax.x - uvMin.x) * (xMax - x1) / (x2 - x1);
                            x2 = xMax;
                        }
                        AddQuad(toFill,
                            new Vector2(x1, 0) + rect.position,
                            new Vector2(x2, yMin) + rect.position,
                            
                            new Vector2(uvMin.x, outer.y),
                            new Vector2(clipped.x, uvMin.y));
                        AddQuad(toFill,
                            new Vector2(x1, yMax) + rect.position,
                            new Vector2(x2, rect.height) + rect.position,
                            
                            new Vector2(uvMin.x, uvMax.y),
                            new Vector2(clipped.x, outer.w));
                    }

                    // Corners
                    AddQuad(toFill,
                        new Vector2(0, 0) + rect.position,
                        new Vector2(xMin, yMin) + rect.position,
                        
                        new Vector2(outer.x, outer.y),
                        new Vector2(uvMin.x, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(xMax, 0) + rect.position,
                        new Vector2(rect.width, yMin) + rect.position,
                        
                        new Vector2(uvMax.x, outer.y),
                        new Vector2(outer.z, uvMin.y));
                    AddQuad(toFill,
                        new Vector2(0, yMax) + rect.position,
                        new Vector2(xMin, rect.height) + rect.position,
                        
                        new Vector2(outer.x, uvMax.y),
                        new Vector2(uvMin.x, outer.w));
                    AddQuad(toFill,
                        new Vector2(xMax, yMax) + rect.position,
                        new Vector2(rect.width, rect.height) + rect.position,
                        
                        new Vector2(uvMax.x, uvMax.y),
                        new Vector2(outer.z, outer.w));
                }
            }
            else
            {
                // Texture has no border, is in repeat mode and not packed. Use texture tiling.
                Vector2 uvScale = new Vector2((xMax - xMin) / tileWidth, (yMax - yMin) / tileHeight);

                if (fillCenter)
                {
                    AddQuad(toFill, new Vector2(xMin, yMin) + rect.position, new Vector2(xMax, yMax) + rect.position,  Vector2.Scale(uvMin, uvScale), Vector2.Scale(uvMax, uvScale));
                }
            }
        }
        
        void GenerateFilledSprite(VertexHelper toFill, bool preserveAspect)
        {
            toFill.Clear();

            if (fillAmount < 0.001f)
                return;

            var activeSprite = overrideSprite;
            Vector4 v = GetDrawingDimensions(preserveAspect);
            Vector4 outer = activeSprite != null ? DataUtility.GetOuterUV(activeSprite) : Vector4.zero;
            UIVertex uiv = UIVertex.simpleVert;
            uiv.color = color;

            float tx0 = outer.x;
            float ty0 = outer.y;
            float tx1 = outer.z;
            float ty1 = outer.w;

            // Horizontal and vertical filled sprites are simple -- just end the Image prematurely
            if (fillMethod == FillMethod.Horizontal || fillMethod == FillMethod.Vertical)
            {
                if (fillMethod == FillMethod.Horizontal)
                {
                    float fill = (tx1 - tx0) * fillAmount;

                    if (fillOrigin == 1)
                    {
                        v.x = v.z - (v.z - v.x) * fillAmount;
                        tx0 = tx1 - fill;
                    }
                    else
                    {
                        v.z = v.x + (v.z - v.x) * fillAmount;
                        tx1 = tx0 + fill;
                    }
                }
                else if (fillMethod == FillMethod.Vertical)
                {
                    float fill = (ty1 - ty0) * fillAmount;

                    if (fillOrigin == 1)
                    {
                        v.y = v.w - (v.w - v.y) * fillAmount;
                        ty0 = ty1 - fill;
                    }
                    else
                    {
                        v.w = v.y + (v.w - v.y) * fillAmount;
                        ty1 = ty0 + fill;
                    }
                }
            }

            s_Xy[0] = new Vector2(v.x, v.y);
            s_Xy[1] = new Vector2(v.x, v.w);
            s_Xy[2] = new Vector2(v.z, v.w);
            s_Xy[3] = new Vector2(v.z, v.y);

            s_Uv[0] = new Vector2(tx0, ty0);
            s_Uv[1] = new Vector2(tx0, ty1);
            s_Uv[2] = new Vector2(tx1, ty1);
            s_Uv[3] = new Vector2(tx1, ty0);

            {
                if (fillAmount < 1f && fillMethod != FillMethod.Horizontal && fillMethod != FillMethod.Vertical)
                {
                    if (fillMethod == FillMethod.Radial90)
                    {
                        if (RadialCut(s_Xy, s_Uv, fillAmount, fillClockwise, fillOrigin))
                            AddQuad(toFill, s_Xy,  s_Uv);
                    }
                    else if (fillMethod == FillMethod.Radial180)
                    {
                        for (int side = 0; side < 2; ++side)
                        {
                            float fx0, fx1, fy0, fy1;
                            int even = fillOrigin > 1 ? 1 : 0;

                            if (fillOrigin == 0 || fillOrigin == 2)
                            {
                                fy0 = 0f;
                                fy1 = 1f;
                                if (side == even)
                                {
                                    fx0 = 0f;
                                    fx1 = 0.5f;
                                }
                                else
                                {
                                    fx0 = 0.5f;
                                    fx1 = 1f;
                                }
                            }
                            else
                            {
                                fx0 = 0f;
                                fx1 = 1f;
                                if (side == even)
                                {
                                    fy0 = 0.5f;
                                    fy1 = 1f;
                                }
                                else
                                {
                                    fy0 = 0f;
                                    fy1 = 0.5f;
                                }
                            }

                            s_Xy[0].x = Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = fillClockwise ? fillAmount * 2f - side : fillAmount * 2f - (1 - side);

                            if (RadialCut(s_Xy, s_Uv, Clamp01(val), fillClockwise, (side + fillOrigin + 3) % 4))
                            {
                                AddQuad(toFill, s_Xy,  s_Uv);
                            }
                        }
                    }
                    else if (fillMethod == FillMethod.Radial360)
                    {
                        for (int corner = 0; corner < 4; ++corner)
                        {
                            float fx0, fx1, fy0, fy1;

                            if (corner < 2)
                            {
                                fx0 = 0f;
                                fx1 = 0.5f;
                            }
                            else
                            {
                                fx0 = 0.5f;
                                fx1 = 1f;
                            }

                            if (corner == 0 || corner == 3)
                            {
                                fy0 = 0f;
                                fy1 = 0.5f;
                            }
                            else
                            {
                                fy0 = 0.5f;
                                fy1 = 1f;
                            }

                            //TODO:
                            s_Xy[0].x = Lerp(v.x, v.z, fx0);
                            s_Xy[1].x = s_Xy[0].x;
                            s_Xy[2].x = Lerp(v.x, v.z, fx1);
                            s_Xy[3].x = s_Xy[2].x;

                            s_Xy[0].y = Lerp(v.y, v.w, fy0);
                            s_Xy[1].y = Lerp(v.y, v.w, fy1);
                            s_Xy[2].y = s_Xy[1].y;
                            s_Xy[3].y = s_Xy[0].y;

                            s_Uv[0].x = Lerp(tx0, tx1, fx0);
                            s_Uv[1].x = s_Uv[0].x;
                            s_Uv[2].x = Lerp(tx0, tx1, fx1);
                            s_Uv[3].x = s_Uv[2].x;

                            s_Uv[0].y = Lerp(ty0, ty1, fy0);
                            s_Uv[1].y = Lerp(ty0, ty1, fy1);
                            s_Uv[2].y = s_Uv[1].y;
                            s_Uv[3].y = s_Uv[0].y;

                            float val = fillClockwise ?
                                fillAmount * 4f - (corner + fillOrigin) % 4 :
                                fillAmount * 4f - (3 - (corner + fillOrigin) % 4);

                            if (RadialCut(s_Xy, s_Uv, Clamp01(val), fillClockwise, (corner + 2) % 4))
                                AddQuad(toFill, s_Xy,  s_Uv);
                        }
                    }
                }
                else
                {
                    AddQuad(toFill, s_Xy,  s_Uv);
                }
            }
        }
        
        
        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>

        static bool RadialCut(Vector3[] xy, Vector3[] uv, float fill, bool invert, int corner)
        {
            // Nothing to fill
            if (fill < 0.001f) return false;

            // Even corners invert the fill direction
            if ((corner & 1) == 1) invert = !invert;

            // Nothing to adjust
            if (!invert && fill > 0.999f) return true;

            // Convert 0-1 value into 0 to 90 degrees angle in radians
            float angle = Clamp01(fill);
            if (invert) angle = 1f - angle;
            angle *= 90f * Deg2Rad;

            // Calculate the effective X and Y factors
            float cos = Cos(angle);
            float sin = Sin(angle);

            RadialCut(xy, cos, sin, invert, corner);
            RadialCut(uv, cos, sin, invert, corner);
            return true;
        }

        /// <summary>
        /// Adjust the specified quad, making it be radially filled instead.
        /// </summary>

        static void RadialCut(Vector3[] xy, float cos, float sin, bool invert, int corner)
        {
            int i0 = corner;
            int i1 = (corner + 1) % 4;
            int i2 = (corner + 2) % 4;
            int i3 = (corner + 3) % 4;

            if ((corner & 1) == 1)
            {
                if (sin > cos)
                {
                    cos /= sin;
                    sin = 1f;

                    if (invert)
                    {
                        xy[i1].x = Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i2].x = xy[i1].x;
                    }
                }
                else if (cos > sin)
                {
                    sin /= cos;
                    cos = 1f;

                    if (!invert)
                    {
                        xy[i2].y = Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i3].y = xy[i2].y;
                    }
                }
                else
                {
                    cos = 1f;
                    sin = 1f;
                }

                if (!invert) xy[i3].x = Lerp(xy[i0].x, xy[i2].x, cos);
                else xy[i1].y = Lerp(xy[i0].y, xy[i2].y, sin);
            }
            else
            {
                if (cos > sin)
                {
                    sin /= cos;
                    cos = 1f;

                    if (!invert)
                    {
                        xy[i1].y = Lerp(xy[i0].y, xy[i2].y, sin);
                        xy[i2].y = xy[i1].y;
                    }
                }
                else if (sin > cos)
                {
                    cos /= sin;
                    sin = 1f;

                    if (invert)
                    {
                        xy[i2].x = Lerp(xy[i0].x, xy[i2].x, cos);
                        xy[i3].x = xy[i2].x;
                    }
                }
                else
                {
                    cos = 1f;
                    sin = 1f;
                }

                if (invert) xy[i3].y = Lerp(xy[i0].y, xy[i2].y, sin);
                else xy[i1].x = Lerp(xy[i0].x, xy[i2].x, cos);
            }
        }
    }
    
        
#if UNITY_EDITOR
    
    [CustomEditor(typeof(LSImage), true)]
    [CanEditMultipleObjects]
    public class LSImageEditor : ImageEditor
    {
        SerializedProperty rotateId;
        SerializedProperty invert;
        SerializedProperty gradient;
        SerializedProperty angle;
        SerializedProperty gradientStart;
        SerializedProperty gradientEnd;
        
        SerializedProperty m_Sprite;
        SerializedProperty m_Type;
        SerializedProperty m_PreserveAspect;
        SerializedProperty m_UseSpriteMesh;
        FieldInfo m_bIsDriven;
        private LSImage image;
        private RectTransform rect;
        private bool isDragging;
        private bool isEditing;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_Sprite = serializedObject.FindProperty("m_Sprite");
            m_Type = serializedObject.FindProperty("m_Type");
            m_PreserveAspect = serializedObject.FindProperty("m_PreserveAspect");
            m_UseSpriteMesh = serializedObject.FindProperty("m_UseSpriteMesh");
            rotateId = serializedObject.FindProperty("rotateId");
            invert = serializedObject.FindProperty("invert");
            gradient = serializedObject.FindProperty("gradient");
            angle = serializedObject.FindProperty("angle");
            gradientStart = serializedObject.FindProperty("gradientStart");
            gradientEnd = serializedObject.FindProperty("gradientEnd");
            var type = GetType().BaseType;
            m_bIsDriven = type.GetField("m_bIsDriven", BindingFlags.Instance | BindingFlags.NonPublic);
            image = (LSImage)target;
            rect = image.GetComponent<RectTransform>();
            isEditing = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            //m_bIsDriven.SetValue(this, (rect.drivenByObject as Slider)?.fillRect == rect);

            SpriteGUI();
            EditorGUILayout.PropertyField(gradient);
            EditorGUILayout.PropertyField(invert);
            if (GUILayout.Button(isEditing ? "Stop Edit" : "Edit"))
            {
                isEditing = !isEditing;
            }
            EditorGUILayout.PropertyField(m_Material);
            RaycastControlsGUI();
            MaskableControlsGUI();
            
            TypeGUI();
            SetShowNativeSize(false);
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUI.indentLevel++;

                if ((Image.Type)m_Type.enumValueIndex == Image.Type.Simple)
                    EditorGUILayout.PropertyField(m_UseSpriteMesh);

                EditorGUILayout.PropertyField(m_PreserveAspect);
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
            NativeSizeButtonGUI();
            
            DrawRotateButton();
            
            serializedObject.ApplyModifiedProperties();
        }

        void SetShowNativeSize(bool instant)
        {
            Image.Type type = (Image.Type)m_Type.enumValueIndex;
            bool showNativeSize = (type == Image.Type.Simple || type == Image.Type.Filled) && m_Sprite.objectReferenceValue != null;
            base.SetShowNativeSize(showNativeSize, instant);
        }
        
        protected virtual void DrawRotateButton()
        {
            GUILayout.Space(10);
            if (GUILayout.Button("Rotate"))
            {
                rotateId.intValue = (rotateId.intValue + 1) % 4;
            }
        }

        private Dictionary<int, int> map = new();
        
        void OnSceneGUI()
        {
            if(!isEditing) return;
            Vector3 imagePosition = (Vector3)image.rectTransform.rect.center + image.transform.position;
            var start = imagePosition + (Vector3)image.GradientStartPoint;
            var end = imagePosition + (Vector3)image.GradientEndPoint;
            float handle1Size = HandleUtility.GetHandleSize(start) * 0.1f;
            float handle2Size = HandleUtility.GetHandleSize(end) * 0.1f;
            Handles.DrawSolidDisc(start, Vector3.forward, handle1Size);
            Handles.DrawSolidDisc(end, Vector3.forward, handle2Size);
            var id1 = GUIUtility.GetControlID(FocusType.Passive);
            var id2 = GUIUtility.GetControlID(FocusType.Passive);
            if(id1 == -1)return;
            map.Clear();
            map.Add(id1, 0);
            map.Add(id2, 1);
            HandleInput(id1);
            HandleInput(id2);
            EditorGUI.BeginChangeCheck();
            Vector3 newHandle1Pos = Handles.FreeMoveHandle(id1, start, handle1Size * 1.5f, Vector3.zero, Handles.CircleHandleCap);
            Vector3 newHandle2Pos = Handles.FreeMoveHandle(id2, end, handle2Size * 1.5f, Vector3.zero, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                newHandle1Pos -= imagePosition;
                newHandle2Pos -= imagePosition;
                image.GradientStart = newHandle1Pos.UnclampedInverseLerp(image.minPoint, image.maxPoint);
                image.GradientEnd = newHandle2Pos.UnclampedInverseLerp(image.maxPoint, image.minPoint);
                image.UpdateMeshColors();
                Undo.RecordObject(image, "Move Handle");
                EditorUtility.SetDirty(image);
            }

            EditorGUI.BeginChangeCheck();
            var newAngle = Handles.RotationHandle(Quaternion.Euler(Vector3.forward * image.Angle), imagePosition);
            if (EditorGUI.EndChangeCheck())
            {
                image.Angle = newAngle.eulerAngles.z;
            }
        }
        
        private static readonly float DoubleClickTime = 0.3f; // Time threshold for double click
        private float lastClickTime = 0f;
        private int currentControlId;
        
        private void HandleInput(int controlId)
        {
            Event e = Event.current;
            var type = e.GetTypeForControl(controlId);
            
            switch (type)
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlId && e.button == 0) // Left mouse button
                    {
                        float timeSinceLastClick = (float)(EditorApplication.timeSinceStartup - lastClickTime);
                    
                        if (timeSinceLastClick < DoubleClickTime)
                        {
                            OpenColorPicker(map[controlId]);
                        }

                        lastClickTime = (float)EditorApplication.timeSinceStartup;
                    }
                    break;
                case EventType.ExecuteCommand:
                    if (e.commandName == "ColorPickerChanged")
                    {
                        var colors = image.Gradient.colorKeys;
                        var alphaKeys = image.Gradient.alphaKeys;
                        colors[currentControlId].color = LSColorPicker.Color;
                        alphaKeys[currentControlId].alpha = LSColorPicker.Color.a;
                        image.Gradient.SetKeys(colors, alphaKeys);
                        image.SetVerticesDirty();
                    }
                    break;
            }
        }
        
        private void OpenColorPicker(int cotrolID)
        {
            currentControlId = cotrolID;
            var color = image.Gradient.colorKeys[cotrolID].color;
            color.a = image.Gradient.alphaKeys[cotrolID].alpha;
            
            LSColorPicker.Show(newColor => { }, color);
        }
        
        [MenuItem("GameObject/LSCore/Image")]
        private static void CreateButton()
        {
            new GameObject("LSImage").AddComponent<LSImage>();
        }
    }
#endif
}