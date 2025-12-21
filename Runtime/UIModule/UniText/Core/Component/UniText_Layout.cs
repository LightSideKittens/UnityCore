using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;


public partial class UniText : ILayoutElement
{
    private float autoSizedFontSizeForLayout;
    private float autoSizeWidthCache;
    private bool hasValidAutoSizeForLayout;

    void ILayoutElement.CalculateLayoutInputHorizontal()
    {
        Profiler.BeginSample("UniText.CalculateLayoutInputHorizontal");
        EnsureShapingForLayout();
        Profiler.EndSample();
    }

    void ILayoutElement.CalculateLayoutInputVertical()
    {
        Profiler.BeginSample("UniText.CalculateLayoutInputVertical");

        if (string.IsNullOrEmpty(text) || textProcessor == null)
        {
            Profiler.EndSample();
            return;
        }

        var width = rectTransform.rect.width;
        if (width <= 0)
        {
            Profiler.EndSample();
            return;
        }

        var effectiveFontSize = GetEffectiveFontSizeForLayout(width);
        textProcessor.EnsureLines(width, effectiveFontSize, enableWordWrap);

        Profiler.EndSample();
    }

    public float minWidth => 0;

    public float preferredWidth
    {
        get
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (textProcessor == null) return 0;

            Profiler.BeginSample("UniText.preferredWidth");

            EnsureShapingForLayout();
            var effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;
            var result = textProcessor.GetPreferredWidth(effectiveFontSize);

            Profiler.EndSample();
            return result;
        }
    }

    public float flexibleWidth => -1;

    public float minHeight => 0;

    public float preferredHeight
    {
        get
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (textProcessor == null) return 0;

            var width = rectTransform.rect.width;
            if (width <= 0) return 0;

            Profiler.BeginSample("UniText.preferredHeight");

            EnsureShapingForLayout();
            var effectiveFontSize = GetEffectiveFontSizeForLayout(width);
            textProcessor.EnsureLines(width, effectiveFontSize, enableWordWrap);
            var result = textProcessor.GetPreferredHeight(effectiveFontSize);

            Profiler.EndSample();
            return result;
        }
    }

    public float flexibleHeight => -1;

    public int layoutPriority => 0;


    private void EnsureShapingForLayout()
    {
        if (string.IsNullOrEmpty(text)) return;
        if (!ValidateAndInitialize()) return;
        if (textProcessor.HasValidShapingData) return;

        Profiler.BeginSample("UniText.EnsureShapingForLayout");

        var textSpan = ParseOrGetParsedAttributes();
        var effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;
        var settings = new TextProcessSettings
        {
            fontSize = effectiveFontSize,
            baseDirection = baseDirection
        };

        textProcessor.EnsureShaping(textSpan, settings);

        Profiler.EndSample();
    }


    private float GetEffectiveFontSizeForLayout(float width)
    {
        if (!enableAutoSize) return fontSize;

        if (hasValidAutoSizeForLayout && Mathf.Approximately(autoSizeWidthCache, width))
            return autoSizedFontSizeForLayout;

        if (!enableWordWrap)
        {
            var settings = new TextProcessSettings
            {
                MaxWidth = width,
                MaxHeight = TextProcessSettings.FloatMax,
                fontSize = maxFontSize,
                baseDirection = baseDirection,
                enableWordWrap = false
            };

            autoSizedFontSizeForLayout = textProcessor.FindOptimalFontSize(
                minFontSize, maxFontSize, width, TextProcessSettings.FloatMax, settings);
        }
        else
        {
            autoSizedFontSizeForLayout = maxFontSize;
        }

        autoSizeWidthCache = width;
        hasValidAutoSizeForLayout = true;

        return autoSizedFontSizeForLayout;
    }


    private void InvalidateAutoSizeCache()
    {
        hasValidAutoSizeForLayout = false;
    }
}
