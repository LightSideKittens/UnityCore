using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace LSCore
{
    public partial class LSImage : Image
    {
        private static readonly LSVertexHelper vertexHelper = new LSVertexHelper();
        private static readonly VertexHelper legacyVertexHelper = new VertexHelper();
        private RectTransform rt;
        private Rect currentRect;
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            rt = rectTransform;
            UpdateColorEvaluateFunc();
            base.OnValidate();
            CalculatePerpendicularPoints();
        }
        
        protected override void Reset()
        {
            rt = rectTransform;
            base.Reset();
            gradient = new LSGradient(new LSGradient.Key(0, Color.white));
            CalculatePerpendicularPoints();
        }
#endif


        protected override void OnEnable()
        {
            rt = rectTransform;
            base.OnEnable();
            gradient ??= new LSGradient(new LSGradient.Key(0, Color.white));
            UpdateColorEvaluateFunc();
        }

        protected override void UpdateGeometry()
        {
            DoMeshGeneration();
        }
        
        private void DoMeshGeneration()
        {
            if (rt != null && rt.rect is { width: > 0, height: > 0 })
                OnPopulateMesh(vertexHelper);
            else
                vertexHelper.Clear(); // clear the vertex helper so invalid graphics dont draw.

            var components = ListPool<Component>.Get();
            GetComponents(typeof(IMeshModifier), components);
            
            if (components.Count > 0)
            {
                vertexHelper.FillLegacy(legacyVertexHelper);
            }
            
            for (var i = 0; i < components.Count; i++)
                ((IMeshModifier)components[i]).ModifyMesh(legacyVertexHelper);

            ListPool<Component>.Release(components);

            if (skipFillMesh)
            {
                skipFillMesh = false;
            }
            else
            {
                vertexHelper.FillMesh(resultMesh);
                resultMeshVerts = vertexHelper.positions.ToArray();
                resultMeshColors = new Color[resultMeshVerts.Length];
            }
            
            canvasRenderer.SetMesh(resultMesh);
        }
        
        
        private void OnPopulateMesh(LSVertexHelper toFill)
        {
            if (isRectDirty && gradient.Count > 1)
            {
                CalculatePerpendicularPoints();
                isRectDirty = false;
            }
            
            if (isColorDirty && resultMesh != null)
            {
                skipFillMesh = true;
                UpdateMeshColors();
                isColorDirty = false;
                return;
            }

            if (isGradientDirty && TryGetCachedVertextHelper(toFill, withoutGradientMesh))
            {
                CutMeshForGradient(toFill);
                UpdateMeshColors(toFill);
                isGradientDirty = false;
                return;
            }
            
            if (overrideSprite == null)
            {
                GenerateDefaultSprite(toFill);
                PostProcessMesh(toFill);
                
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

            PostProcessMesh(toFill);
        }

        private void PostProcessMesh(LSVertexHelper vh)
        {
            RotateMesh(vh);
            withoutGradientMesh = new Mesh();
            vh.FillMesh(withoutGradientMesh);
            
            if (gradient.Count > 1)
            {
                CutMeshForGradient(vh);
            }
            
            UpdateMeshColors(vh);
            resultMesh = new Mesh();
        }

        public delegate void RotateAction(ref Vector3 value, in Vector2 center);
        private RotateAction rotateAction;

        private void RotateMesh(LSVertexHelper vh)
        {
            if(rotateId == 0) return;
            
            UIVertex vert = new UIVertex();
            var count = vh.currentVertCount;
            var center = rt.rect.center * 2;

            rotateAction = rotateId switch
            {
                1 => Rotate90,
                2 => Rotate180,
                3 => Rotate270,
                _ => rotateAction
            };
            
            for (int i = 0; i < count; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                var pos = vert.position;
                rotateAction(ref pos, center);
                vert.position = pos;
                vh.SetUIVertex(vert, i);
            }

            count = vh.currentIndexCount / 6 * 4;
            vh.ClearTriangles();
            
            if (rotateId % 2 == 1)
            {
                for (int i = 0; i < count; i += 4)
                {
                    vh.AddTriangle(i+1, i + 2, i+3);
                    vh.AddTriangle(i+3, i, i+1);
                }
            }
            else
            {
                for (int i = 0; i < count; i += 4)
                {
                    vh.AddTriangle(i, i + 1, i + 2);
                    vh.AddTriangle(i + 2, i + 3, i);
                }
            }

        }

        private void Rotate90(ref Vector3 pos, in Vector2 center)
        {
            var x = pos.x;
            pos.x = -pos.y + center.x;
            pos.y = x;
        }
        
        private void Rotate180(ref Vector3 pos, in Vector2 center)
        {
            pos.x = -pos.x + center.x;
            pos.y = -pos.y + center.y;
        }
        
        private void Rotate270(ref Vector3 pos, in Vector2 center)
        {
            var x = pos.x;
            pos.x = pos.y;
            pos.y = -x + center.y;
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
        }

        private bool TryGetCachedVertextHelper(LSVertexHelper vh, Mesh mesh)
        {
            var canCompute = mesh != null;
            
            if (canCompute)
            {
                vh.Clear();
                var zero = Vector4.zero;
                var verts = mesh.vertices;
                var tris = mesh.triangles;
                var uvs = mesh.uv;
                var uv0 = new Vector4();

                if (isColorDirty)
                {
                    for (int i = 0; i < verts.Length; i++)
                    {
                        var uv = uvs[i];
                        uv0.x = uv.x;
                        uv0.y = uv.y;
                        vh.AddVert(verts[i], defaultColor, uv0, zero, zero, zero);
                    }
                }
                else
                {
                    var colors = mesh.colors;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        var uv = uvs[i];
                        uv0.x = uv.x;
                        uv0.y = uv.y;
                        vh.AddVert(verts[i], colors[i], uv0, zero, zero, zero);
                    }
                }

                for (int i = 0; i < tris.Length; i += 3)
                {
                    vh.AddTriangle(tris[i], tris[i+1], tris[i+2]);
                }
            }

            return canCompute;
        }
    }
}