using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LSCore.NativeUtils
{
    public struct EmojiRange
    {
        public int index;
        public int adjustedIndex;
        public int length;
        public string imagePath;
    }

    public static partial class Emoji
    {
        private static string packageName = "com.lscore.emoji";
        private static AndroidJavaClass api;
        private static AndroidJavaObject currentActivity;

        private static AndroidJavaClass API => api ??= new AndroidJavaClass($"{packageName}.API");

        private static AndroidJavaObject CurrentActivity 
        {
            get 
            {
                if (currentActivity == null)
                {
                    using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                    {
                        currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    }
                }
                return currentActivity;
            }
        }

        private static Dictionary<string, Texture2D> cachedTextures = new();
        private static Texture2D[] texturesArr = new Texture2D[1000];
        private static List<EmojiRange> emojiRanges = new (1024);
        
        public static ListSpan<EmojiRange> ParseEmojis(string text, string saveDirPath, out Texture2D[] textures)
        {
            var result = GetArray(text, saveDirPath);
            int i = 0;
            int count = Math.Min(result.Count, texturesArr.Length);
            
            for (; i < count; i++)
            {
                var entry = result[i];
                var path = entry.imagePath;
                
                if (!cachedTextures.TryGetValue(path, out Texture2D texture))
                {
                    texture = new Texture2D(2, 2);
                    texture.LoadImage(File.ReadAllBytes(path));
                    texture.hideFlags = HideFlags.DontSave;
                    cachedTextures[path] = texture;
                }
                
                texturesArr[i] = texture;
            }
            
            textures = texturesArr[..i];
            return result;
        }
        
        public static ListSpan<EmojiRange> GetArray(string text, string saveDirPath)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                return ProcessEmojis(text).AsSpan(..);
            }
#endif
            AndroidJavaObject[] emojiRangesJavaArray = API.CallStatic<AndroidJavaObject[]>("parseEmojis", text, saveDirPath);

            if (emojiRangesJavaArray == null || emojiRangesJavaArray.Length == 0)
            {
                return Array.Empty<EmojiRange>().AsSpan(..);
            }
            
            int i = 0;
            
            for (; i < emojiRangesJavaArray.Length; i++)
            {
                using (var item = emojiRangesJavaArray[i])
                {
                    var range = new EmojiRange
                    {
                        index = item.Get<int>("index"),
                        length = item.Get<int>("length"),
                        imagePath = item.Get<string>("imagePath")
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
        
        public static string ReplaceWithEmojiRanges(string input, ListSpan<EmojiRange> ranges, string replacement, Texture2D[] textures)
        {
            StringBuilder result = new StringBuilder(input);
            int offset = 0; 
            int replacementLength = replacement.Length;

            for (int i = 0; i < textures.Length; i++)
            {
                var range = ranges[i];
                int adjustedIndex = range.index + offset;
                range.adjustedIndex = adjustedIndex;
                result.Remove(adjustedIndex, range.length);
                result.Insert(adjustedIndex, replacement);
                offset += replacementLength - range.length;
                ranges[i] = range;
            }
            
            replacement = string.Empty;
            replacementLength = 0;
            
            for (int i = textures.Length; i < ranges.Count; i++)
            {
                var range = ranges[i];
                int adjustedIndex = range.index + offset;
                range.adjustedIndex = adjustedIndex;
                result.Remove(adjustedIndex, range.length);
                result.Insert(adjustedIndex, replacement);
                offset += replacementLength - range.length;
                ranges[i] = range;
            }

            return result.ToString();
        }
    }
}
