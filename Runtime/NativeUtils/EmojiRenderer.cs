using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using UnityEngine;
using Color = System.Drawing.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;
using Graphics = System.Drawing.Graphics;

public static class EmojiRenderer
{
#if UNITY_ANDROID
    private const string pluginName = "com.lscore.emojirenderer.EmojiRendererPlugin";
#endif
    
    [DllImport("__Internal")]
    private static extern string renderEmoji(string text, float fontSize);

    public static List<string> ExtractEmojis(string input)
    {
        List<string> emojis = new List<string>();
        TextElementEnumerator enumerator = StringInfo.GetTextElementEnumerator(input);

        while (enumerator.MoveNext())
        {
            string element = enumerator.GetTextElement();

                emojis.Add(element);
            
        }

        return emojis;
    }

    static bool IsEmoji(string textElement)
    {
        foreach (char ch in textElement)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category is UnicodeCategory.OtherSymbol or UnicodeCategory.NonSpacingMark or UnicodeCategory.EnclosingMark)
            {
                return true;
            }
        }

        return false;
    }
    
    public static bool TryRenderEmoji(string emoji, float fontSize, out Texture2D texture)
    {
        if (!IsEmoji(emoji))
        {
            texture = null;
            return false;
        }
        
        string text = emoji;
        string base64Image = null;

#if !UNITY_EDITOR
    #if UNITY_ANDROID
            using (AndroidJavaClass pluginClass = new AndroidJavaClass(pluginName))
            {
                base64Image = pluginClass.CallStatic<string>("renderEmoji", text, fontSize);
            }
    #elif UNITY_IOS
            base64Image = renderEmoji(text, fontSize);
    #endif
#endif
        
#if UNITY_EDITOR_WIN
        base64Image = RenderEmojiOnWindows(text, fontSize);
#endif

        if (string.IsNullOrEmpty(base64Image))
        {
            Debug.LogError("Failed to render emoji");
            texture = null;
            return false;
        }

        byte[] imageBytes = Convert.FromBase64String(base64Image);
        texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        return true;
    }

#if UNITY_EDITOR_WIN
    private static string RenderEmojiOnWindows(string text, float fontSize)
    {
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        Font font = new Font("Segoe UI Emoji", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        SizeF textSize = graphics.MeasureString(text, font);
        using var textBitmap = new Bitmap((int)textSize.Width, (int)textSize.Height);
        using var textGraphics = Graphics.FromImage(textBitmap);
        textGraphics.Clear(Color.Transparent);
        textGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        textGraphics.DrawString(text, font, Brushes.Black, 0, 0);

        using var stream = new MemoryStream();
        textBitmap.Save(stream, ImageFormat.Png);
        return Convert.ToBase64String(stream.ToArray());
    }
#endif
}