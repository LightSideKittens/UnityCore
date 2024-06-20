using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using LSCore.Runtime;
using UnityEngine;
using Color = UnityEngine.Color;
using Font = System.Drawing.Font;
using FontStyle = System.Drawing.FontStyle;
using Graphics = System.Drawing.Graphics;

public static class EmojiRenderer
{
    private static readonly int[][] emojiRanges =
    {
        new[] { 0x1F600, 0x1F64F }, // Emoticons
        new[] { 0x1F300, 0x1F5FF }, // Miscellaneous Symbols and Pictographs
        new[] { 0x1F680, 0x1F6FF }, // Transport and Map Symbols
        new[] { 0x1F700, 0x1F77F }, // Alchemical Symbols
        new[] { 0x1F780, 0x1F7FF }, // Geometric Shapes Extended
        new[] { 0x1F800, 0x1F8FF }, // Supplemental Arrows-C
        new[] { 0x1F900, 0x1F9FF }, // Supplemental Symbols and Pictographs
        new[] { 0x1FA00, 0x1FA6F }, // Chess Symbols
        new[] { 0x1FA70, 0x1FAFF }, // Symbols and Pictographs Extended-A
        new[] { 0x2600, 0x26FF },   // Miscellaneous Symbols
        new[] { 0x2700, 0x27BF },   // Dingbats
        new[] { 0xFE00, 0xFE0F }    // Variation Selectors
    };
    
#if UNITY_ANDROID
    private const string packageName = "com.lscore.emojirenderer.";
    private static AndroidJavaClass emojiRenderer;
    private static AndroidJavaClass EmojiRendererClass => emojiRenderer ??= new AndroidJavaClass($"{packageName}EmojiRenderer");
#endif
    
    [DllImport("__Internal")]
    private static extern byte[] renderEmoji(string text, float fontSize);
    
    public static void RenderEmoji(string emoji, float fontSize, Color color, out Texture2D texture)
    {
        string text = emoji;
        byte[] imageBytes = null;

#if !UNITY_EDITOR
    #if UNITY_ANDROID
        imageBytes = EmojiRendererClass.CallStatic<byte[]>("renderEmoji", text, fontSize, color.ToARGB());
    #elif UNITY_IOS
            imageBytes = renderEmoji(text, fontSize); //TODO: COLOR
    #endif
#endif
        
#if UNITY_EDITOR_WIN
        imageBytes = RenderEmojiOnWindows(text, fontSize, color);
#endif

        if (imageBytes == null)
        {
            Debug.LogError("Failed to render emoji");
            texture = Texture2D.whiteTexture;
        }
        
        texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
    }

#if UNITY_EDITOR_WIN
    private static float GetWidthOnWin(string text, float fontSize)
    {
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        Font font = new Font("Segoe UI Emoji", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        SizeF textSize = graphics.MeasureString(text, font);
        return textSize.Width;
    }
    
    private static byte[] RenderEmojiOnWindows(string text, float fontSize, Color color)
    {
        using var bitmap = new Bitmap(1, 1);
        using var graphics = Graphics.FromImage(bitmap);
        Font font = new Font("Segoe UI Emoji", fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
        SizeF textSize = graphics.MeasureString(text, font);
        using var textBitmap = new Bitmap((int)textSize.Width, (int)textSize.Height);
        using var textGraphics = Graphics.FromImage(textBitmap);
        textGraphics.Clear(System.Drawing.Color.Transparent);
        textGraphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        
        System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(color.ToARGB());

        using Brush brush = new SolidBrush(drawingColor);
        textGraphics.DrawString(text, font, brush, 0, 0);
        
        using var stream = new MemoryStream();
        textBitmap.Save(stream, ImageFormat.Png);
        return stream.ToArray();
    }
#endif

    public static float GetWidth(string text, float fontSize)
    {

#if !UNITY_EDITOR
    #if UNITY_ANDROID
        return EmojiRendererClass.CallStatic<float>("getWidth", text, fontSize);
    #elif UNITY_IOS
            imageBytes = renderEmoji(text, fontSize); //TODO: COLOR
    #endif
#endif
        
#if UNITY_EDITOR_WIN
        return GetWidthOnWin(text, fontSize);
#endif
    }
    
    public static List<string> GetGraphemeClustersAndroid(string text)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            var result = EmojiRendererClass.CallStatic<AndroidJavaObject>("getGraphemeClusters", text);
            var list = new List<string>();
            for (int i = 0; i < result.Call<int>("size"); i++)
            {
                list.Add(result.Call<string>("get", i));
            }
            return list;
        }
        return new List<string>();
    }
    
    public static List<string> GetGraphemeClusters(string text)
    {
#if !UNITY_EDITOR
    #if UNITY_ANDROID
        return GetGraphemeClustersAndroid(text);
    #elif UNITY_IOS
            imageBytes = renderEmoji(text, fontSize); //TODO: COLOR
    #endif
#endif
        
        List<string> clusters = new List<string>();
        StringInfo stringInfo = new StringInfo(text);

        for (int i = 0; i < stringInfo.LengthInTextElements; i++)
        {
            clusters.Add(stringInfo.SubstringByTextElements(i, 1));
        }

        return clusters;
    }
}
