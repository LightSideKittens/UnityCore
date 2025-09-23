using LSCore;
using UnityEngine;

[ExecuteAlways]
public sealed class LottieImage : LSRawImage
{
    public LottieImageManager manager;
    
    public static LottieImage Create(BaseLottieAsset asset, RectTransform transform, float animationSpeed = 1f, bool loop = true)
    {
        var image = new GameObject(nameof(LottieImage)).AddComponent<LottieImage>();
        var manager = new LottieImageManager();
        manager.renderer = image;
        image.manager = manager;
        manager.Asset = asset;
        transform.SetParent(transform, false);
        manager.Speed = animationSpeed;
        manager.Loop = loop;
        return image;
    }
    
    public bool IsVisible => !canvasRenderer.cull && IsActive();
    
    internal uint Size
    {
        get
        {
            var size = rectTransform.rect.size;
            var min = (int)(Mathf.Min(size.x, size.y) * 0.5f);
            min = Mathf.ClosestPowerOfTwo(min);
            var newSize = (uint)Mathf.Clamp(min, 64, 2048);
            return newSize;
        }
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        manager.renderer = this;
    }
#endif
    
    protected override void Awake()
    {
        manager.renderer = this;
        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        manager.UpdatePlayState();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        manager.UpdatePlayState();
    }

    public override void OnCullingChanged()
    {
        base.OnCullingChanged();
        manager.UpdatePlayState();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        manager.DestroyLottie();
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        manager.renderer = this;
        manager.ResizeIfNeeded();
    }

    internal void OnSpriteChanged(Lottie.Sprite sprite)
    {
        OnTextureChanged(sprite.Texture);
    }

    private void OnTextureChanged(Texture texture)
    {
        m_Texture = texture;
        canvasRenderer.SetTexture(m_Texture);
    }
}
