﻿#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using LSCore.Extensions;
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

        private static readonly string EmojiPattern =
            // однобайтовые эмодзи и символы-маркировки
            @"[\u203C\u2049\u2122\u2139\u24C2\u2600-\u26FF\u2700-\u27BF\uFE0E\uFE0F]" +
            // суррогатные пары для основных категорий эмодзи
            @"|(\uD83C[\uDDE6-\uDDFF])" +   // региональные флажки
            @"|(\uD83C[\uDF00-\uDFFF])" +   // Misc Symbols & Pictographs
            @"|(\uD83D[\uDC00-\uDE4F])" +   // Emoticons
            @"|(\uD83D[\uDE80-\uDEF6])";    // Transport & Map

        public static bool IsEmoji(string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            return Regex.IsMatch(input, EmojiPattern);
        }
        
        private static Texture2D RenderEmojiToTexture(string emoji)
        {
            float dpiScaling = typeof(GUIUtility).Eval<float>("pixelsPerPoint");
            if (dpiScaling <= 0) dpiScaling = 1;
            
            var textureSize = 256;
            var fontSize = (int)(230 / dpiScaling);

            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            var style = new GUIStyle
            {
                font = font,
                fontSize = fontSize,
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };

            var renderTexture = RenderTexture.GetTemporary(textureSize, textureSize, 0, RenderTextureFormat.ARGB64);

            var texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA64, false);

            RenderTexture.active = renderTexture;
            GL.Clear(true, true, Color.clear);

            var offsetX = textureSize - (textureSize / dpiScaling);
            offsetX /= 2;
            var offsetY = textureSize * (0.05f / dpiScaling);
            var rect = new Rect(-offsetX, -offsetY, textureSize, textureSize);

            GUI.matrix = Matrix4x4.identity;
            Graphics.SetRenderTarget(renderTexture);
            style.Draw(rect, new GUIContent(emoji), GUIUtility.GetControlID(FocusType.Keyboard), false);

            texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            texture.Apply();

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
