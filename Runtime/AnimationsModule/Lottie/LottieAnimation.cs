using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using LSCore;
using LSCore.Extensions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Random = UnityEngine.Random;

public sealed class LottieAnimation
{
    internal class RenderData
    {
        public LottieRenderData renderData;
        public IntPtr renderDataPtr;
        public Texture2D texture;
    }
    
    public Texture2D Texture => renderData[readIndex].texture;
    public int currentFrame;
    public bool loop = true;
    
    public double FrameRate => animationWrapper.frameRate;
    public long TotalFramesCount => animationWrapper.totalFrames;
    public double DurationSeconds => animationWrapper.duration;
    public event Action<Texture2D> OnTextureSwapped;

    private IntPtr animationWrapperIntPtr;
    private LottieAnimationWrapper animationWrapper;
    
    private static Dictionary<Vector2Int, LSObjectPool<RenderData>> renderDataPools = new();
    private readonly RenderData[] renderData = new RenderData[2];
    
    private int readIndex = 0;
    private int writeIndex = 1;
    private Vector2Int size;
    
    public float timeSinceLastRenderCall;
    private float frameDelta;
    private bool hasPending;
    private float aspect;
    
    private readonly Action<int> drawOneFrameSyncCached;
    private readonly Action<int> drawOneFrameAsyncPrepareCached;

    public LottieAnimation(string jsonData, string resourcesPath, uint pixelsPerAspectUnit)
    {
        var s = GetSize(jsonData);
        aspect = (float)s.x / s.y;
        SetupSize(pixelsPerAspectUnit);
        
        animationWrapper = NativeBridge.LoadFromData(jsonData, resourcesPath, out animationWrapperIntPtr);
        frameDelta = (float)animationWrapper.duration / animationWrapper.totalFrames;

        CreateDoubleBufferedRenderTargets(); 
        drawOneFrameSyncCached = DrawOneFrameSyncInternal;
        drawOneFrameAsyncPrepareCached = DrawOneFrameAsyncPrepareInternal;
    }

    private void SetupSize(uint pixelsPerAspectUnit)
    {
        var width = (int)(pixelsPerAspectUnit * aspect);
        var height = (int)(pixelsPerAspectUnit / aspect);
        size = new Vector2Int(width, height);
    }

    public static Vector2Int GetSize(string json)
    {
        var wh = JTokenExtensions.FindByPath(json, "w", "h");
        return new Vector2Int(wh[0].ToInt(), wh[1].ToInt());
    }

