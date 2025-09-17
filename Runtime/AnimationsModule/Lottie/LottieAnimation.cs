using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LSCore;
using LSCore.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Random = UnityEngine.Random;

public sealed class LottieAnimation
{
    public Texture2D Texture => textures[readIndex];
    public int currentFrame;
    public bool loop = true;
    
    public double FrameRate => animationWrapper.frameRate;
    public long TotalFramesCount => animationWrapper.totalFrames;
    public double DurationSeconds => animationWrapper.duration;
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

    private readonly Action<int> drawOneFrameSyncCached;
    private readonly Action<int> drawOneFrameAsyncPrepareCached;

    public LottieAnimation(string jsonData, string resourcesPath, uint pixelsPerAspectUnit)
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

    public static Vector2Int GetWH(string json)
    {
        var wh = JTokenExtensions.FindByPath(json, "w", "h");
        return new Vector2Int(wh[0].ToInt(), wh[1].ToInt());
    }

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

        textures[writeIndex].Apply(false, false);
        
        SwapReadWrite();
        OnTextureSwapped?.Invoke(textures[readIndex]);

        hasPending = false;
    }

    public void DrawOneFrame(int frameNumber)
    {
        DrawOneFrameSyncInternal(frameNumber);
        textures[writeIndex].Apply(false, false);
        SwapReadWrite();
        OnTextureSwapped?.Invoke(textures[readIndex]);
    }

    public void CompletePending()
    {
        if (hasPending)
        {
            NativeBridge.LottieRenderGetFutureResult(animationWrapperIntPtr, renderDataPtr[writeIndex]);
            hasPending = false;
        }
    }
    
    public async void Destroy()
    {
        await Task.Delay(Random.Range(1000, 3000));
        CompletePending();
        NativeBridge.Dispose(animationWrapper);
        
        for (int i = 0; i < 2; i++)
        {
            NativeBridge.LottieDisposeRenderData(ref renderDataPtr[i]);
#if UNITY_EDITOR
            if(World.IsEditMode) UnityEngine.Object.DestroyImmediate(textures[i]);
            else UnityEngine.Object.Destroy(textures[i]);
#else 
            UnityEngine.Object.Destroy(textures[i]);
#endif
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

            var texture = new Texture2D((int)width, (int)height, fmt, 1, false)
            {
                hideFlags = HideFlags.DontSave
            };
            texture.wrapMode = TextureWrapMode.Clamp;
            textures[i] = texture;
            pixelData[i] = texture.GetRawTextureData<byte>();
            renderData[i].buffer = pixelData[i].GetUnsafePtr();

            NativeBridge.LottieAllocateRenderData(ref renderDataPtr[i]);
            Marshal.StructureToPtr(renderData[i], renderDataPtr[i], false);
        }

        readIndex = 0;
        writeIndex = 1;
    }

    private void UpdateInternal(float deltaTime, Action<int> drawMethod, bool synchronous)
    {
        timeSinceLastRenderCall += Mathf.Max(0, deltaTime);

        if (timeSinceLastRenderCall < frameDelta)
            return;

        int framesDelta = Mathf.FloorToInt((float)(timeSinceLastRenderCall / frameDelta));
        int total = Mathf.Max(1, (int)animationWrapper.totalFrames);
        currentFrame = (currentFrame + framesDelta);

        if (loop)
        {
            if (currentFrame >= total) currentFrame %= total;
        }
        else
        {
            if (currentFrame >= total) currentFrame = total - 1;
        }

        if (synchronous)
        {
            drawMethod(currentFrame);
            textures[writeIndex].Apply(false, false);
            SwapReadWrite();
            OnTextureSwapped?.Invoke(textures[readIndex]);
        }
        else
        {
            if (!hasPending)
            {
                drawMethod(currentFrame);
                hasPending = true;
            }
        }

        timeSinceLastRenderCall -= (float)(framesDelta * frameDelta);
        if (timeSinceLastRenderCall < 0) timeSinceLastRenderCall = 0;
    }

    private void DrawOneFrameSyncInternal(int frameNumber)
    {
        NativeBridge.LottieRenderImmediately(animationWrapperIntPtr, renderDataPtr[writeIndex], frameNumber, false);
    }

    private void DrawOneFrameAsyncPrepareInternal(int frameNumber)
    {
        NativeBridge.LottieRenderCreateFutureAsync(animationWrapperIntPtr, renderDataPtr[writeIndex], frameNumber, false);
    }

    private void SwapReadWrite()
    {
        (readIndex, writeIndex) = (writeIndex, readIndex);
    }
}
