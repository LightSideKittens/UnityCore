#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LSCore.NativeUtils
{
    public static partial class Emoji
    {
        [MenuItem("CONTEXT/LSText/Clear Emoji Cache")]
        public static void ClearEmojiCache()
        {
            Directory.Delete(EmojiCachePath, true);
        }
        
        private static Event repaintEvent;
        private static FieldInfo ignoreGuiDepth;
        private static string EmojiCachePath => Path.Combine(Application.persistentDataPath, "EmojiCache");

        private static EmojiRange[] ProcessEmojis(string text)
        {
            if (string.IsNullOrEmpty(text)) return Array.Empty<EmojiRange>();
            
            var emojiClusters = GetEmojiClusters(text);
            var result = new EmojiRange[emojiClusters.Count];

            for (var i = 0; i < emojiClusters.Count; i++)
            {
                var cluster = emojiClusters[i];
                var emoji = cluster.Emoji;
                var startIndex = cluster.StartIndex;
                var length = cluster.Length;
                var cachedPath = Path.Combine(EmojiCachePath, $"{ToValidFileName(emoji)}.png");

                var range = new EmojiRange
                {
                    index = startIndex,
                    length = length,
                    imagePath = cachedPath
                };
                result[i] = range;
                
                if (!File.Exists(cachedPath))
                {
                    ignoreGuiDepth ??= typeof(Event).GetField("ignoreGuiDepth", BindingFlags.Static | BindingFlags.NonPublic);
                    var prev =  ignoreGuiDepth.GetValue(null);
                    ignoreGuiDepth.SetValue(null, true);
                    Event previousEvent = Event.current;
                    repaintEvent = new Event { type = EventType.Repaint };

                    try
                    {
                        Event.current = repaintEvent;
                        var texture = RenderEmojiToTexture(emoji);
                        SaveTextureAsPNG(texture, cachedPath);
                        Object.DestroyImmediate(texture);
                    }
                    finally
                    {
                        ignoreGuiDepth.SetValue(null, prev);
                        Event.current = previousEvent;
                    }
                }
            }

            return result;
        }

        private static List<EmojiCluster> GetEmojiClusters(string text)
        {
            var emojiClusters = new List<EmojiCluster>();
            var enumerator = StringInfo.GetTextElementEnumerator(text);

            while (enumerator.MoveNext())
            {
                var element = enumerator.GetTextElement();
                var index = enumerator.ElementIndex;

                if (IsEmoji(element))
                {
                    emojiClusters.Add(new EmojiCluster
                    {
                        Emoji = element,
                        StartIndex = index,
                        Length = element.Length
                    });
                }
            }

            return emojiClusters;
        }

        private static bool IsEmoji(string element)
        {
            foreach (var ch in element)
            {
                var codePoint = char.ConvertToUtf32(element, 0);

                if ((codePoint >= 0x1F300 && codePoint <= 0x1F5FF) || // Символы эмодзи
                    (codePoint >= 0x1F600 && codePoint <= 0x1F64F) || // Эмоции
                    (codePoint >= 0x1F680 && codePoint <= 0x1F6FF) || // Транспорт
                    (codePoint >= 0x2600 && codePoint <= 0x26FF) || // Разное
                    (codePoint >= 0x2700 && codePoint <= 0x27BF) || // Символы
                    (codePoint >= 0x1F1E6 && codePoint <= 0x1F1FF) || // Региональные индикаторы
                    (codePoint >= 0x1F900 && codePoint <= 0x1F9FF)) // Дополнительные эмодзи
                {
                    return true;
                }
            }

            return false;
        }
        
        private static Texture2D RenderEmojiToTexture(string emoji)
        {
            // Размер текстуры и шрифта
            float dpiScaling = Screen.dpi / 96f; // DPI стандартного экрана — 96
            if (dpiScaling <= 0) dpiScaling = 1;
            
            var textureSize = 256; // Размер текстуры
            var fontSize = (int)(200 * dpiScaling); // Размер шрифта

            // Настройка шрифта
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var style = new GUIStyle
            {
                font = font,
                fontSize = fontSize,
                alignment = TextAnchor.UpperCenter, // Ставим текст сверху
                normal = { textColor = Color.white }
            };

            // Создание RenderTexture
            var renderTexture = RenderTexture.GetTemporary(textureSize, textureSize, 0, RenderTextureFormat.ARGB64);

            // Создание текстуры для хранения результата
            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA64, false);

            // Настройка матрицы и рендеринг текста
            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);

            // Поднимаем текст вверх (смещение)
            var offsetY = textureSize * 0.05f; // Смещение на 10% вверх
            var rect = new Rect(0, -offsetY, textureSize, textureSize);

            // Рендеринг через GUI с использованием заданного стиля
            GUI.matrix = Matrix4x4.identity;
            Graphics.SetRenderTarget(renderTexture);
            style.Draw(rect, new GUIContent(emoji), GUIUtility.GetControlID(FocusType.Keyboard), false);

            // Чтение из RenderTexture
            texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            texture.Apply();

            // Очистка
            RenderTexture.ReleaseTemporary(renderTexture);
            RenderTexture.active = null;

            return texture;
        }

        private static void SaveTextureAsPNG(Texture2D texture, string filePath)
        {
            var pngData = texture.EncodeToPNG();
            if (pngData != null)
            {
                if (!Directory.Exists(EmojiCachePath))
                {
                    Directory.CreateDirectory(EmojiCachePath);
                }

                File.WriteAllBytes(filePath, pngData);
                AssetDatabase.Refresh();
            }
        }

        private static string ToValidFileName(string input)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                input = input.Replace(c, '_');
            }

            return input;
        }

        private class EmojiCluster
        {
            public string Emoji { get; set; }
            public int StartIndex { get; set; }
            public int Length { get; set; }
        }
    }
}
#endif
