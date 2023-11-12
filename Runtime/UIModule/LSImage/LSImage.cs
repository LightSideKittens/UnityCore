using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace LSCore
{
    public partial class LSImage : Image
    {
        private static readonly LSVertexHelper vertexHelper = new LSVertexHelper();
        private Rect currentRect;
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            UpdateColorEvaluateFunc();
            base.OnValidate();
            CalculatePerpendicularPoints();
        }
        
        protected override void Reset()
        {
            base.Reset();
            gradient = new Gradient();
            CalculatePerpendicularPoints();
        }
#endif


        protected override void OnEnable()
        {
            base.OnEnable();
            gradient ??= new Gradient();
            UpdateColorEvaluateFunc();
        }

        protected override void UpdateGeometry()
        {
            DoMeshGeneration();
        }
        
        private void DoMeshGeneration()
        {
            if (rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
                OnPopulateMesh(vertexHelper);
            else
                vertexHelper.Clear(); // clear the vertex helper so invalid graphics dont draw.

            var components = ListPool<Component>.Get();
            GetComponents(typeof(IMeshModifier), components);

            VertexHelper legacy = null;
            
            if (components.Count > 0)
            {
                legacy = vertexHelper.ToLegacy();
            }
            for (var i = 0; i < components.Count; i++)
                ((IMeshModifier)components[i]).ModifyMesh(legacy);

            ListPool<Component>.Release(components);

            vertexHelper.FillMesh(workerMesh);
            canvasRenderer.SetMesh(workerMesh);
        }
        
        protected void OnPopulateMesh(LSVertexHelper toFill)
        {
            if (currentRect != rectTransform.rect)
            {
                CalculatePerpendicularPoints();
            }
            
            if (isColorDirty && cachedMesh != null)
            {
                toFill.Clear();
                var zero = Vector4.zero;
                
                for (int i = 0; i < cachedMesh.vertices.Length; i++)
                {
                    toFill.AddVert(cachedMesh.vertices[i], cachedMesh.colors[i], cachedMesh.uv[i], zero, zero, zero);
                }
                toFill.AddTriangle(0, 1, 2);
                toFill.AddTriangle(2, 3, 0);
                UpdateMeshColors(toFill);
                isColorDirty = false;
                return;
            }
            
            var activeSprite = overrideSprite;
            if (activeSprite == null)
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
            UpdateMeshColors(vh);
            cachedMesh = new Mesh();
            vh.FillMesh(cachedMesh);
        }

        private void RotateMesh(LSVertexHelper vh)
        {
            if(rotateId == 0) return;
            
            UIVertex vert = new UIVertex();
            
            for (int i = 0; i < vh.currentVertCount; i++)
            {
                vh.PopulateUIVertex(ref vert, i);
                var pos = vert.position;
                
                switch (rotateId)
                {
                    case 1:
                        pos = new Vector3(-pos.y, pos.x, pos.z);
                        break;

                    case 2:
                        pos = new Vector3(-pos.x, -pos.y, pos.z);
                        break;

                    case 3:
                        pos = new Vector3(pos.y, -pos.x, pos.z);
                        break;
                }

                vert.position = pos;
                vh.SetUIVertex(vert, i);
            }
        }

        private void CutMeshByGradient(LSVertexHelper vh)
        {
            CutMesh(vh);
            UpdateMeshColors(vh);
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
    }
}