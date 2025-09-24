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
            var t = lottie?.Spritee?.Texture;
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
                SubOnPreRender(ForceUpdateAsset, 0);
            }
        }
    }
    
    [SerializeField] [HideInInspector] private bool shouldPlay = true;
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

    [SerializeField] [HideInInspector] private float speed = 1f;
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
    
    internal Lottie lottie;
    
    public abstract void SetupByAsset();
    protected abstract void OnSpriteChanged(Lottie.Sprite sprite);
    
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
            var t = Texture;
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
            SubOnPreRender(ForceUpdatePlayState, 0);
        }
    }
    
    protected abstract void SubOnPreRender(Action action, int order);
    protected abstract void UnSubOnPreRender(Action action, int order);

    private void ForceUpdateAsset()
    {
        if (isAssetDirty)
        {
            isAssetDirty = false;
            UnSubOnPreRender(ForceUpdateAsset, 0);
        }
        
        DestroyLottie();
        ForceUpdatePlayState();
    }
    
    protected void ForceUpdatePlayState()
    {
        if (isUpdatePlayStateQueued)
        {
            isUpdatePlayStateQueued = false;
            UnSubOnPreRender(ForceUpdatePlayState, 0);
        }
        
        var lastIsPlaying = IsPlaying;
        var isVisible = IsVisible;
        IsPlaying = isVisible && shouldPlay && asset != null && !IsEnded && speed > 0;
        
        if (!isVisible && lottie != null)
        { 
            lottie.DisallowToRender();
        }
        
        if (IsPlaying == lastIsPlaying) return;
        
        if (IsPlaying)
        {
            if (lottie == null)
            {
                CreateLottie();
                SubOnPreRender(Draw, 1);

                void Draw()
                {
                    UnSubOnPreRender(Draw, 1);
                    lottie.DrawOneFrame(0);
                    SubOnPreRender(UpdateAsync, 3);

                    void UpdateAsync()
                    {
                        UnSubOnPreRender(UpdateAsync, 3);
                        lottie.UpdateDeltaAsync(speed * Time.deltaTime);
                    }
                }
            }
            else
            {
                lottie.AllowToRender();
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
        lottie = new Lottie(asset.Json, string.Empty, Size, true);
        lottie.SpriteChanged += OnSpriteChanged;
        lottie.loop = Loop;
        SetupByAsset();
    }
    
    internal void DestroyLottie()
    {
        isAssetDirty = false;
        isUpdatePlayStateQueued = false;
        IsPlaying = false;
        UnSubOnPreRender(ForceUpdatePlayState, 0);
        UnSubOnPreRender(ForceUpdateAsset, 0);
        if (lottie == null) return;
        lottie.DisallowToRender();
        LottieUpdater.managers.Remove(this);
        lottie.Destroy();
        lottie = null;
    }

    internal void PreUpdate()
    {
        Tick();
        lottie.FetchTexture();
    }
    
    internal void Update()
    {
        lottie.UpdateDeltaAsync(speed * World.DeltaTime);
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
    protected override void SubOnPreRender(Action action, int order) => LottieUpdater.CanvasPreRendering[order] += action;
    protected override void UnSubOnPreRender(Action action, int order) => LottieUpdater.CanvasPreRendering[order] -= action;

    protected override void Tick() { }
    public override void SetupByAsset() => asset.SetupImage(renderer);
    protected override void OnSpriteChanged(Lottie.Sprite sprite) => renderer.OnSpriteChanged(sprite);
}

[Serializable]
public class LottieRendererManager : BaseLottieManager
{
    internal LottieRenderer renderer;
    
    public override bool IsVisible => renderer.IsVisible;
    protected override uint Size_Internal => renderer.Size;
    protected override void SubOnPreRender(Action action, int order) => LottieUpdater.PreRendering[order] += action;
    protected override void UnSubOnPreRender(Action action, int order) => LottieUpdater.PreRendering[order] -= action;

    protected override void Tick() => renderer.TryRefresh();
    public override void SetupByAsset() => asset.SetupRenderer(renderer);
    protected override void OnSpriteChanged(Lottie.Sprite sprite) => renderer.OnSpriteChanged(sprite);
}