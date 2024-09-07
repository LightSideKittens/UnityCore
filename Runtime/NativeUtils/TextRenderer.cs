using LSCore.Extensions.Unity;
using LSCore.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using View;
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

    public static (int width, int height) GetSize(NativeTextMeshPro text)
    {
        RectTransform rectTransform = text.rectTransform;
        float w = rectTransform.rect.width;
        float h = rectTransform.rect.height;
        Vector4 m = text.margin;
        
        w -= m.x;
        w -= m.z;
        h -= m.y;
        h -= m.w;
        
        int width = Mathf.RoundToInt(w);
        int height = Mathf.RoundToInt(h);
        return (width, height);
    }
    
    public static void SetRect(NativeTextMeshPro text)
    {
        var (width, height) = GetSize(text);
        Debug.Log($"SetRect: {(width, height)}");
        TextRendererClass.CallStatic("setRect", width, height);
    }
    
    public static Vector3 GetLossyScaleRelativeToRoot(Transform transform)
    {
        Transform current = transform;
        Vector3 relativeScale = transform.localScale; // начальный масштаб (локальный)

        while (current.parent != null) // пока не дойдем до корневого родителя
        {
            current = current.parent;
            relativeScale = Vector3.Scale(relativeScale, current.localScale); // масштабируем на каждый родительский scale
        }

        return relativeScale;
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
    
    public static bool IsValidString(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }
        
        foreach (var c in text)
        {
            if (c != '\u200B')
            {
                return true; 
            }
        }
        
        return false;
    }
    
    public static RawImage ConvertToNative(NativeTextMeshPro text)
    {
        if (!IsValidString(text.text))
        {
            return null;
        }
        
        const string imageGoName = "native-image";
        
        var imageTransform = text.transform.Find(imageGoName);

        GameObject imageGo;
        RawImage rawImage;
        
        if (imageTransform == null)
        {
            imageGo = new GameObject(imageGoName);
            imageGo.hideFlags = HideFlags.HideAndDontSave;
            rawImage = imageGo.AddComponent<RawImage>();
        }
        else
        {
            imageGo = imageTransform.gameObject;
            imageGo.hideFlags = HideFlags.HideAndDontSave;
            rawImage = imageGo.GetComponent<RawImage>();
            if (rawImage.texture != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    Object.DestroyImmediate(rawImage.texture);
                }
                else
                {
                    Object.Destroy(rawImage.texture);
                }
#else
                Object.Destroy(rawImage.texture);
#endif
            }
        }
        
        var texture = RenderText(text);
        rawImage.texture = texture;
        rawImage.transform.SetParent(text.transform, false);
        
        var pivot = text.rectTransform.pivot;
        var pos = Vector2.zero;
        var m = text.margin;
        
        switch (text.verticalAlignment)
        {
            case VerticalAlignmentOptions.Top:
                pivot.y = 1;
                pos.y -= m.y;
                break;
            case VerticalAlignmentOptions.Middle:
                pivot.y = 0.5f;
                break;
            case VerticalAlignmentOptions.Bottom:
                pivot.y = 0;
                pos.y += m.w;
                break;
        }

        switch (text.horizontalAlignment)
        {
            case HorizontalAlignmentOptions.Right:
                pivot.x = 1;
                pos.x -= m.z;
                break;
            case HorizontalAlignmentOptions.Center:
                pivot.x = 0.5f;
                break;
            case HorizontalAlignmentOptions.Left:
                pivot.x = 0;
                pos.x += m.x;
                break;
        }
        
        var rawImageRect = rawImage.rectTransform;
        rawImageRect.anchorMin = pivot;
        rawImageRect.anchorMax = pivot;
        rawImageRect.anchoredPosition = pos;
        rawImageRect.sizeDelta = texture.Size();
        rawImageRect.SetPivot(pivot);
        return rawImage;
    }
    
    public static Texture2D RenderText(NativeTextMeshPro textComp)
    {
        byte[] imageBytes = null;
        string fontName = textComp.font.name.ToLower();
        
#if !UNITY_EDITOR
    #if UNITY_ANDROID
        string text = textComp.text;
        float fontSize = textComp.fontSize;
        Color color = textComp.color;

        // Установка шрифта, размера текста и цвета текста
        SetCustomFont(fontName);
        SetCustomTextSize(fontSize);
        SetCustomTextColor(color);
        SetRect(textComp);
        SetAlignment(textComp.alignment);
        SetWrapText(textComp.textWrappingMode == TextWrappingModes.Normal);
        SetOverflow(textComp.overflowMode);
        
        Material mat = textComp.fontMaterial;
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
            var (width, height) = GetSize(textComp);
                texture = Texture2DExtensions.GetTextureByColor(new Color(0.5f, 0.5f, 0.5f, 0.5f), width, height);
            return texture;
        }
        
        texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);
        return texture;
    }
}

