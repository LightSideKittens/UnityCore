//-----------------------------------------------------------------------
// <copyright file="CachedMemoryStream.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.Serialization
{
#pragma warning disable

    using Sirenix.Serialization.Utilities;
    using System.IO;

    internal sealed class CachedMemoryStream : ICacheNotificationReceiver
    {
        public static int InitialCapacity = 1024 * 1; // Initial capacity of 1 kb
        public static int MaxCapacity = 1024 * 32; // Max of 32 kb cached stream size

        private MemoryStream memoryStream;

        public MemoryStream MemoryStream
        {
            get
            {
                if (!this.memoryStream.CanRead)
                {
                    this.memoryStream = new MemoryStream(InitialCapacity);
                }

                return this.memoryStream;
            }
        }

        public CachedMemoryStream()
        {
            this.memoryStream = new MemoryStream(InitialCapacity);
        }

        public void OnFreed()
        {
            this.memoryStream.SetLength(0);
            this.memoryStream.Position = 0;

            if (this.memoryStream.Capacity > MaxCapacity)
            {
                this.memoryStream.Capacity = MaxCapacity;
            }
        }

        public void OnClaimed()
        {
            this.memoryStream.SetLength(0);
            this.memoryStream.Position = 0;
        }

        public static Cache<CachedMemoryStream> Claim(int minCapacity)
        {
            var cache = Cache<CachedMemoryStream>.Claim();

            if (cache.Value.MemoryStream.Capacity < minCapacity)
            {
                cache.Value.MemoryStream.Capacity = minCapacity;
            }

            return cache;
        }

        public static Cache<CachedMemoryStream> Claim(byte[] bytes = null)
        {
            var cache = Cache<CachedMemoryStream>.Claim();

            if (bytes != null)
            {
                cache.Value.MemoryStream.Write(bytes, 0, bytes.Length);
                cache.Value.MemoryStream.Position = 0;
            }

            return cache;
        }
    }
}