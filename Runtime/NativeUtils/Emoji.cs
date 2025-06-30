using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LSCore.ConfigModule;
using LSCore.DataStructs;
using Newtonsoft.Json;
using UnityEngine;

namespace LSCore.NativeUtils
{
    public static partial class Emoji
    {
        public class ConfigManager : 
#if UNITY_EDITOR
            EditorConfigManager<Config>
#else
            LocalDynamicConfigManager<Config>
#endif
        {
            protected override string DefaultPath => "EmojiConfig";
        }
        
        public class Config : LocalDynamicConfig
        {
            private static Config instance;
            public static ConfigManager Manager => ConfigMaster<ConfigManager>.Default;
            public static Config Instance => instance ??= Manager.Config;
            public Dictionary<string, Sprite> sprites = new();
            public List<Atlas> atlases = new();
        }
        
        public struct Range
        {
            public int index;
            public int adjustedIndex;
            public int length;
            public string emoji;
        }
        
        [Serializable]
        public struct Sprite
        {
            public int atlasIndex;
            public Vector2 uvMin;
            public Vector2 uvMax;
            private Atlas atlas;

            [JsonIgnore]
            public Atlas Atlas
            {
                get { return atlas ??= Atlas.Get(atlasIndex); }
                set
                {
                    atlasIndex = value.index;
                    atlas = value;
                }
            }
        }
        
        [Serializable]
        public class Atlas
        {
            private const int AtlasSize     = 1024;
            private const int BlockSize     = 256;
            private const int MaxBlocksInAtlas = (AtlasSize * AtlasSize) / (BlockSize * BlockSize);
#if UNITY_EDITOR
            private const int BytesPerPixel  = 8;
            public const TextureFormat Format = TextureFormat.RGBA64;
#else
            public const TextureFormat Format = TextureFormat.RGBA32;
            private const int BytesPerPixel  = 4;
#endif
            private const int BlockSizeBytes = BlockSize * BlockSize * BytesPerPixel;
            private const int AtlasSizeBytes = AtlasSize * AtlasSize * BytesPerPixel;

            public int index;
            public int spriteCount;
            [JsonIgnore] public bool isDirty = true;
            private Texture2D texture;
            private byte[] bytes;

            private void LoadBytesIfNeeded()
            {
                if(bytes != null) return;
                var path = Path.Combine(CachePath, string.Concat(index, ".atlas"));
                bytes = File.ReadAllBytes(path);
            }
            
            [JsonIgnore]
            public Texture2D Texture
            {
                get
                {
                    if (isDirty)
                    {
                        var path = Path.Combine(CachePath, string.Concat(index, ".atlas"));
                        var bytesWasNull = bytes == null;
                        if (bytesWasNull)
                        {
                            bytes = File.ReadAllBytes(path);
                        }
                        var isFulled = IsFulled;
                        texture ??= new Texture2D(AtlasSize, AtlasSize, Format, false);
                        texture.LoadRawTextureData(bytes);
                        texture.Apply(updateMipmaps: false, makeNoLongerReadable: isFulled);
                        if (!bytesWasNull)
                        {
                            File.WriteAllBytes(path, bytes);
                        }
                        else if(isFulled)
                        {
                            bytes = null;
                        }
                        Config.Manager.Save();
                        isDirty = false;
                    }
                    
                    return texture;
                }
            }

            
            [JsonIgnore]
            public bool IsFulled => spriteCount == MaxBlocksInAtlas;
            
            public static Atlas Get(int atlasIndex)
            {
                var config = Config.Instance;
                return config.atlases[atlasIndex];
            }

            public static Sprite Add(byte[] rgbaBlockBytes)
            {
                var config = Config.Instance;

                Atlas atlas;
                
                if (config.atlases.Count == 0)
                {
                    AddAtlas();
                }
                else
                {
                    atlas = config.atlases[^1];
                    atlas.LoadBytesIfNeeded();
                }
                
                var sprite = atlas.Internal_Add(rgbaBlockBytes);
                
                if (atlas.IsFulled)
                {
                    AddAtlas();
                }

                return sprite;
                
                void AddAtlas()
                {
                    atlas = new Atlas();
                    atlas.bytes = new byte[AtlasSizeBytes]; 
                    atlas.index = config.atlases.Count;
                    config.atlases.Add(atlas);
                }
            }
            
