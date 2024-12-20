using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace LSCore.NativeUtils
{
    public class EmojiRange
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
        private static Texture2D[] texturesArr = new Texture2D[100];
        
        public static EmojiRange[] ParseEmojis(string text, string saveDirPath, out Texture2D[] textures)
        {
            var result = GetArray(text, saveDirPath);
            int i = 0;
            
            for (; i < result.Length; i++)
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
        
        public static EmojiRange[] GetArray(string text, string saveDirPath)
        {
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                return ProcessEmojis(text);
            }
#endif
            AndroidJavaObject[] emojiRangesJavaArray = API.CallStatic<AndroidJavaObject[]>("parseEmojis", text, saveDirPath);

            if (emojiRangesJavaArray == null || emojiRangesJavaArray.Length == 0)
            {
                return Array.Empty<EmojiRange>();
            }
            
            EmojiRange[] result = new EmojiRange[emojiRangesJavaArray.Length];
            
            for (int i = 0; i < emojiRangesJavaArray.Length; i++)
            {
                using (var item = emojiRangesJavaArray[i])
                {
                    result[i] = new EmojiRange
                    {
                        index = item.Get<int>("index"),
                        length = item.Get<int>("length"),
                        imagePath = item.Get<string>("imagePath")
                    };
                }
            }
            
            return result;
        }
        
        public static string ReplaceWithEmojiRanges(string input, EmojiRange[] emojiRanges, string replacement)
        {
            StringBuilder result = new StringBuilder(input);
            int offset = 0; 
            int replacementLength = replacement.Length;

            foreach (var range in emojiRanges)
            {
                int adjustedIndex = range.index + offset;
                range.adjustedIndex = adjustedIndex;
                result.Remove(adjustedIndex, range.length);
                result.Insert(adjustedIndex, replacement);
                offset += replacementLength - range.length;
            }

            return result.ToString();
        }
    }
}
