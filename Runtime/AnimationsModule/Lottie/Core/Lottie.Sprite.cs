using UnityEngine;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LSCore;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

public sealed partial class Lottie
{
    private const int Bpp = 4;
    
    public class Sprite
    {
        internal class RenderData
        {
            public LottieRenderData renderData;
            private NativeArray<byte> spriteRaw;
            public unsafe byte* srcPtr;
            public IntPtr ptr;
            public int srcStride;

            private unsafe RenderData(Sprite sprite)
            {
                var size = sprite.size;
                srcStride = size.x * Bpp;
                var byteCount = srcStride * size.y;
                
                spriteRaw = new NativeArray<byte>(
                    byteCount,
                    Allocator.Persistent,
                    NativeArrayOptions.UninitializedMemory);
                
                srcPtr = (byte*)spriteRaw.GetUnsafePtr();
                
                renderData.width = (uint)size.x;
                renderData.height = (uint)size.y;
                renderData.bytesPerLine = (uint)srcStride;
                renderData.buffer = srcPtr;
            }
            
            private static Dictionary<Vector2Int, LSObjectPool<RenderData>> renderDataPools = new();

            public static RenderData Alloc(Sprite sprite)
            {
                var size = sprite.size;
                
                if (!renderDataPools.TryGetValue(size, out var pool))
                {
                    pool = new LSObjectPool<RenderData>(Create, shouldStoreActive: true);
                    renderDataPools.Add(size, pool);
                }
            
                var rData = pool.Get();
                NativeBridge.LottieAllocateRenderData(ref rData.ptr);
                Marshal.StructureToPtr(rData.renderData, rData.ptr, false);
                return rData;

                RenderData Create()
                {
                    var data = new RenderData(sprite);
                    return data;
                }
            }

            public void Release(Sprite sprite)
            {
                var size = sprite.size;
                renderDataPools[size].Release(this);
                NativeBridge.LottieDisposeRenderData(ref ptr);   
            }
        }
        
        public Texture2D Texture => atlas.Texture;
        private Atlas atlas;
        
        private Vector2 uvMin;
        private Vector2 uvMax;
        
        internal Vector2Int size;
        internal Vector2Int pixelStart;

        internal RenderData renderData;
        private bool isSetup;
        
        public Vector2 UvMin => uvMin; 
        public Vector2 UvMax => uvMax;
        
        public Sprite(Atlas atlas, Vector2 uvMin, Vector2 uvMax)
        {
            this.atlas = atlas;
            this.uvMin = uvMin;
            this.uvMax = uvMax;
            OnTextureChanged();
        }

        public void Destroy()
        {
            ReleaseRenderData();
            atlas = null;
        }

        private void OnTextureChanged()
        {
            var tex = atlas.Texture;
            var atlasWidth = tex.width;
            var atlasHeight = tex.height;

            float u0 = uvMin.x, v0 = uvMin.y;
            float u1 = uvMax.x, v1 = uvMax.y;

            var x = Mathf.FloorToInt(u0 * atlasWidth);
            var y = Mathf.FloorToInt(v0 * atlasHeight);
            var w = Mathf.CeilToInt((u1 - u0) * atlasWidth);
            var h = Mathf.CeilToInt((v1 - v0) * atlasHeight);

            x = Mathf.Clamp(x, 0, atlasWidth - 1);
            y = Mathf.Clamp(y, 0, atlasHeight - 1);
            pixelStart = new Vector2Int(x, y);
            
            var width = Mathf.Clamp(w, 1, atlasWidth - x);
            var height = Mathf.Clamp(h, 1, atlasHeight - y);
            size = new Vector2Int(width, height);
            
            ReleaseRenderData();
            SetupRenderData();
        }

        private void SetupRenderData()
        {
            if (isSetup) return;
            isSetup = true;
            renderData = RenderData.Alloc(this);
        }

        private void ReleaseRenderData()
        {
            if (!isSetup) return;
            isSetup = false;
            CompletePending();
            renderData.Release(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DrawOneFrameSyncInternal(IntPtr animationWrapperIntPtr, int frameNumber)
        {
            NativeBridge.LottieRenderImmediately(animationWrapperIntPtr, renderData.ptr, frameNumber, false);
            animationWrapperPtr = animationWrapperIntPtr;
            atlas.AddToBlit(this);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void DrawOneFrameAsyncPrepareInternal(IntPtr animationWrapperIntPtr, int frameNumber)
        {
            if(hasPending) return;
            hasPending = true;
            animationWrapperPtr = animationWrapperIntPtr;
            NativeBridge.LottieRenderCreateFutureAsync(animationWrapperIntPtr, renderData.ptr, frameNumber, false);
        }

        public void FetchTexture()
        {
            if (!hasPending) return;

            NativeBridge.LottieRenderGetFutureResult(animationWrapperPtr, renderData.ptr);
            atlas.AddToBlit(this);

            hasPending = false;
        }
        
        private bool hasPending;
        private IntPtr animationWrapperPtr;
        
        private void CompletePending()
        {
            if (hasPending)
            {
                NativeBridge.LottieRenderGetFutureResult(animationWrapperPtr, renderData.ptr);
                hasPending = false;
            }
        }
    }
}