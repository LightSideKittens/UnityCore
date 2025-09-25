using System;
using System.Runtime.CompilerServices;
using LSCore;
using LSCore.Extensions;
using UnityEngine;


public sealed partial class Lottie
{
    private static LSList<Lottie> toPack = new();
    private static bool toPackIsDirty;

    private static bool ToPackIsDirty
    {
        get => toPackIsDirty;
        set
        {
            if(toPackIsDirty == value) return;
            toPackIsDirty = value;
            if (value)
            {
                LottieUpdater.PreRendering[1] += Pack;
                LottieUpdater.CanvasPreRendering[1] += Pack;
            }
            else
            {
                LottieUpdater.PreRendering[1] -= Pack;
                LottieUpdater.CanvasPreRendering[1] -= Pack;
            }
        }
    }
    
    private static void Pack()
    {
        ToPackIsDirty = false;
        Atlas.Pack(toPack);
    }
    
    public static Vector2Int GetSize(string json)
    {
        var wh = JTokenExtensions.FindByPath(json, "w", "h");
        return new Vector2Int(wh[0].ToInt(), wh[1].ToInt());
    }
    
    public double FrameRate => animationWrapper.frameRate;
    public long TotalFramesCount => animationWrapper.totalFrames;
    public double DurationSeconds => animationWrapper.duration;

    private Sprite sprite;
    public Sprite Spritee
    {
        get => sprite;
        set
        {
            if(sprite == value) return;
            sprite?.Destroy();
            sprite = value;
            SpriteChanged?.Invoke(sprite);
        }
    }

    public event Action<Sprite> SpriteChanged;

    public int currentFrame;
    public bool loop = true;

    private IntPtr animationWrapperIntPtr;
    private LottieAnimationWrapper animationWrapper;
    
    internal Vector2Int size;
    private float timeSinceLastRenderCall;
    private float frameDelta;
    public bool hasRendererData;
    private float aspect;
    private readonly bool canPack;
    
    private readonly Action<int> drawOneFrameSyncCached;
    private readonly Action<int> drawOneFrameAsyncPrepareCached;
    
    public Lottie(string jsonData, string resourcesPath, uint pixelsPerAspectUnit, bool canPack)
    {
        this.canPack = canPack;
        var s = GetSize(jsonData);
        aspect = (float)s.x / s.y;
        SetupSize(pixelsPerAspectUnit);
        
        animationWrapper = NativeBridge.LoadFromData(jsonData, resourcesPath, out animationWrapperIntPtr);
        frameDelta = (float)animationWrapper.duration / animationWrapper.totalFrames;

        AllowToRender();
        drawOneFrameSyncCached = DrawOneFrameSyncInternal;
        drawOneFrameAsyncPrepareCached = DrawOneFrameAsyncPrepareInternal;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Update(float animationSpeed = 1f) => UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameSyncCached);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateAsync(float animationSpeed = 1f) => UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameAsyncPrepareCached);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateDelta(float deltaTime) => UpdateInternal(deltaTime, drawOneFrameSyncCached);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void UpdateDeltaAsync(float deltaTime) => UpdateInternal(deltaTime, drawOneFrameAsyncPrepareCached);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void FetchTexture() => sprite.FetchTexture();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void DrawOneFrame(int frameNumber)
    {
        currentFrame = frameNumber;
        DrawCurrentFrame(drawOneFrameSyncCached);
    }

    private void UpdateInternal(float deltaTime, Action<int> drawMethod)
    {
        timeSinceLastRenderCall += deltaTime;

        if (timeSinceLastRenderCall < frameDelta)
            return;

        int framesDelta = Mathf.FloorToInt(timeSinceLastRenderCall / frameDelta);
        currentFrame += framesDelta;
        
        DrawCurrentFrame(drawMethod);

        timeSinceLastRenderCall -= framesDelta * frameDelta;
        if (timeSinceLastRenderCall < 0) timeSinceLastRenderCall = 0;
    }

    private void DrawCurrentFrame(Action<int> drawMethod)
    {
        int total = (int)animationWrapper.totalFrames;
        if (loop)
        {
            if (currentFrame >= total) currentFrame %= total;
        }
        else
        {
            if (currentFrame >= total) currentFrame = total - 1;
        }
        
        drawMethod(currentFrame);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DrawOneFrameSyncInternal(int frameNumber)
    {
        sprite.DrawOneFrameSyncInternal(animationWrapperIntPtr, frameNumber);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DrawOneFrameAsyncPrepareInternal(int frameNumber)
    {
        sprite.DrawOneFrameAsyncPrepareInternal(animationWrapperIntPtr, frameNumber);
    }
    
    private void SetupSize(uint pixelsPerAspectUnit)
    {
        var width = (int)(pixelsPerAspectUnit * aspect);
        var height = (int)(pixelsPerAspectUnit / aspect);
        size = new Vector2Int(width, height);
    }
    
    public void Resize(uint pixelsPerAspectUnit)
    {
        var lastSize = size;
        SetupSize(pixelsPerAspectUnit);
        if(lastSize == size) return;
        
        if (canPack)
        {
            ToPackIsDirty = true;
        }
        else
        {
            Spritee = new Sprite(Atlas.Get(size.x), Vector2.zero, Vector2.one);
        }
    }
    
    public void AllowToRender()
    {
        if(hasRendererData) return;
        hasRendererData = true;

        if (canPack)
        {
            toPack.Add(this);
            ToPackIsDirty = true;
        }
        else
        {
            Spritee = new Sprite(Atlas.Get(size.x), Vector2.zero, Vector2.one);
        }
    }

    public void DisallowToRender()
    {
        if(!hasRendererData) return;
        hasRendererData = false;
        
        sprite.Destroy();
        sprite = null;
        
        if (canPack)
        {
            toPack.Remove(this);
            ToPackIsDirty = true;
        }
    }
    
    public void Destroy()
    {
        DisallowToRender();
        NativeBridge.Dispose(animationWrapper);
    }
}
