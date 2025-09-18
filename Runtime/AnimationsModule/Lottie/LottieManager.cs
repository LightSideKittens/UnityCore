using System;
using Sirenix.OdinInspector;
using UnityEngine;

[Serializable]
public abstract class BaseLottieManager
{
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
    
    private LottieAnimation lottie;
    
    public abstract void SetupByAsset();
    protected abstract void OnTextureSwapped(Texture tex);
    
    public bool IsEnded => !loop && lottie.currentFrame >= lottie.TotalFramesCount - 1;
    internal bool IsPlaying { get; set; }
    public abstract bool IsVisible { get; }
    
    private uint lastSize;

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

            LottieUpdater.Updated += Update;
        }
        else
        {
            LottieUpdater.Updated -= Update;
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
    private int skippedFrames;
    private static int skipFrames = 0;

    private bool ShouldSkipFrame
    {
        get
        {
            time += Time.deltaTime;
            var sdt = Time.smoothDeltaTime;
        
            /*if (sdt > 0.01f)
            {
                skipFrames = 1;
            }
            else if(sdt > 0.02f)
            {
                skipFrames = 2;
            }*/
        
            if (skippedFrames < skipFrames)
            {
                skippedFrames++;
                return true;
            }

            skippedFrames = 0;
            return false;
        }
    }
    
    private void Update()
    {
        if (ShouldSkipFrame) return;
        Tick();
        lottie.LateUpdateFetch();
        lottie.UpdateDeltaAsync(speed * time - lastDrawTime);
        if (IsEnded) UpdatePlayState();
        lastDrawTime = time;
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