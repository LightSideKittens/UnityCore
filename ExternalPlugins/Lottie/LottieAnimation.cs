using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LSCore.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


public sealed class LottieAnimation : IDisposable
{
    public Texture2D Texture { get; private set; }
    public int CurrentFrame { get; private set; }
    public double FrameRate => animationWrapper.frameRate;
    public long TotalFramesCount => animationWrapper.totalFrames;
    public double DurationSeconds => animationWrapper.duration;
    public bool IsPlaying { get; private set; }

    private IntPtr animationWrapperIntPtr;
    private LottieAnimationWrapper animationWrapper;

    private IntPtr lottieRenderDataIntPtr;
    private LottieRenderData lottieRenderData;
    private NativeArray<byte> pixelData;
    public float timeSinceLastRenderCall;
    private double frameDelta;
    private bool asyncDrawWasCalled;

    private Action<int> drawOneFrameCached;
    private Action<int> drawOneFrameAsyncPrepareCached;

    private LottieAnimation(string jsonData, string resourcesPath, uint pixelsPerAspectUnit)
    {
        var size = GetWH(jsonData);
        var aspect = size.x / size.y;
        animationWrapper = NativeBridge.LoadFromData(jsonData, resourcesPath, out animationWrapperIntPtr);
        frameDelta = animationWrapper.duration / animationWrapper.totalFrames;
        CreateRenderDataTexture2DMarshalToNative((uint)(pixelsPerAspectUnit * aspect), (uint)(pixelsPerAspectUnit / aspect));
        IsPlaying = true;
        drawOneFrameCached = DrawOneFrame;
        drawOneFrameAsyncPrepareCached = DrawOneFrameAsyncPrepare;
    }

    public void Dispose()
    {
        NativeBridge.Dispose(animationWrapper);
        NativeBridge.LottieDisposeRenderData(ref lottieRenderDataIntPtr);
        UnityEngine.Object.DestroyImmediate(Texture);
        Texture = null;
    }

    public void Update(float animationSpeed = 1f)
    {
        UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameCached);
    }
    
    public void UpdateDelta(float deltaTime)
    {
        UpdateInternal(deltaTime, drawOneFrameCached);
    }

    public void UpdateAsync(float animationSpeed = 1f)
    {
        UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameAsyncPrepareCached);
    }

    public void TogglePlay()
    {
        IsPlaying = !IsPlaying;
    }

    public void Play()
    {
        IsPlaying = true;
        DrawOneFrame(++CurrentFrame);
    }

    public void Pause()
    {
        IsPlaying = false;
    }
    
    public void Resume()
    {
        IsPlaying = true;
    }

    public void Stop()
    {
        Pause();
        CurrentFrame = 0;
    }

    public void DrawOneFrame(int frameNumber)
    {
        NativeBridge.LottieRenderImmediately(animationWrapperIntPtr, lottieRenderDataIntPtr, frameNumber, true);
        CurrentFrame = frameNumber;
        Texture.Apply();
    }

    public void DrawOneFrameAsyncPrepare(int frameNumber)
    {
        NativeBridge.LottieRenderCreateFutureAsync(animationWrapperIntPtr, lottieRenderDataIntPtr, frameNumber, true);
    }

    public void DrawOneFrameAsyncGetResult()
    {
        if (asyncDrawWasCalled)
        {
            NativeBridge.LottieRenderGetFutureResult(animationWrapperIntPtr, lottieRenderDataIntPtr);
            Texture.Apply();
            asyncDrawWasCalled = false;
        }
    }

    private unsafe void CreateRenderDataTexture2DMarshalToNative(uint width, uint height)
    {
        lottieRenderData = new LottieRenderData();
        lottieRenderData.width = width;
        lottieRenderData.height = height;
        lottieRenderData.bytesPerLine = width * sizeof(uint);
        Texture = new Texture2D(
            (int)lottieRenderData.width,
            (int)lottieRenderData.height,
            TextureFormat.BGRA32,
            0,
            false);
        Texture.hideFlags = HideFlags.DontSave;
        pixelData = Texture.GetRawTextureData<byte>();
        lottieRenderData.buffer = pixelData.GetUnsafePtr();
        NativeBridge.LottieAllocateRenderData(ref lottieRenderDataIntPtr);
        Marshal.StructureToPtr(lottieRenderData, lottieRenderDataIntPtr, false);
    }

    private void UpdateInternal(float deltaTime, Action<int> drawOneFrameMethod)
    {
        if (IsPlaying)
        {
            timeSinceLastRenderCall += deltaTime;
        }

        if (timeSinceLastRenderCall >= frameDelta)
        {
            int framesDelta = Mathf.RoundToInt(timeSinceLastRenderCall / (float)frameDelta);
            CurrentFrame += framesDelta;
            if (CurrentFrame >= animationWrapper.totalFrames)
            {
                CurrentFrame = 0;
            }

            drawOneFrameMethod(CurrentFrame);
            asyncDrawWasCalled = true;
            timeSinceLastRenderCall = 0;
        }
    }
    
    public static LottieAnimation LoadFromJsonData(string jsonData, string resourcesPath, uint pixelsPerAspectUnit)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
        {
            throw new System.ArgumentException($"The provided json animation file is empty");
        }

        return new LottieAnimation(jsonData, resourcesPath, pixelsPerAspectUnit);
    }
    
    public static Vector2Int GetWH(string json)
    {
        var wh = JTokenExtensions.FindByPath(json, "w", "h");
        return new Vector2Int(wh[0].ToInt(), wh[1].ToInt());
    }
    
}

