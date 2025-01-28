using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LSCore.DataStructs;
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
        private static AndroidJavaClass textRenderer;
        private static AndroidJavaObject currentActivity;

        private static AndroidJavaClass API => textRenderer ??= new AndroidJavaClass($"{packageName}.API");

        private static Dictionary<string, Texture2D> cachedTextures = new();
        private static List<Texture2D> texturesArr = new (4096);
        private static List<EmojiRange> emojiRanges = new (4096);
        
        public static ListSpan<EmojiRange> ParseEmojis(string text, string saveDirPath, out ListSpan<Texture2D> textures)
        {
            ListSpan<EmojiRange> result = GetArray(text, saveDirPath);
            int i = 0;
            
            for (; i < result.Count; i++)
            {
                EmojiRange entry = result[i];
                string path = entry.imagePath;
                
                if (!cachedTextures.TryGetValue(path, out Texture2D texture))
                {
                    texture = new Texture2D(2, 2);
                    texture.LoadImage(File.ReadAllBytes(path));
                    texture.hideFlags = HideFlags.DontSave;
                    cachedTextures[path] = texture;
                }
                
                if (texturesArr.Count <= i)
                {
                    texturesArr.Add(texture);
                }
                else
                {
                    texturesArr[i] = texture;
                }
            }
            
            textures = texturesArr.AsSpan(..i);
            return result;
        }
        
        public static ListSpan<EmojiRange> GetArray(string text, string saveDirPath)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                return IListExtensions.AsSpan(ProcessEmojis(text), ..);
            }
#endif
            AndroidJavaObject[] emojiRangesJavaArray = API.CallStatic<AndroidJavaObject[]>("parseEmojis", text, saveDirPath);

            if (emojiRangesJavaArray == null || emojiRangesJavaArray.Length == 0)
            {
                return IListExtensions.AsSpan(Array.Empty<EmojiRange>(), ..);
            }
            
            int i = 0;
            
            for (; i < emojiRangesJavaArray.Length; i++)
            {
                using (AndroidJavaObject item = emojiRangesJavaArray[i])
                {
                    EmojiRange range = new EmojiRange
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
        
        public static string ReplaceWithEmojiRanges(string input, ListSpan<EmojiRange> ranges, string replacement, ListSpan<Texture2D> textures)
        {
            StringBuilder result = new StringBuilder(input);
            int offset = 0; 
            int replacementLength = replacement.Length;
            int i = 0;
            
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
                EmojiRange range = ranges[i];
                int adjustedIndex = range.index + offset;
                range.adjustedIndex = adjustedIndex;
                result.Remove(adjustedIndex, range.length);
                result.Insert(adjustedIndex, replacement);
                offset += replacementLength - range.length;
                ranges[i] = range;
            }
        }
    }
}
