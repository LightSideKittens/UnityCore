using System.Reflection;
using LSCore;
using UnityEngine;
using UnityEngine.UI;

public class EmojiImage : RawImage
{
    private static OnOffPool<EmojiImage> Pool => pool ??= OnOffPool<EmojiImage>.GetOrCreatePool(emojiImagePrefab);
    private static OnOffPool<EmojiImage> pool;
    private static EmojiImage emojiImagePrefab;
    private bool vertsDirty;
    private bool materialDirty;
    private static MethodInfo updateClipParent;


    static EmojiImage()
    {
        updateClipParent = typeof(MaskableGraphic).GetMethod("UpdateClipParent",
            BindingFlags.NonPublic | BindingFlags.Instance);
#if UNITY_EDITOR
        World.Creating += () =>
        {
            if (emojiImagePrefab != null)
            {
                Pool.Created -= OnCreate;
                emojiImagePrefab = null;
                pool = null;
            }
        };

        World.Destroyed += () =>
        {
            if (pool != null)
            {
                pool.Created -= OnCreate;
            }
            emojiImagePrefab = null;
            pool = null;
        };
#endif
    }

    private static void OnCreate(EmojiImage rawImage)
    {
        rawImage.gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    public static void InitPool()
    {
        if (emojiImagePrefab is null)
        {
            emojiImagePrefab = new GameObject("Emoji.Image").AddComponent<EmojiImage>();
            emojiImagePrefab.gameObject.hideFlags = HideFlags.HideAndDontSave;
            Pool.Created += OnCreate;
            if (World.IsPlaying)
            {
                DontDestroyOnLoad(emojiImagePrefab.gameObject);
            }
        }
    }

    public static EmojiImage Get() => Pool.Get();
    public static void Release(EmojiImage image) => Pool.Release(image);

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
        Pool.Remove(this);
#if UNITY_EDITOR
        GraphicRebuildTracker.UnTrackGraphic(this);
#endif
        GraphicRegistry.UnregisterGraphicForCanvas(canvas, this);
        Canvas.willRenderCanvases -= Rebuild;
        if (m_CachedMesh)
            Destroy(m_CachedMesh);
        m_CachedMesh = null;
    }

    public override void OnCullingChanged()
    {
    }

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

    public override void Rebuild(CanvasUpdate update)
    {
    }

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