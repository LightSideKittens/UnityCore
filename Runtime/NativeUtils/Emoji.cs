using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LSCore.NativeUtils
{
    public class EmojiRange
    {
        public int index;
        public int length;
        public string imagePath;
    }

    public  static partial class Emoji
    {
        private static string packageName = "com.lscore.emoji";
        private static AndroidJavaClass textRenderer;
        private static AndroidJavaObject currentActivity;

        // Ссылка на Java класс API
        private static AndroidJavaClass API => textRenderer ??= new AndroidJavaClass($"{packageName}.API");

        // Ссылка на текущую активити Unity
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

        /// <summary>
        /// Вызывает parseEmojis у плагина и возвращает массив EmojiRange.
        /// text - текст для анализа
        /// saveDirPath - путь к директории, где будут сохранены PNG-файлы эмодзи
        /// </summary>
        public static EmojiRange[] ParseEmojis(string text, string saveDirPath)
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

            foreach (var range in emojiRanges)
            {
                int adjustedIndex = range.index + offset;
                result.Remove(adjustedIndex, range.length);
                result.Insert(adjustedIndex, replacement);
                range.index += offset;
                offset += replacement.Length - range.length;
            }

            return result.ToString();
        }
    }
}
