﻿using System.Collections.Generic;
using LSCore;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor.Compilation;
#endif

public sealed partial class Lottie
{
    public class Atlas
    {

#if UNITY_EDITOR
        static Atlas()
        {
            CompilationPipeline.compilationFinished += x => { DestroyAllTextures(true); };

            World.Created += () => DestroyAllTextures(false);

            void DestroyAllTextures(bool immediate)
            {
                foreach (var renderDataPool in atlasPools)
                {
                    var pool = renderDataPool.Value;
                    pool.Removed += OnRemoved;
                    pool.Clear();
                    pool.Removed -= OnRemoved;
                }

                void OnRemoved(Atlas atlas)
                {
                    atlas.Destroy(immediate);
                }
            }
        }
#endif

        private static Dictionary<Vector2Int, LSObjectPool<Atlas>> atlasPools = new();

        public static Atlas Get(int size)
        {
            var s = new Vector2Int(size, size);

            if (!atlasPools.TryGetValue(s, out var pool))
            {
                pool = new LSObjectPool<Atlas>(Create, shouldStoreActive: true);
                atlasPools.Add(s, pool);
            }

            return pool.Get();

            Atlas Create()
            {
                var atlas = new Atlas(s);
                return atlas;
            }
        }

        public unsafe struct SpriteCopy
        {
            // Разрешаем хранить «сырые» указатели в job-данных
            [NativeDisableUnsafePtrRestriction] public byte* SrcPtr;
            // Пишем в одну общую целевую память с непересекающимися регионами
            [NativeDisableUnsafePtrRestriction] public byte* AtlasPtr;
            public int AtlasStride; // atlasWidth * Bpp
            public int SrcStride;
            public int WidthBytes;   // s.x * Bpp
            public int Height;       // s.y
            public int DstOffset;    // смещение в байтах от начала атласа
        }

        [BurstCompile]
        public unsafe struct CopySpritesJob : IJobFor
        {
            [ReadOnly] public NativeArray<SpriteCopy> Sprites;

            public void Execute(int i)
            {
                var sc = Sprites[i];

                byte* dstBase = sc.AtlasPtr;
                byte* dst     = dstBase + sc.DstOffset;

                UnsafeUtility.MemCpyStride(
                    destination:      dst,
                    destinationStride:sc.AtlasStride,
                    source:           sc.SrcPtr,
                    sourceStride:     sc.SrcStride,
                    elementSize:      sc.WidthBytes,
                    count:            sc.Height);
            }
        }


        public static unsafe void CopyAll()
        {
            int spriteCount = 0;
            
            for (int k = 0; k < atlases.Count; k++)
            {
                spriteCount += atlases[k].sprites.Count;
            }
            
            var copies = new NativeArray<SpriteCopy>(spriteCount, Allocator.TempJob);
            int spriteIndex = 0;
            
            for (int k = 0; k < atlases.Count; k++)
            {
                var atlas = atlases[k];
                var sprites = atlas.sprites;
                int dstStride = atlas.size.x * Bpp;
                
                for (int i = 0; i < sprites.Count; i++)
                {
                    var sp = sprites[i];
                    var s  = sp.size;
                    var ps = sp.pixelStart;

                    copies[spriteIndex] = new SpriteCopy
                    {
                        AtlasStride = dstStride,
                        AtlasPtr = (byte*)atlas.raw.GetUnsafePtr(),
                        SrcPtr     = sp.renderData.srcPtr,
                        SrcStride  = sp.renderData.srcStride,
                        WidthBytes = s.x * Bpp,
                        Height     = s.y,
                        DstOffset  = (ps.y * dstStride) + (ps.x * Bpp),
                    };
                    
                    spriteIndex++;
                }
                
                sprites.FakeClear();
            }


            var job = new CopySpritesJob
            {
                Sprites = copies,
            };
            
            job.ScheduleParallel(copies.Length, 8, default).Complete();
            
            copies.Dispose();
            atlases.FakeClear();
            IsSpritesDirty = false;
        }


        private Vector2Int size;
        public Texture2D Texture { get; }
        private NativeArray<byte> raw;

        private bool isTextureDirty;

        private bool IsTextureDirty
        {
            get => isTextureDirty;
            set
            {
                if (value == isTextureDirty) return;
                isTextureDirty = value;
                if (value)
                {
                    LottieUpdater.TextureApplyTime += ApplyTexture;
                }
                else
                {
                    LottieUpdater.TextureApplyTime -= ApplyTexture;
                }
            }
        }
        
        private static bool isSpritesDirty;

        private static bool IsSpritesDirty
        {
            get => isSpritesDirty;
            set
            {
                if (value == isSpritesDirty) return;
                isSpritesDirty = value;
                if (value)
                {
                    LottieUpdater.TextureApplyTime += CopyAll;
                }
                else
                {
                    LottieUpdater.TextureApplyTime -= CopyAll;
                }
            }
        }

