using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class NativeTextMeshPro : TextMeshProUGUI
    {
        private RawImage rawImage;
        private float width;
        private float height;
        private CanvasRenderer rawImageRenderer;
#if UNITY_EDITOR
        [SerializeField] private bool useDefault = true;
        private bool UseDefault => Application.isEditor && useDefault;
#endif
        public float basePreferredHeight => base.preferredHeight;

        public override float preferredHeight
        {
            get
            {
#if UNITY_EDITOR
                if (UseDefault)
                {
                    return base.preferredHeight;
                }
#endif

                Vector4 m = base.margin;
                float h = height;
                
                if (m.y > 0)
                {
                    h += m.y;
                }

                if (m.w > 0)
                {
                    h += m.w;
                }
                
                return h;
            }
        }

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (UseDefault)
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
            if (UseDefault)
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
            if (UseDefault)
            {
                base.OnDisable();
                return;
            }
#endif
        }

        private void DestroyRawImage()
        {
            DestroyRawImageTexture();
#if UNITY_EDITOR
            if (rawImage != null)
            {
                if (!Application.isPlaying)
                {
                    DestroyImmediate(rawImage.gameObject);
                }
                else
                {
                    Destroy(rawImage.gameObject);
                }
            }

#else
            if (rawImage != null)
            {
                Destroy(rawImage.gameObject);
            }
#endif
        }
        
        protected override void OnDestroy()
        {
            DestroyRawImage();
#if UNITY_EDITOR
            if (UseDefault)
            {
                base.OnDestroy();
                return;
            }
#endif
        }

        protected override void GenerateTextMesh()
        {
#if UNITY_EDITOR
            if (UseDefault)
            {
                base.GenerateTextMesh();
                return;
            }
#endif
        }

        protected override void UpdateGeometry()
        {
#if UNITY_EDITOR
            if (UseDefault)
            {
                base.UpdateGeometry();
                return;
            }
#endif
        }

        public override void UpdateGeometry(Mesh mesh, int index)
        {
#if UNITY_EDITOR
            if (UseDefault)
            {
                base.UpdateGeometry(mesh, index);
                return;
            }
#endif
        }
        
#if UNITY_EDITOR
        protected override void OnValidate()
        {

            if (UseDefault)
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
            if (UseDefault)
            {
                base.Rebuild(update);
                return;
            }
#endif
        }

        public override void Cull(Rect clipRect, bool validRect)
        {
#if UNITY_EDITOR
            if (UseDefault)
            {
                base.Cull(clipRect, validRect);
                return;
            }
#endif
        }

        
        public override void SetLayoutDirty()
        {
#if UNITY_EDITOR
            if (UseDefault)
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
                    DestroyRawImageTexture();
                }
            }
            
            var newRawImage = TextRenderer.ConvertToNative(this);
            
            if (newRawImage == null)
            {
                DestroyRawImage();
                rawImage = null;
                rawImageRenderer = null;
                width = 0;
                height = 0;
                return;
            }

            rawImage = newRawImage;
            var texture = rawImage.texture;
            width = texture.width;
            height = texture.height;
            rawImageRenderer = canvasRenderer;
        }

        private void DestroyRawImageTexture()
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

        private void OnRawImageCullStateChanged(bool cull)
        {
            onCullStateChanged.Invoke(true);
            OnCullingChanged();
        }
    }
}