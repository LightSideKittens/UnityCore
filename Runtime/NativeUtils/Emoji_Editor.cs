#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
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

        private static byte[] GetRawBytes(string emoji)
        {
            ignoreGuiDepth ??= typeof(Event).GetField("ignoreGuiDepth", BindingFlags.Static | BindingFlags.NonPublic);
            var prev =  ignoreGuiDepth.GetValue(null);
            ignoreGuiDepth.SetValue(null, true);
            var previousEvent = Event.current;
            repaintEvent = new Event { type = EventType.Repaint };

            try
            {
                Event.current = repaintEvent;
                var texture = RenderEmojiToTexture(emoji);
                var bytes = texture.GetRawTextureData();
                Object.DestroyImmediate(texture);
                return bytes;
            }
            finally
            {
                ignoreGuiDepth.SetValue(null, prev);
                Event.current = previousEvent;
            }
        }
        
        private static Range[] ProcessEmojis(string text)
        {
            if (string.IsNullOrEmpty(text)) return Array.Empty<Range>();
            
            var emojiClusters = GetEmojiClusters(text);
            var result = new Range[emojiClusters.Count];

            for (var i = 0; i < emojiClusters.Count; i++)
            {
                var cluster = emojiClusters[i];
                var emoji = cluster.Emoji;
                var startIndex = cluster.StartIndex;
                var length = cluster.Length;

                var range = new Range
                {
                    index = startIndex,
                    length = length,
                    emoji = emoji
                };
                result[i] = range;
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

        private static bool IsEmoji(string cluster)
        {
            if (string.IsNullOrEmpty(cluster)) return false;

            var offset = 0;
            var length = cluster.Length;

            while (offset < length)
            {
                var codePoint = char.ConvertToUtf32(cluster, offset);
                offset += char.IsSurrogatePair(cluster, offset) ? 2 : 1;

                if (IsEmojiCodepoint(codePoint))
                {
                    return true;
                }
            }

            return false;
        }
        
        private static bool IsEmojiCodepoint(int codePoint)
        {
            return
                codePoint is >= 0x1F600 and <= 0x1F64F ||   // Emoticons
                codePoint is >= 0x1F300 and <= 0x1F5FF ||   // Misc Symbols and Pictographs
                codePoint is >= 0x1F680 and <= 0x1F6FF ||   // Transport and Map Symbols
                codePoint is >= 0x1F900 and <= 0x1F9FF ||   // Supplemental Symbols and Pictographs
                codePoint is >= 0x2600 and <= 0x26FF  ||   // Misc symbols
                codePoint is >= 0x2700 and <= 0x27BF  ||   // Dingbats
                codePoint is >= 0x2B00 and <= 0x2BFF  ||   // Misc symbols and arrows
                codePoint is >= 0x1F100 and <= 0x1F1FF ||   // Enclosed Alphanumeric Supplement
                codePoint is >= 0x1F200 and <= 0x1F2FF ||   // Enclosed Ideographic Supplement
                codePoint is >= 0x1FA70 and <= 0x1FAFF;      // Symbols and Pictographs Extended-A
        }
        
        private static Texture2D RenderEmojiToTexture(string emoji)
        {
            var dpiScaling = typeof(GUIUtility).Eval<float>("pixelsPerPoint");
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

        private class EmojiCluster
        {
            public string Emoji { get; set; }
            public int StartIndex { get; set; }
            public int Length { get; set; }
        }
    }
}
#endif