        private Atlas(Vector2Int size)
        {
            Debug.Log($"Create Atlas {size}");
            this.size = size;
            var tex = CreateTexture();
            Texture = tex;
            raw = tex.GetRawTextureData<byte>();
        }

        private LSList<Sprite> sprites = new(8);
        private static LSList<Atlas> atlases = new(8);

        public void AddToBlit(in Sprite sprite)
        {
            sprites.Add(sprite);
            if (!IsTextureDirty)
            {
                atlases.Add(this);
            }
            IsSpritesDirty = true;
            IsTextureDirty = true;
        }

        private void ApplyTexture()
        {
            var lastDirty = IsTextureDirty;
            if (!lastDirty) return;
            IsTextureDirty = false;
            Texture.Apply(false, false);
        }

        public void Destroy(bool immediate)
        {
            if (immediate) Object.DestroyImmediate(Texture);
            else Object.Destroy(Texture);
        }

        private Texture2D CreateTexture() => CreateTexture(size);

        private static Texture2D CreateTexture(Vector2Int size)
        {
            const GraphicsFormat fmt = GraphicsFormat.B8G8R8A8_SRGB;
            const TextureCreationFlags flags = TextureCreationFlags.DontInitializePixels |
                                               TextureCreationFlags.DontUploadUponCreate;
            var texture = new Texture2D(size.x, size.y, fmt, flags)
            {
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp
            };
            return texture;
        }

        private static LSList<Lottie> pending = new(64);
        private static readonly int[] allowedAtlas = { 256, 512, 1024, 2048 };
        
        private sealed class SizeClassState
        {
            public readonly int tile;
            public readonly int atlas;
            public readonly int grid;
            public readonly int capacity;
            public readonly int gridShift;
            public readonly int capMask;
            public readonly float invA;
            public readonly List<Atlas> pages;
            public int placed;

            public SizeClassState(int tile, int atlas)
            {
                this.tile = tile;
                this.atlas = atlas;
                grid = atlas / tile;
                capacity = grid * grid;
                gridShift = Pow2Shift(grid);
                capMask = capacity - 1;
                invA = 1f / atlas;
                pages = new List<Atlas>(4);
                placed = 0;
            }

            private static int Pow2Shift(int v)
            {
                int s = 0;
                while ((v >>= 1) != 0) s++;
                return s;
            }
        }

        public static void Pack(LSList<Lottie> animations)
        {
            foreach (var pool in atlasPools.Values) pool.ReleaseAll();
            pending.FakeClear();
            if (animations == null || animations.Count == 0) return;

            var counts = new Dictionary<int, int>(8);
            int n = animations.Count;
            for (int i = 0; i < n; i++)
            {
                int s = animations[i].size.x;
                if (counts.TryGetValue(s, out int c)) counts[s] = c + 1;
                else counts[s] = 1;
            }

            var classes = new Dictionary<int, SizeClassState>(counts.Count);
            foreach (var kv in counts)
            {
                int s = kv.Key;
                int cnt = kv.Value;

                int bestA = 256;
                int bestPages = int.MaxValue;
                long bestCost = long.MaxValue;
                int bestCap = 1;

                for (int i = 0; i < allowedAtlas.Length; i++)
                {
                    int A = allowedAtlas[i];
                    if (A < s) continue;

                    int g = A / s;
                    int cap = g * g;
                    if (cap <= 0) continue;

                    int pages = (cnt + cap - 1) / cap;
                    long cost = pages * (long)A * A;

                    bool better =
                        (pages < bestPages) ||
                        (pages == bestPages && cost < bestCost) ||
                        (pages == bestPages && cost == bestCost && A > bestA);

                    if (better)
                    {
                        bestA = A;
                        bestPages = pages;
                        bestCost = cost;
                        bestCap = cap;
                    }
                }

                var sc = new SizeClassState(s, bestA);

                int pagesNeeded = (cnt + bestCap - 1) / bestCap;
                for (int p = 0; p < pagesNeeded; p++)
                    sc.pages.Add(Get(bestA));

                classes[s] = sc;
            }

            for (int i = 0; i < n; i++)
            {
                var lottie = animations[i];
                int s = lottie.size.x;
                var sc = classes[s];

                int idx = sc.placed++;
                int pageIndex = idx >> (sc.gridShift * 2);
                int cellIndex = idx & sc.capMask;
                int gx = cellIndex & (sc.grid - 1);
                int gy = cellIndex >> sc.gridShift;

                float u0 = gx * s * sc.invA;
                float v0 = gy * s * sc.invA;
                float u1 = u0 + s * sc.invA;
                float v1 = v0 + s * sc.invA;

                var uvMin = new Vector2(u0, v0);
                var uvMax = new Vector2(u1, v1);

                var sprite = new Sprite(sc.pages[pageIndex], uvMin, uvMax);
                lottie.Spritee = sprite;
            }
        }

    }
}