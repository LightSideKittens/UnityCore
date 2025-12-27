using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;


public partial class UniText : ILayoutElement, ILayoutController
{
    private float cachedEffectiveFontSize;
    private float cachedAutoSizeWidth;
    private bool hasValidLayoutCache;
    private bool hasValidAutoSizeCache;

    #region ILayoutElement

    void ILayoutElement.CalculateLayoutInputHorizontal()
    {
        Profiler.BeginSample("UniText.CalculateLayoutInputHorizontal");

        if (!string.IsNullOrEmpty(text) && ValidateAndInitialize())
        {
            if (!textProcessor.HasValidFirstPassData)
            {
                var textSpan = ParseOrGetParsedAttributes();
                var shapingFontSize = enableAutoSize ? maxFontSize : fontSize;
                var settings = new TextProcessSettings
                {
                    fontSize = shapingFontSize,
                    baseDirection = baseDirection
                };
                textProcessor.EnsureFirstPass(textSpan, settings);
            }
        }

        Profiler.EndSample();
    }

    void ILayoutElement.CalculateLayoutInputVertical()
    {
        Profiler.BeginSample("UniText.CalculateLayoutInputVertical");

        if (string.IsNullOrEmpty(text) || textProcessor == null || !textProcessor.HasValidFirstPassData)
        {
            hasValidLayoutCache = false;
            Profiler.EndSample();
            return;
        }

        var rect = rectTransform.rect;
        if (rect.width <= 0)
        {
            hasValidLayoutCache = false;
            Profiler.EndSample();
            return;
        }

        cachedEffectiveFontSize = GetEffectiveFontSize(rect.width);
        hasValidLayoutCache = true;

        textProcessor.EnsureLines(rect.width, cachedEffectiveFontSize, enableWordWrap);

        Profiler.EndSample();
    }

    public float minWidth => 0;

    public float preferredWidth
    {
        get
        {
            if (textProcessor == null || !textProcessor.HasValidFirstPassData) return 0;

            var effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;
            return textProcessor.GetPreferredWidth(effectiveFontSize);
        }
    }

    public float flexibleWidth => -1;

    public float minHeight => 0;

    public float preferredHeight
    {
        get
        {
            if (textProcessor == null || !textProcessor.HasValidFirstPassData) return 0;
            if (!hasValidLayoutCache) return 0;

            if (enableAutoSize && enableWordWrap)
                return textProcessor.GetPreferredHeight(maxFontSize);

            return textProcessor.GetPreferredHeight(cachedEffectiveFontSize);
        }
    }

    public float flexibleHeight => -1;

    public int layoutPriority => 0;

    #endregion

    #region ILayoutController

    void ILayoutController.SetLayoutHorizontal() { }

    void ILayoutController.SetLayoutVertical()
    {
        if (!enableAutoSize || !enableWordWrap) return;
        if (textProcessor == null || !textProcessor.HasValidFirstPassData) return;

        var rect = rectTransform.rect;
        if (rect.width <= 0 || rect.height <= 0) return;

        textProcessor.EnsureLines(rect.width, maxFontSize, true);
        var preferredH = textProcessor.GetPreferredHeight(maxFontSize);

        if (rect.height < preferredH - 0.01f)
        {
            var settings = new TextProcessSettings
            {
                MaxWidth = rect.width,
                MaxHeight = rect.height,
                fontSize = maxFontSize,
                baseDirection = baseDirection,
                enableWordWrap = true
            };

            cachedEffectiveFontSize = textProcessor.FindOptimalFontSize(
                minFontSize, maxFontSize, rect.width, rect.height, settings);
            textProcessor.EnsureLines(rect.width, cachedEffectiveFontSize, true);
        }
    }

    #endregion

    #region AutoSize

    private float GetEffectiveFontSize(float width)
    {
        if (!enableAutoSize) return fontSize;
        if (enableWordWrap) return maxFontSize;

        if (hasValidAutoSizeCache && Mathf.Approximately(cachedAutoSizeWidth, width))
            return cachedEffectiveFontSize;

        var settings = new TextProcessSettings
        {
            MaxWidth = width,
            MaxHeight = TextProcessSettings.FloatMax,
            fontSize = maxFontSize,
            baseDirection = baseDirection,
            enableWordWrap = false
        };

        var result = textProcessor.FindOptimalFontSize(
            minFontSize, maxFontSize, width, TextProcessSettings.FloatMax, settings);
        cachedAutoSizeWidth = width;
        hasValidAutoSizeCache = true;

        return result;
    }

    private void InvalidateLayoutCache()
    {
        hasValidLayoutCache = false;
        hasValidAutoSizeCache = false;
    }

    #endregion
}
