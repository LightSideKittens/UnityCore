using System.Reflection;
using LSCore;
using LSCore.NativeUtils;
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
        vertexHelper.Init();
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

    private Emoji.Sprite sprite;
    public Emoji.Sprite Sprite
    {
        set
        {
            sprite = value;
            texture = value.Atlas.Texture;
        }
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

    protected override void UpdateGeometry()
    {
        DoMeshGeneration();
    }
    
    private static readonly LSVertexHelper vertexHelper = new();
    
    private void DoMeshGeneration()
    {
        if (rectTransform != null && rectTransform.rect.width >= 0 && rectTransform.rect.height >= 0)
            DoMesh(vertexHelper, sprite.uvMin, sprite.uvMax);
        else
            vertexHelper.Clear(); // clear the vertex helper so invalid graphics dont draw.

        vertexHelper.FillMesh(workerMesh);
        canvasRenderer.SetMesh(workerMesh);
    }
    
    private void DoMesh(LSVertexHelper vh, Vector2 uvMin, Vector2 uvMax)
    {
        var r = GetPixelAdjustedRect();
        var v = new Vector4(r.x, r.y, r.x + r.width, r.y + r.height);

        Color32 c = color;
        vh.Clear();
        vh.AddVert(new Vector3(v.x, v.y), c, new Vector2(uvMin.x, uvMin.y));
        vh.AddVert(new Vector3(v.x, v.w), c, new Vector2(uvMin.x, uvMax.y));
        vh.AddVert(new Vector3(v.z, v.w), c, new Vector2(uvMax.x, uvMax.y));
        vh.AddVert(new Vector3(v.z, v.y), c, new Vector2(uvMax.x, uvMin.y));

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(2, 3, 0);
    }
}