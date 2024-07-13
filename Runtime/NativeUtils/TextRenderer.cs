using LSCore.Runtime;
using TMPro;
using UnityEngine;
using Color = UnityEngine.Color;

public static class TextRenderer
{
#if UNITY_ANDROID
    private static string packageName = "com.lscore.textrenderer";
    private static AndroidJavaClass textRenderer;
    private static AndroidJavaObject currentActivity;
    private static AndroidJavaClass TextRendererClass => textRenderer ??= new AndroidJavaClass($"{packageName}.TextRenderer");
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
#endif
    
    public static void SetCustomFont(string fontName)
    {
        Debug.Log($"SetCustomFont: {fontName}");
        TextRendererClass.CallStatic("setCustomFont", CurrentActivity, fontName);
    }

    public static void SetCustomTextSize(float textSize)
    {
        Debug.Log($"SetCustomTextSize: {textSize}");
        TextRendererClass.CallStatic("setCustomTextSize", textSize);
    }

    public static void SetCustomTextColor(Color textColor)
    {
        Debug.Log($"SetCustomTextColor: {textColor}");
        TextRendererClass.CallStatic("setCustomTextColor", textColor.ToARGB());
    }
    
    public static void SetRect(RectTransform rectTransform)
    {
        int width = Mathf.RoundToInt(rectTransform.rect.width);
        int height = Mathf.RoundToInt(rectTransform.rect.height);
        Debug.Log($"SetRect: {(width, height)}");
        TextRendererClass.CallStatic("setRect", width, height);
    }

    public static void SetAlignment(TextAlignmentOptions alignmentOptions)
    {
        Debug.Log($"SetAlignment: {alignmentOptions}");
        int alignment = 0;
        switch (alignmentOptions)
        {
            case TextAlignmentOptions.Left:
                alignment = 0;
                break;
            case TextAlignmentOptions.Right:
                alignment = 1;
                break;
            case TextAlignmentOptions.Center:
                alignment = 2;
                break;
            // Добавьте другие опции выравнивания, если это необходимо
        }
        TextRendererClass.CallStatic("setAlignment", alignment);
    }
    
    
    public static void SetWrapText(bool wrapText)
    {
        Debug.Log($"SetWrapText: {wrapText}");
        TextRendererClass.CallStatic("setWrapText", wrapText);
    }

    public static void SetOverflow(TextOverflowModes overflowMode)
    {
        Debug.Log($"SerOverflow: {overflowMode}");
        int overflow = 0;
        switch (overflowMode)
        {
            case TextOverflowModes.Overflow:
                overflow = 0;
                break;
            case TextOverflowModes.Ellipsis:
                overflow = 1;
                break;
            case TextOverflowModes.Masking:
                overflow = 2;
                break;
            case TextOverflowModes.Truncate:
                overflow = 3;
                break;
            case TextOverflowModes.ScrollRect:
                overflow = 4;
                break;
            // Добавьте другие опции переполнения, если это необходимо
        }
        TextRendererClass.CallStatic("setOverflow", overflow);
    }
    
    public static void SetFaceDilate(float dilate)
    {
        TextRendererClass.CallStatic("setFaceDilate", dilate);
    }
    
    public static void SetStroke(float width, Color color)
    {
        Debug.Log($"SetStroke: {(width, color)}");
        TextRendererClass.CallStatic("setStroke", width, color.ToARGB());
    }


    public static void SetUnderlay(Color color, float offsetX, float offsetY, float dilate, float softness)
    {
        Debug.Log($"SetUnderlay: {(color, offsetX, offsetY, dilate, softness)}");
        TextRendererClass.CallStatic("setUnderlay", color.ToARGB(), offsetX, offsetY, dilate, softness);
    }
    
    public static Texture2D RenderText(TextMeshProUGUI textMeshProUGUI)
    {
        byte[] imageBytes = null;

        Debug.Log(textMeshProUGUI.font.name);
        
#if !UNITY_EDITOR
    #if UNITY_ANDROID
        string text = textMeshProUGUI.text;
        float fontSize = textMeshProUGUI.fontSize;
        Color color = textMeshProUGUI.color;
        string fontName = textMeshProUGUI.font.name;

        // Установка шрифта, размера текста и цвета текста
        SetCustomFont(fontName);
        SetCustomTextSize(fontSize);
        SetCustomTextColor(color);
        SetRect(textMeshProUGUI.rectTransform);
        SetAlignment(textMeshProUGUI.alignment);
        SetWrapText(textMeshProUGUI.textWrappingMode == TextWrappingModes.Normal);
        SetOverflow(textMeshProUGUI.overflowMode);
        
        Material mat = textMeshProUGUI.fontMaterial;
        if (mat.HasProperty(ShaderUtilities.ID_OutlineWidth))
        {
            float outlineWidth = mat.GetFloat(ShaderUtilities.ID_OutlineWidth);
            Color outlineColor = mat.GetColor(ShaderUtilities.ID_OutlineColor);
            SetStroke(outlineWidth, outlineColor);
        }
        
        if (mat.HasProperty(ShaderUtilities.ID_UnderlayColor))
        {
            Color underlayColor = mat.GetColor(ShaderUtilities.ID_UnderlayColor);
            float underlayOffsetX = mat.GetFloat(ShaderUtilities.ID_UnderlayOffsetX);
            float underlayOffsetY = mat.GetFloat(ShaderUtilities.ID_UnderlayOffsetY);
            float underlayDilate = mat.GetFloat(ShaderUtilities.ID_UnderlayDilate);
            float underlaySoftness = mat.GetFloat(ShaderUtilities.ID_UnderlaySoftness);
            SetUnderlay(underlayColor, underlayOffsetX, underlayOffsetY, underlayDilate, underlaySoftness);
        }
        
        if (mat.HasProperty(ShaderUtilities.ID_FaceDilate))
        {
            float faceDilate = mat.GetFloat(ShaderUtilities.ID_FaceDilate);
            SetFaceDilate(faceDilate);
        }

        imageBytes = TextRendererClass.CallStatic<byte[]>("render", text);
    #endif
#endif

        Texture2D texture = null;
        if (imageBytes == null)
        {
            Debug.LogError("Failed to render text");
            texture = Texture2D.whiteTexture;
            return texture;
        }
        
        texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        return texture;
    }
}