            private Sprite Internal_Add(byte[] rgbaBlockBytes)
            {
                Sprite sprite = default;
                isDirty = true;

                if (IsFulled)
                {
                    return sprite;
                }

                var i = spriteCount;
                var xSlot = i % (AtlasSize / BlockSize);
                var ySlot = i / (AtlasSize / BlockSize);

                var xPixelOffset = xSlot * BlockSize;
                var yPixelOffset = ySlot * BlockSize;

                const int strideAtlas = AtlasSize * BytesPerPixel;
                const int strideSmall = BlockSize * BytesPerPixel;
                
                for (var row = 0; row < BlockSize; row++)
                {
                    var srcOff  = row * strideSmall;
                    var dstOff  = ((yPixelOffset + row) * strideAtlas) + (xPixelOffset * BytesPerPixel);
                    Buffer.BlockCopy(rgbaBlockBytes, srcOff, bytes, dstOff, strideSmall);
                }
                
                var uvUnit = BlockSize / (float)AtlasSize;
                
                sprite.uvMin = new Vector2(xSlot * uvUnit, ySlot * uvUnit);
                sprite.uvMax = new Vector2((xSlot + 1) * uvUnit, (ySlot + 1) * uvUnit);
                sprite.Atlas = this;
                spriteCount++;
                return sprite;
            }
        }
        
        private static string cachePath;
        public static string CachePath => cachePath ??= Path.Combine(Application.persistentDataPath, "Emojis");
        private static string packageName = "com.lscore.emoji";
        private static AndroidJavaClass textRenderer;
        private static AndroidJavaObject currentActivity;

        static Emoji()
        {
            Directory.CreateDirectory(CachePath);
        }

        private static AndroidJavaClass API => textRenderer ??= new AndroidJavaClass($"{packageName}.API");
        
        private static List<Sprite> spritesArr = new (4096);
        private static List<Range> emojiRanges = new (4096);
        
        public static ListSpan<Range> ParseEmojis(string text, out ListSpan<Sprite> sprites)
        {
            var spritesDict = Config.Instance.sprites;
            var result = GetArray(text);
            var i = 0;
            
            for (; i < result.Count; i++)
            {
                var entry = result[i];
                var emoji = entry.emoji;
                
                if (!spritesDict.TryGetValue(emoji, out var sprite))
                {
                    var emojiData = GetRaw(emoji); 
                    sprite = Atlas.Add(emojiData);
                    spritesDict[emoji] = sprite;
                }
                
                if (spritesArr.Count <= i)
                {
                    spritesArr.Add(sprite);
                }
                else
                {
                    spritesArr[i] = sprite;
                }
            }
            
            sprites = spritesArr.AsSpan(..i);
            return result;
        }

        public static byte[] GetRaw(string emoji)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                return GetRawBytes(emoji);
            }
#endif
            
            return API.CallStatic<byte[]>("emojiToRGBA32", emoji); 
        }
        
        public static ListSpan<Range> GetArray(string text)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                return ((IList<Range>)ProcessEmojis(text)).AsSpan(..);
            }
#endif
            var emojiRangesJavaArray = API.CallStatic<AndroidJavaObject[]>("parseEmojis", text);

            if (emojiRangesJavaArray == null || emojiRangesJavaArray.Length == 0)
            {
                return ((IList<Range>)Array.Empty<Range>()).AsSpan(..);
            }
            
            var i = 0;
            
            for (; i < emojiRangesJavaArray.Length; i++)
            {
                using (var item = emojiRangesJavaArray[i])
                {
                    var range = new Range
                    {
                        index = item.Get<int>("index"),
                        length = item.Get<int>("length"),
                        emoji = item.Get<string>("emoji")
                    };

                    if (emojiRanges.Count <= i)
                    {
                        emojiRanges.Add(range);
                    }
                    else
                    {
                        emojiRanges[i] = range;
                    }
                }
            }
            
            return emojiRanges.AsSpan(..i);
        }
        
        public static string ReplaceWithEmojiRanges(string input, ListSpan<Range> ranges, string replacement, ListSpan<Sprite> textures)
        {
            var result = new StringBuilder(input);
            var offset = 0; 
            var replacementLength = replacement.Length;
            var i = 0;
            
            for (; i < textures.Count; i++)
            {
                ModifyResult();
            }
            
            replacement = "\u200b";
            replacementLength = 1;
            
            for (; i < ranges.Count; i++)
            {
                ModifyResult();
            }
            
            return result.ToString();

            void ModifyResult()
            {
                var range = ranges[i];
                var adjustedIndex = range.index + offset;
                range.adjustedIndex = adjustedIndex;
                result.Remove(adjustedIndex, range.length);
                result.Insert(adjustedIndex, replacement);
                offset += replacementLength - range.length;
                ranges[i] = range;
            }
        }
    }
}
