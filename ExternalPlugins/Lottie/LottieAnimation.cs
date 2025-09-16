using System;
using System.Runtime.InteropServices;
using LSCore.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public sealed class LottieAnimation : IDisposable
{
    public Texture2D Texture => textures[readIndex];
    public int CurrentFrame { get; set; }
    public double FrameRate => animationWrapper.frameRate;
    public long TotalFramesCount => animationWrapper.totalFrames;
    public double DurationSeconds => animationWrapper.duration;
    public bool IsPlaying { get; private set; } = true;

    public event Action<Texture2D> OnTextureSwapped;

    private IntPtr animationWrapperIntPtr;
    private LottieAnimationWrapper animationWrapper;

    private readonly Texture2D[] textures = new Texture2D[2];
    private readonly NativeArray<byte>[] pixelData = new NativeArray<byte>[2];
    private readonly IntPtr[] renderDataPtr = new IntPtr[2];
    private readonly LottieRenderData[] renderData = new LottieRenderData[2];

    private int readIndex = 0;
    private int writeIndex = 1;

    public float timeSinceLastRenderCall;
    private double frameDelta;
    private bool hasPending;
    private int pendingFrame;

    private readonly Action<int> drawOneFrameSyncCached;
    private readonly Action<int> drawOneFrameAsyncPrepareCached;

    private bool isVisible = true;
    public void SetVisible(bool visible) => isVisible = visible;

    private readonly bool clampOneFramePerTick = true;
    private readonly bool loop = true;
    private readonly bool clearBeforeRender = true;

    private LottieAnimation(string jsonData, string resourcesPath, uint pixelsPerAspectUnit)
    {
        var size = GetWH(jsonData);
        var aspect = (float)size.x / Mathf.Max(1, size.y);

        animationWrapper = NativeBridge.LoadFromData(jsonData, resourcesPath, out animationWrapperIntPtr);
        frameDelta = animationWrapper.duration / Math.Max(1, animationWrapper.totalFrames);

        uint width = (uint)Mathf.Max(1, Mathf.RoundToInt(pixelsPerAspectUnit * aspect));
        uint height = (uint)Mathf.Max(1, Mathf.RoundToInt(pixelsPerAspectUnit / aspect));
        CreateDoubleBufferedRenderTargets(width, height);

        drawOneFrameSyncCached = DrawOneFrameSyncInternal;
        drawOneFrameAsyncPrepareCached = DrawOneFrameAsyncPrepareInternal;
    }

    public static LottieAnimation LoadFromJsonData(string jsonData, string resourcesPath, uint pixelsPerAspectUnit)
    {
        if (string.IsNullOrWhiteSpace(jsonData))
            throw new ArgumentException("The provided json animation file is empty");

        return new LottieAnimation(jsonData, resourcesPath, pixelsPerAspectUnit);
    }

    public static Vector2Int GetWH(string json)
    {
        var wh = JTokenExtensions.FindByPath(json, "w", "h");
        return new Vector2Int(wh[0].ToInt(), wh[1].ToInt());
    }

    public void Play()   { IsPlaying = true; }
    public void Pause()  { IsPlaying = false; }
    public void Resume() { IsPlaying = true; }
    public void Stop()   { Pause(); CurrentFrame = 0; }

    public void TogglePlay() => IsPlaying = !IsPlaying;

    public void Update(float animationSpeed = 1f)
        => UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameSyncCached, synchronous: true);

    public void UpdateDelta(float deltaTime)
        => UpdateInternal(deltaTime, drawOneFrameSyncCached, synchronous: true);

    public void UpdateAsync(float animationSpeed = 1f)
        => UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameAsyncPrepareCached, synchronous: false);

    public void LateUpdateFetch()
    {
        if (!hasPending) return;

        NativeBridge.LottieRenderGetFutureResult(animationWrapperIntPtr, renderDataPtr[writeIndex]);

        textures[writeIndex].Apply();

        SwapReadWrite();
        OnTextureSwapped?.Invoke(textures[readIndex]);

        hasPending = false;
    }

    public void DrawOneFrame(int frameNumber)
    {
        DrawOneFrameSyncInternal(frameNumber);
        textures[writeIndex].Apply();
        SwapReadWrite();
        OnTextureSwapped?.Invoke(textures[readIndex]);
    }

    public void Dispose()
    {
        try
        {
            NativeBridge.Dispose(animationWrapper);
        }
        catch {
        }

        for (int i = 0; i < 2; i++)
        {
            try { NativeBridge.LottieDisposeRenderData(ref renderDataPtr[i]); } catch {
            }
            if (textures[i] != null)
            {
                try { UnityEngine.Object.DestroyImmediate(textures[i]); } catch {
                }
                textures[i] = null;
            }
        }
    }

    private unsafe void CreateDoubleBufferedRenderTargets(uint width, uint height)
    {
        var fmt =TextureFormat.BGRA32;
        for (int i = 0; i < 2; i++)
        {
            renderData[i] = new LottieRenderData
            {
                width = width,
                height = height,
                bytesPerLine = width * sizeof(uint)
            };

            textures[i] = new Texture2D((int)width, (int)height, fmt, 0, false)
            {
                hideFlags = HideFlags.DontSave
            };

            pixelData[i] = textures[i].GetRawTextureData<byte>();
            renderData[i].buffer = pixelData[i].GetUnsafePtr();

            NativeBridge.LottieAllocateRenderData(ref renderDataPtr[i]);
            Marshal.StructureToPtr(renderData[i], renderDataPtr[i], false);
        }

        readIndex = 0;
        writeIndex = 1;
    }

    private void UpdateInternal(float deltaTime, Action<int> drawMethod, bool synchronous)
    {
        if (!IsPlaying || !isVisible) return;

        timeSinceLastRenderCall += Mathf.Max(0, deltaTime);

        if (timeSinceLastRenderCall < frameDelta)
            return;

        int framesDelta = Mathf.FloorToInt((float)(timeSinceLastRenderCall / frameDelta));
        if (clampOneFramePerTick) framesDelta = Mathf.Min(framesDelta, 1);

        int total = Mathf.Max(1, (int)animationWrapper.totalFrames);
        CurrentFrame = (CurrentFrame + framesDelta);

        if (loop)
        {
            if (CurrentFrame >= total) CurrentFrame %= total;
        }
        else
        {
            if (CurrentFrame >= total) { CurrentFrame = total - 1; IsPlaying = false; }
        }

        if (synchronous)
        {
            drawMethod(CurrentFrame);
            textures[writeIndex].Apply(false, false);
            SwapReadWrite();
            OnTextureSwapped?.Invoke(textures[readIndex]);
        }
        else
        {
            if (!hasPending)
            {
                drawMethod(CurrentFrame);
                hasPending = true;
                pendingFrame = CurrentFrame;
            }
        }

        timeSinceLastRenderCall -= (float)(framesDelta * frameDelta);
        if (timeSinceLastRenderCall < 0) timeSinceLastRenderCall = 0;
    }

    private void DrawOneFrameSyncInternal(int frameNumber)
    {
        NativeBridge.LottieRenderImmediately(animationWrapperIntPtr, renderDataPtr[writeIndex], frameNumber, clearBeforeRender);
    }

    private void DrawOneFrameAsyncPrepareInternal(int frameNumber)
    {
        NativeBridge.LottieRenderCreateFutureAsync(animationWrapperIntPtr, renderDataPtr[writeIndex], frameNumber, clearBeforeRender);
    }

    private void SwapReadWrite()
    {
        (readIndex, writeIndex) = (writeIndex, readIndex);
    }
}
