using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;


public partial class UniText : ILayoutElement
{
    private float cachedPreferredWidth;
    private float cachedPreferredHeight;
    private float cachedPreferredHeightForWidth;
    private bool hasValidPreferredWidth;
    private bool hasValidPreferredHeight;
    private float layoutCachedFontSize;
    private float layoutCachedWidth;
    private bool hasValidLayoutCache;
    
    void ILayoutElement.CalculateLayoutInputHorizontal()
    {
        Profiler.BeginSample("UniText.CalculateLayoutInputHorizontal");
        EnsureShapingForLayout();
        Profiler.EndSample();
    }

    void ILayoutElement.CalculateLayoutInputVertical() { }

    public float minWidth => 0;

    public float preferredWidth
    {
        get
        {
            if (string.IsNullOrEmpty(text)) return 0;
            if (textProcessor == null) return 0;

            if (hasValidPreferredWidth) return cachedPreferredWidth;

            Profiler.BeginSample("UniText.preferredWidth");

            EnsureShapingForLayout();

            var glyphScale = GetGlyphScaleForLayout();
            cachedPreferredWidth = Mathf.Ceil(textProcessor.GetMaxLineWidth() * glyphScale);
            hasValidPreferredWidth = true;

            Profiler.EndSample();
            return cachedPreferredWidth;
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

            if (hasValidPreferredHeight && Mathf.Approximately(cachedPreferredHeightForWidth, width))
                return cachedPreferredHeight;

            Profiler.BeginSample("UniText.preferredHeight");

            EnsureShapingForLayout();

            float effectiveFontSize;
            if (enableAutoSize && !enableWordWrap)
            {
                if (hasValidLayoutCache && Mathf.Approximately(layoutCachedWidth, width))
                {
                    effectiveFontSize = layoutCachedFontSize;
                }
                else
                {
                    var tempSettings = CreateProcessSettingsForLayout(width);
                    effectiveFontSize = textProcessor.FindOptimalFontSize(minFontSize, maxFontSize, width,
                        TextProcessSettings.FloatMax, tempSettings);

                    layoutCachedFontSize = effectiveFontSize;
                    layoutCachedWidth = width;
                    hasValidLayoutCache = true;
                }
            }
            else
            {
                effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;
            }

            var settings = new TextProcessSettings
            {
                MaxWidth = width,
                MaxHeight = TextProcessSettings.FloatMax,
                fontSize = effectiveFontSize,
                baseDirection = baseDirection,
                enableWordWrap = enableWordWrap,
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment
            };

            cachedPreferredHeight = textProcessor.GetHeightForWidth(width, settings);
            cachedPreferredHeightForWidth = width;
            hasValidPreferredHeight = true;

            Profiler.EndSample();
            return cachedPreferredHeight;
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
        var settings = CreateProcessSettingsForLayout(TextProcessSettings.FloatMax);
        textProcessor.EnsureShaping(textSpan, settings);

        Profiler.EndSample();
    }


    private TextProcessSettings CreateProcessSettingsForLayout(float width)
    {
        var effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;

        return new TextProcessSettings
        {
            MaxWidth = width,
            MaxHeight = TextProcessSettings.FloatMax,
            fontSize = effectiveFontSize,
            baseDirection = baseDirection,
            enableWordWrap = enableWordWrap,
            HorizontalAlignment = horizontalAlignment,
            VerticalAlignment = verticalAlignment
        };
    }


    private float GetGlyphScaleForLayout()
    {
        var buf = buffers;
        var targetFontSize = enableAutoSize ? maxFontSize : fontSize;
        return buf.shapingFontSize > 0 ? targetFontSize / buf.shapingFontSize : 1f;
    }
}