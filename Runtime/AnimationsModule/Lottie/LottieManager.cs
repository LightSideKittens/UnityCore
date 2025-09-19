using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BaseLottieManager
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
    
    [HideInInspector] public BaseLottieAsset asset;
    [ShowInInspector]
    public BaseLottieAsset Asset
    {
        get => asset;
        set
        {
            if (value == asset) return;
            asset = value;
            DestroyLottie();
            CreateLottie();
            if (asset != null)
            {
                SetupByAsset();
            }
            UpdatePlayState();
        }
    }
    
    [SerializeField] private bool isEnabled = true;
    [ShowInInspector]
    public bool Enabled
    {
        get => isEnabled;
        set
        {
            if (value == isEnabled) return;
            isEnabled = value;
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
    
    public bool IsEnded => !loop && lottie.currentFrame >= lottie.TotalFramesCount - 1;
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
            if (newSize > 512)
            {
                newSize /= sizeModifier;
            }
            lastSize = newSize;
            return newSize;
        }
    }

    protected abstract uint Size_Internal { get; }

    internal void UpdatePlayState()
    {
        var lastIsPlaying = IsPlaying;
        IsPlaying = IsVisible && isEnabled && asset != null && !IsEnded && speed > 0;
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
            lottie.ReleaseRenderData(lottie.size);
            LottieUpdater.managers.Remove(this);
        }
    }

    protected abstract void Tick();
    
    internal void ResizeIfNeeded()
    {
        if (lastSize == Size) return; 
        if (lottie == null || !IsVisible) return;
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
        if (lottie != null)
        {
            lottie.Destroy();
            lottie = null;
        }
    }


    private float time;
    private float lastDrawTime;
    private int frameCount;
    
    internal void Update()
    {
        time += Time.deltaTime;
        if (frameCount < framesToSkip)
        {
            frameCount++;
            return;
        }
        
        frameCount = 0;
        
        Tick();
        lottie.LateUpdateFetch();
        lottie.UpdateDeltaAsync(speed * time - lastDrawTime);
        if (IsEnded) UpdatePlayState();
        lastDrawTime = time;
    }
    
    internal int framesToSkip;
    private uint sizeModifier = 1;
    
    public void ApplySettings()
    {
        framesToSkip = LottieUpdater.framesToSkip;
        sizeModifier = LottieUpdater.sizeDivider;
    }
}

[Serializable]
public class LottieImageManager : BaseLottieManager
{
    internal LottieImage renderer;

    public override bool IsVisible => renderer.IsVisible;
    protected override uint Size_Internal => renderer.Size;
    protected override void Tick() { }

    public override void SetupByAsset()
    {
        asset.SetupImage(renderer);
    }

    protected override void OnTextureSwapped(Texture tex)
    {
        renderer.OnTextureSwapped(tex);
    }
}

[Serializable]
public class LottieRendererManager : BaseLottieManager
{
    internal LottieRenderer renderer;
    
    public override bool IsVisible => renderer.IsVisible;
    protected override uint Size_Internal => renderer.Size;
    protected override void Tick()
    {
        renderer.TryRefresh();
    }

    public override void SetupByAsset()
    {
        asset.SetupRenderer(renderer);
    }

    protected override void OnTextureSwapped(Texture tex)
    {
        renderer.OnTextureSwapped(tex);
    }
}