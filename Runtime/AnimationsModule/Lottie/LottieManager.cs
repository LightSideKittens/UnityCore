using System;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BaseLottieManager
#if UNITY_EDITOR
    : ISerializationCallbackReceiver
#endif
{
    [ShowInInspector]
    public Texture Texture
    {
        get
        {
            var t = lottie?.Texture;
            return t;
        }
    }

    private bool isAssetDirty;
    [HideInInspector] public BaseLottieAsset asset;
    [ShowInInspector]
    public BaseLottieAsset Asset
    {
        get => asset;
        set
        {
            if (value == asset) return;
            asset = value;

            if (!isAssetDirty)
            {
                isAssetDirty = true;
                SubOnPreRender(ForceUpdateAsset);
            }
        }
    }
    
    [SerializeField] private bool shouldPlay = true;
    [ShowInInspector]
    public bool ShouldPlay
    {
        get => shouldPlay;
        set
        {
            if (value == shouldPlay) return;
            shouldPlay = value;
            UpdatePlayState();
        }
    }
    
    [SerializeField] [HideInInspector] private bool loop = true;
    [ShowInInspector]
    public bool Loop
    {
        get => loop;
        set
        {
            if(loop == value) return;
            if(lottie != null) lottie.loop = value;
            loop = value;
            if (value)
            {
                UpdatePlayState();
            }
        }
    }

    [SerializeField] private float speed = 1f;
    [ShowInInspector]
    public float Speed
    {
        get => speed;
        set
        {
            if(Mathf.Abs(speed - value) < 0.01f) return;
            speed = Mathf.Max(0f, value);
            UpdatePlayState();
        }
    }
    
    internal LottieAnimation lottie;
    
    public abstract void SetupByAsset();
    protected abstract void OnTextureSwapped(Texture tex);
    
    public bool IsEnded =>
        !loop
        && lottie != null
        && lottie.currentFrame >= lottie.TotalFramesCount - 1;
    
    internal bool IsPlaying { get; set; }
    public abstract bool IsVisible { get; }
    
    private uint lastSize;

    [ShowInInspector]
    internal Vector2Int TextureSize
    {
        get
        {
            var t = lottie?.Texture;
            if (t != null)
            {
                return new Vector2Int(t.width, t.height);
            }

            return Vector2Int.zero;
        }
    }
    
    public uint Size
    {
        get
        {
            var newSize = Size_Internal;
            lastSize = newSize;
            return newSize;
        }
    }

    protected abstract uint Size_Internal { get; }
    
    private bool isUpdatePlayStateQueued;
    
    internal void UpdatePlayState()
    {
        if (!isUpdatePlayStateQueued)
        {
            isUpdatePlayStateQueued = true;
            SubOnPreRender(ForceUpdatePlayState);
        }
    }
    
    protected abstract void SubOnPreRender(Action action);
    protected abstract void UnSubOnPreRender(Action action);

    private void ForceUpdateAsset()
    {
        if (isAssetDirty)
        {
            isAssetDirty = false;
            UnSubOnPreRender(ForceUpdateAsset);
        }
        
        DestroyLottie();
        if (asset != null)
        {
            CreateLottie();
            SetupByAsset();
        }
        ForceUpdatePlayState();
    }
    
    protected void ForceUpdatePlayState()
    {
        if (isUpdatePlayStateQueued)
        {
            isUpdatePlayStateQueued = false;
            UnSubOnPreRender(ForceUpdatePlayState);
        }
        
        var lastIsPlaying = IsPlaying;
        var isVisible = IsVisible;
        IsPlaying = isVisible && shouldPlay && asset != null && !IsEnded && speed > 0;
        if (!isVisible && lottie != null)
        { 
            lottie.ReleaseRenderData(lottie.size);
        }
        if (IsPlaying == lastIsPlaying) return;
        
        if (IsPlaying)
        {
            if (lottie == null)
            {
                CreateLottie();
                lottie!.DrawOneFrame(0);
            }
            else
            {
                lottie.SetupRenderData(lottie.size);
            }
            
            LottieUpdater.managers.Add(this);
        }
        else
        {
            LottieUpdater.managers.Remove(this);
        }
    }
    

    protected abstract void Tick();
    
    internal void ResizeIfNeeded()
    {
        if (lottie == null || !IsVisible) return;
        if (lastSize == Size) return;
        lottie.Resize(Size);
    }
    
    private void CreateLottie()
    {
        lottie = new LottieAnimation(asset.Json, string.Empty, Size);
        lottie.OnTextureSwapped += OnTextureSwapped;
        lottie.loop = Loop;
    }
    
    internal void DestroyLottie()
    {
        isAssetDirty = false;
        isUpdatePlayStateQueued = false;
        IsPlaying = false;
        UnSubOnPreRender(ForceUpdatePlayState);
        UnSubOnPreRender(ForceUpdateAsset);
        if (lottie == null) return;
        if (lottie.hasRendererData)
        {
            lottie.ReleaseRenderData(lottie.size);
            LottieUpdater.managers.Remove(this);
        }
        lottie.Destroy();
        lottie = null;
    }
    
    internal void Update()
    {
        Tick();
        lottie.LateUpdateFetch();
        lottie.UpdateDeltaAsync(speed * Time.deltaTime);
        if (IsEnded) UpdatePlayState();
    }
    
#if UNITY_EDITOR
    public void OnBeforeSerialize() { }
    public void OnAfterDeserialize()
    {
        IsPlaying = false;
        isUpdatePlayStateQueued = false;
    }
#endif
}

[Serializable]
public class LottieImageManager : BaseLottieManager
{
    internal LottieImage renderer;

    public override bool IsVisible => renderer.IsVisible;
    protected override uint Size_Internal => renderer.Size;
    protected override void SubOnPreRender(Action action) => World.CanvasPreRendering += action;
    protected override void UnSubOnPreRender(Action action) => World.CanvasPreRendering -= action;
    protected override void Tick() { }
    public override void SetupByAsset() => asset.SetupImage(renderer);
    protected override void OnTextureSwapped(Texture tex) => renderer.OnTextureSwapped(tex);
}

[Serializable]
public class LottieRendererManager : BaseLottieManager
{
    internal LottieRenderer renderer;
    
    public override bool IsVisible => renderer.IsVisible;
    protected override uint Size_Internal => renderer.Size;
    protected override void SubOnPreRender(Action action) => World.PreRendering += action;
    protected override void UnSubOnPreRender(Action action) => World.PreRendering -= action;
    protected override void Tick() => renderer.TryRefresh();
    public override void SetupByAsset() => asset.SetupRenderer(renderer);
    protected override void OnTextureSwapped(Texture tex) => renderer.OnTextureSwapped(tex);
}