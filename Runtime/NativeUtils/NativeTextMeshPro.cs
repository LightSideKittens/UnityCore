using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class NativeTextMeshPro : TextMeshProUGUI
    {
        private RawImage rawImage;
        private CanvasRenderer rawImageRenderer;
        
        protected override void Awake()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.Awake();
                return;
            }
#endif
            
            m_isAwake = true;
        }

        protected override void OnEnable()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.OnEnable();
                return;
            }
#endif
            if (m_isAwake == false)
                return;
            
            SetLayoutDirty();
        }

        protected override void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.OnDisable();
                return;
            }
#endif
        }

        protected override void OnDestroy()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.OnDestroy();
                return;
            }
#endif
        }

        protected override void GenerateTextMesh()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.GenerateTextMesh();
                return;
            }
#endif
        }

        protected override void UpdateGeometry()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.UpdateGeometry();
                return;
            }
#endif
        }

        public override void UpdateGeometry(Mesh mesh, int index)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.UpdateGeometry(mesh, index);
                return;
            }
#endif
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {

            if (Application.isEditor)
            {
                base.OnValidate();
                return;
            }

            if (m_isAwake == false)
                return;
            
            SetLayoutDirty();
        }
#endif

        public override void Rebuild(CanvasUpdate update)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.Rebuild(update);
                return;
            }
#endif
        }

        public override void Cull(Rect clipRect, bool validRect)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.Cull(clipRect, validRect);
                return;
            }
#endif
        }

        
        public override void SetLayoutDirty()
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                base.SetLayoutDirty();
                return;
            }
#endif
            
            base.SetLayoutDirty();
            
            if (rawImageRenderer != null)
            {
                rawImage.onCullStateChanged.RemoveListener(OnRawImageCullStateChanged);
                rawImage.onCullStateChanged.AddListener(OnRawImageCullStateChanged);

                if (rawImageRenderer.cull)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        DestroyImmediate(rawImage.texture);
                    }
                    else
                    {
                        Destroy(rawImage.texture);
                    }
#else
                    Destroy(rawImage.texture);
#endif
                    return;
                }
            }
            
            rawImage = TextRenderer.ConvertToNative(this);
            rawImageRenderer = canvasRenderer;
        }

        private void OnRawImageCullStateChanged(bool cull)
        {
            onCullStateChanged.Invoke(true);
            OnCullingChanged();
        }
    }
}