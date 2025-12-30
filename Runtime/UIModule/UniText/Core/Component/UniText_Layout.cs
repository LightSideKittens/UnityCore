using UnityEngine;
using UnityEngine.UI;


public partial class UniText : ILayoutElement, ILayoutController
{
    private float cachedEffectiveFontSize;
    private float cachedPreferredWidth;
    private float cachedPreferredHeight;
    private float cachedLayoutWidth;
    private float cachedLayoutHeight;
    private bool hasValidLayoutCache;

    #region ILayoutElement

    void ILayoutElement.CalculateLayoutInputHorizontal()
    {
        UniTextDebug.BeginSample("UniText.CalculateLayoutInputHorizontal");

        cachedPreferredWidth = 0;

        if (!string.IsNullOrEmpty(text) && textProcessor != null && textProcessor.HasValidFirstPassData)
        {
            var effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;
            cachedPreferredWidth = textProcessor.GetPreferredWidth(effectiveFontSize);
        }

        UniTextDebug.EndSample();
    }

    void ILayoutElement.CalculateLayoutInputVertical()
    {
        UniTextDebug.BeginSample("UniText.CalculateLayoutInputVertical");

        if (string.IsNullOrEmpty(text) || textProcessor == null || !textProcessor.HasValidFirstPassData)
        {
            hasValidLayoutCache = false;
            cachedPreferredHeight = 0;
            UniTextDebug.EndSample();
            return;
        }

        var rect = rectTransform.rect;
        if (rect.width <= 0)
        {
            hasValidLayoutCache = false;
            cachedPreferredHeight = 0;
            UniTextDebug.EndSample();
            return;
        }

        var height = (enableAutoSize && !enableWordWrap)
            ? TextProcessSettings.FloatMax
            : (rect.height > 0 ? rect.height : TextProcessSettings.FloatMax);

        if (hasValidLayoutCache &&
            Mathf.Approximately(cachedLayoutWidth, rect.width) &&
            ((enableAutoSize && !enableWordWrap) || Mathf.Approximately(cachedLayoutHeight, height)))
        {
            UniTextDebug.EndSample();
            return;
        }

        cachedEffectiveFontSize = GetEffectiveFontSize(rect.width, height);
        cachedLayoutWidth = rect.width;
        cachedLayoutHeight = height;
        hasValidLayoutCache = true;

        textProcessor.EnsureLines(rect.width, cachedEffectiveFontSize, enableWordWrap);

        cachedPreferredHeight = (enableAutoSize && enableWordWrap)
            ? textProcessor.GetPreferredHeight(maxFontSize)
            : textProcessor.GetPreferredHeight(cachedEffectiveFontSize);

        UniTextDebug.EndSample();
    }

    public float minWidth => 0;
    public float preferredWidth => cachedPreferredWidth;
    public float flexibleWidth => -1;

    public float minHeight => 0;
    public float preferredHeight => cachedPreferredHeight;
    public float flexibleHeight => -1;

    public int layoutPriority => 0;

    #endregion

    #region ILayoutController

    void ILayoutController.SetLayoutHorizontal() { }

    void ILayoutController.SetLayoutVertical()
    {
        if (!enableAutoSize) return;
        if (textProcessor == null || !textProcessor.HasValidFirstPassData) return;

        var rect = rectTransform.rect;
        if (rect.width <= 0 || rect.height <= 0) return;

        textProcessor.EnsureLines(rect.width, maxFontSize, enableWordWrap);
        var preferredH = textProcessor.GetPreferredHeight(maxFontSize);

        if (rect.height < preferredH - 0.01f)
        {
            var settings = new TextProcessSettings
            {
                MaxWidth = rect.width,
                MaxHeight = rect.height,
                fontSize = maxFontSize,
                baseDirection = baseDirection,
                enableWordWrap = enableWordWrap
            };

            cachedEffectiveFontSize = textProcessor.FindOptimalFontSize(
                minFontSize, maxFontSize, rect.width, rect.height, settings);
            textProcessor.EnsureLines(rect.width, cachedEffectiveFontSize, enableWordWrap);
        }
    }

    #endregion

    #region AutoSize

    private float GetEffectiveFontSize(float width, float height)
    {
        if (!enableAutoSize) return fontSize;
        if (enableWordWrap) return maxFontSize;

        var settings = new TextProcessSettings
        {
            MaxWidth = width,
            MaxHeight = height,
            fontSize = maxFontSize,
            baseDirection = baseDirection,
            enableWordWrap = false
        };

        return textProcessor.FindOptimalFontSize(
            minFontSize, maxFontSize, width, height, settings);
    }

    private void InvalidateLayoutCache()
    {
        hasValidLayoutCache = false;
    }

    #endregion
}