    public void Update(float animationSpeed = 1f)
        => UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameSyncCached, synchronous: true);

    public void UpdateAsync(float animationSpeed = 1f)
        => UpdateInternal(animationSpeed * Time.deltaTime, drawOneFrameAsyncPrepareCached, synchronous: false);
    
    public void UpdateDelta(float deltaTime)
        => UpdateInternal(deltaTime, drawOneFrameSyncCached, synchronous: true);
    
    public void UpdateDeltaAsync(float deltaTime)
        => UpdateInternal(deltaTime, drawOneFrameAsyncPrepareCached, synchronous: false);

    public void LateUpdateFetch()
    {
        if (!hasPending) return;

        NativeBridge.LottieRenderGetFutureResult(animationWrapperIntPtr, renderData[writeIndex].renderDataPtr);
        ApplyTexture();
        SwapReadWrite();
        OnTextureSwapped?.Invoke(renderData[readIndex].texture);

        hasPending = false;
    }

    public void DrawOneFrame(int frameNumber)
    {
        currentFrame = frameNumber;
        DrawCurrentFrame(drawOneFrameSyncCached, true);
    }

    private void CompletePending()
    {
        if (hasPending)
        {
            NativeBridge.LottieRenderGetFutureResult(animationWrapperIntPtr, renderData[writeIndex].renderDataPtr);
            hasPending = false;
        }
    }
    
    public void Destroy()
    {
        CompletePending();
        NativeBridge.Dispose(animationWrapper);

        var pool = renderDataPools[size];
        for (int i = 0; i < 2; i++)
        {
            NativeBridge.LottieDisposeRenderData(ref renderData[i].renderDataPtr);
            pool.Release(renderData);
        }
    }
    
    public void Resize(uint pixelsPerAspectUnit)
    {
        var lastSize = size;
        SetupSize(pixelsPerAspectUnit);
        if(lastSize == size) return;
        
        CompletePending();

        var pool = renderDataPools[lastSize];
        for (int i = 0; i < 2; i++)
        {
            NativeBridge.LottieDisposeRenderData(ref renderData[i].renderDataPtr);
            pool.Release(renderData);
        }
        
        CreateDoubleBufferedRenderTargets();
    }
    
    private static unsafe RenderData CreateTexture(Vector2Int size)
    {
        var data = new RenderData();
        const GraphicsFormat fmt = GraphicsFormat.B8G8R8A8_SRGB;
        const TextureCreationFlags flags = TextureCreationFlags.DontInitializePixels | TextureCreationFlags.DontUploadUponCreate;
        var texture = new Texture2D(size.x, size.y, fmt, flags)
        {
            hideFlags = HideFlags.DontSave,
            wrapMode = TextureWrapMode.Clamp
        };
        
        data.renderData = new LottieRenderData
        {
            width = (uint)size.x,
            height = (uint)size.y,
            bytesPerLine = (uint)size.x * sizeof(uint)
        };
        
        data.texture = texture;
        data.renderData.buffer = texture.GetRawTextureData<byte>().GetUnsafePtr();
        return data;
    }

    private void CreateDoubleBufferedRenderTargets()
    {
        if (!renderDataPools.TryGetValue(size, out var pool))
        {
            var s = size;
            pool = new LSObjectPool<RenderData>(() => CreateTexture(s));
            renderDataPools.Add(size, pool);
        }
        
        for (int i = 0; i < 2; i++)
        {
            var data = pool.Get();
            renderData[i] = data;
            
            NativeBridge.LottieAllocateRenderData(ref data.renderDataPtr);
            Marshal.StructureToPtr(renderData[i].renderData, data.renderDataPtr, false);
        }

        readIndex = 0;
        writeIndex = 1;
    }

    private void UpdateInternal(float deltaTime, Action<int> drawMethod, bool synchronous)
    {
        timeSinceLastRenderCall += deltaTime;

        if (timeSinceLastRenderCall < frameDelta)
            return;

        int framesDelta = Mathf.FloorToInt(timeSinceLastRenderCall / frameDelta);
        currentFrame += framesDelta;
        
        DrawCurrentFrame(drawMethod, synchronous);

        timeSinceLastRenderCall -= framesDelta * frameDelta;
        if (timeSinceLastRenderCall < 0) timeSinceLastRenderCall = 0;
    }

    private void DrawCurrentFrame(Action<int> drawMethod, bool synchronous)
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
        
        if (synchronous)
        {
            drawMethod(currentFrame);
            ApplyTexture();
            SwapReadWrite();
            OnTextureSwapped?.Invoke(renderData[readIndex].texture);
        }
        else
        {
            if (!hasPending)
            {
                drawMethod(currentFrame);
                hasPending = true;
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DrawOneFrameSyncInternal(int frameNumber)
    {
        NativeBridge.LottieRenderImmediately(animationWrapperIntPtr, renderData[writeIndex].renderDataPtr, frameNumber, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DrawOneFrameAsyncPrepareInternal(int frameNumber)
    {
        NativeBridge.LottieRenderCreateFutureAsync(animationWrapperIntPtr, renderData[writeIndex].renderDataPtr, frameNumber, false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SwapReadWrite()
    {
        (readIndex, writeIndex) = (writeIndex, readIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ApplyTexture()
    {
        renderData[writeIndex].texture.Apply(false, false);
    }
}
