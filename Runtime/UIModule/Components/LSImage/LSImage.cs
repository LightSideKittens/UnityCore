﻿using System;
using System.Collections;
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
        private bool isShowed;
        public event Action Showed;
        public event Action Hidden;
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            rt = rectTransform;
            base.OnValidate();
        }
        
        protected override void Reset()
        {
            rt = rectTransform;
            base.Reset();
        }
#endif

        protected override void OnEnable()
        {
            rt = rectTransform;
            base.OnEnable();
            StartCoroutine(DelayedOnEnable());
        }

        private IEnumerator DelayedOnEnable()
        {
            yield return null;
            
            if (!canvasRenderer.cull)
            {
                OnShowed();
            }
            
            onCullStateChanged.AddListener(OnCullStateChanged);
        }

        private void OnCullStateChanged(bool cull)
        {
            if (cull)
            {
                OnHidden();
            }
            else
            {
                OnShowed();
            }
        }
        
        protected override void OnDisable()
        {
            onCullStateChanged.RemoveListener(OnCullStateChanged);
            
            var cull = canvasRenderer.cull;
            
            base.OnDisable();
            
            if (!cull)
            {
                OnHidden();
            }
        }

        protected virtual void OnShowed()
        {
            if(isShowed) return;
            
            isShowed = true;
            Showed?.Invoke();
        }
        
        protected virtual void OnHidden()
        {
            if(!isShowed) return;
            
            isShowed = false;
            Hidden?.Invoke();
        }
        
        protected override void UpdateGeometry()
        {
            DoMeshGeneration();
        }
        
        private void DoMeshGeneration()
        {
            Action<Mesh> fillMesh = vertexHelper.FillMesh;
            if (rt != null && rt.rect is { width: > 0, height: > 0 })
                OnPopulateMesh(vertexHelper);
            else
                vertexHelper.Clear(); // clear the vertex helper so invalid graphics dont draw.

            var components = ListPool<Component>.Get();
            GetComponents(typeof(IMeshModifier), components);
            
            if (components.Count > 0)
            {
                vertexHelper.FillLegacy(legacyVertexHelper);
                fillMesh = legacyVertexHelper.FillMesh;
            }

            for (var i = 0; i < components.Count; i++)
            {
                ((IMeshModifier)components[i]).ModifyMesh(legacyVertexHelper);
            }

            ListPool<Component>.Release(components);
            
            fillMesh(workerMesh);
            canvasRenderer.SetMesh(workerMesh);
        }
        
        
        private void OnPopulateMesh(LSVertexHelper toFill)
        {
            defaultColor = color;
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
                    if (combineFilledWithSliced && hasBorder && type == Type.Filled && fillMethod is FillMethod.Horizontal or FillMethod.Vertical)
                    {
                        GenerateFilledSlicedSprite(toFill);
                    }
                    else
                    {
                        GenerateFilledSprite(toFill, preserveAspect);
                    }
                    break;
            }

            PostProcessMesh(toFill);
        }
        
        private void PostProcessMesh(LSVertexHelper vh)
        {
            RotateMesh(vh);
        }

        public delegate void RotateAction(ref Vector3 value, in Vector2 center);
        private RotateAction rotateAction;

        private void RotateMesh(LSVertexHelper vh)
        {
            UIVertex vert = new UIVertex();
            var count = vh.currentVertCount;
            var center = rt.rect.center * 2;

            rotateAction = rotateId switch
            {
                1 => Rotate90,
                2 => Rotate180,
                3 => Rotate270,
                _ => null
            };

            rotateAction += Invert;
            
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

        private void Invert(ref Vector3 pos, in Vector2 center)
        {
            float xOffset = pos.x - center.x;
            float yOffset = pos.y - center.y;
            
            if (flip.x == 1)
            {
                pos.x = -xOffset + center.x;
            }

            if (flip.y == 1)
            {
                pos.y = -yOffset + center.y;
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
    }
}