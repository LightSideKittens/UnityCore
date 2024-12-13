using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace LSCore
{
    public class EmojiImage : RawImage
    {
        private bool vertsDirty;
        private bool materialDirty;
        private static MethodInfo updateClipParent;

        static EmojiImage()
        {
            updateClipParent = typeof(MaskableGraphic).GetMethod("UpdateClipParent", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Canvas.willRenderCanvases += Rebuild;
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Canvas.willRenderCanvases -= Rebuild;
            Canvas.willRenderCanvases -= Rebuild;
            Canvas.willRenderCanvases += Rebuild;
        }
#endif

        protected override void OnDisable()
        {
#if UNITY_EDITOR
            GraphicRebuildTracker.UnTrackGraphic(this);
#endif
            GraphicRegistry.DisableGraphicForCanvas(canvas, this);
            Canvas.willRenderCanvases -= Rebuild;

            if (canvasRenderer != null)
                canvasRenderer.Clear();

            LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            
            m_ShouldRecalculateStencil = true;
            SetMaterialDirty();
            updateClipParent.Invoke(this, null);
            StencilMaterial.Remove(m_MaskMaterial);
            m_MaskMaterial = null;

            if (isMaskingGraphic)
            {
                MaskUtilities.NotifyStencilStateChanged(this);
            }
        }

        protected override void OnDestroy()
        {
#if UNITY_EDITOR
            GraphicRebuildTracker.UnTrackGraphic(this);
#endif
            GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
            Canvas.willRenderCanvases -= Rebuild;
            if (m_CachedMesh)
                Destroy(m_CachedMesh);
            m_CachedMesh = null;
        }

        public override void OnCullingChanged() { }

        public override void SetMaterialDirty()
        {
            if (!IsActive())
                return;

            materialDirty = true;

            if (m_OnDirtyMaterialCallback != null)
                m_OnDirtyMaterialCallback();
        }

        public override void SetVerticesDirty()
        {
            if (!IsActive())
                return;

            vertsDirty = true;

            if (m_OnDirtyVertsCallback != null)
                m_OnDirtyVertsCallback();
        }

        public override void Rebuild(CanvasUpdate update) { }

        private void Rebuild()
        {
            if (canvasRenderer == null || canvasRenderer.cull)
                return;

            if (vertsDirty)
            {
                UpdateGeometry();
                vertsDirty = false;
            }
            if (materialDirty)
            {
                UpdateMaterial();
                materialDirty = false;
            }
        }
    }
}