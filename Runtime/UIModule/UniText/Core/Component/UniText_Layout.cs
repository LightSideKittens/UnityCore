using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;


public partial class UniText : ILayoutElement, ILayoutController
{
    private float cachedEffectiveFontSize;
    private float cachedPreferredWidth;
    private float cachedPreferredHeight;
    private float cachedLayoutWidth;
    private bool hasValidLayoutCache;

    #region ILayoutElement

    void ILayoutElement.CalculateLayoutInputHorizontal()
    {
        Profiler.BeginSample("UniText.CalculateLayoutInputHorizontal");

        cachedPreferredWidth = 0;

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

            var effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;
            cachedPreferredWidth = textProcessor.GetPreferredWidth(effectiveFontSize);
        }

        Profiler.EndSample();
    }

    void ILayoutElement.CalculateLayoutInputVertical()
    {
        Profiler.BeginSample("UniText.CalculateLayoutInputVertical");

        if (string.IsNullOrEmpty(text) || textProcessor == null || !textProcessor.HasValidFirstPassData)
        {
            hasValidLayoutCache = false;
            cachedPreferredHeight = 0;
            Profiler.EndSample();
            return;
        }

        var width = rectTransform.rect.width;
        if (width <= 0)
        {
            hasValidLayoutCache = false;
            cachedPreferredHeight = 0;
            Profiler.EndSample();
            return;
        }

        // Пропускаем перерасчёт если ширина не изменилась
        if (hasValidLayoutCache && Mathf.Approximately(cachedLayoutWidth, width))
        {
            Profiler.EndSample();
            return;
        }

        cachedEffectiveFontSize = GetEffectiveFontSize(width);
        cachedLayoutWidth = width;
        hasValidLayoutCache = true;

        textProcessor.EnsureLines(width, cachedEffectiveFontSize, enableWordWrap);

        // Кэшируем preferredHeight
        cachedPreferredHeight = (enableAutoSize && enableWordWrap)
            ? textProcessor.GetPreferredHeight(maxFontSize)
            : textProcessor.GetPreferredHeight(cachedEffectiveFontSize);

        Profiler.EndSample();
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

        // Без word wrap — ищем размер по ширине
        var settings = new TextProcessSettings
        {
            MaxWidth = width,
            MaxHeight = TextProcessSettings.FloatMax,
            fontSize = maxFontSize,
            baseDirection = baseDirection,
            enableWordWrap = false
        };

        return textProcessor.FindOptimalFontSize(
            minFontSize, maxFontSize, width, TextProcessSettings.FloatMax, settings);
    }

    private void InvalidateLayoutCache()
    {
        hasValidLayoutCache = false;
    }

    #endregion
}
