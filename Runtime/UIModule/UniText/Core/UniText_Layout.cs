using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ILayoutElement implementation for UniText.
/// Allows UniText to work with LayoutGroup and ContentSizeFitter.
/// </summary>
public partial class UniText : ILayoutElement
{
    public void CalculateLayoutInputHorizontal()
    {
        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log("[CalculateLayoutInputHorizontal]");

        // Unity calls this before GetPreferredWidth
        // We ensure shaping is done here for efficiency
        EnsureShapingForLayout();
    }

    public void CalculateLayoutInputVertical()
    {
        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log("[CalculateLayoutInputVertical]");

        // Unity calls this before GetPreferredHeight
        // Width is already set at this point, shaping already done
    }

    public float minWidth => 0;

    public float preferredWidth
    {
        get
        {
            if (CommonData.DebugPipelineLogging)
                UnityEngine.Debug.Log("[preferredWidth] getter");

            if (string.IsNullOrEmpty(text)) return 0;
            if (processor == null) return 0;

            EnsureShapingForLayout();

            float glyphScale = GetGlyphScaleForLayout();
            return processor.GetUnwrappedWidth() * glyphScale;
        }
    }

    public float flexibleWidth => -1;

    public float minHeight => 0;

    public float preferredHeight
    {
        get
        {
            if (CommonData.DebugPipelineLogging)
                UnityEngine.Debug.Log("[preferredHeight] getter");

            if (string.IsNullOrEmpty(text)) return 0;
            if (processor == null) return 0;

            // At this point Unity has already set rect.width via SetLayoutHorizontal
            float width = rectTransform.rect.width;
            if (width <= 0) return 0;

            EnsureShapingForLayout();

            var settings = CreateProcessSettingsForLayout(width);
            return processor.GetHeightForWidth(width, settings);
        }
    }

    public float flexibleHeight => -1;

    public int layoutPriority => 0;

    /// <summary>
    /// Ensure shaping data exists for layout calculations.
    /// </summary>
    private void EnsureShapingForLayout()
    {
        if (CommonData.DebugPipelineLogging)
            UnityEngine.Debug.Log("[EnsureShapingForLayout]");

        if (string.IsNullOrEmpty(text)) return;

        // Ensure components are initialized
        EnsureInitialized();

        if (processor == null || fontProvider == null) return;

        // Set CommonData.Current to our textBuffers (normally set in Rebuild)
        CommonData.Current = textBuffers;

        if (processor.HasValidShapingData) return;

        // Use same text processing as Rebuild - parse tags first
        if (!textIsParsed)
        {
            parser?.ResetModifiers();
            parser?.Parse(text);
            textIsParsed = true;
        }

        // Get text span: from parser if available, otherwise original text
        var textSpan = parser != null ? parser.CleanTextSpan : text.AsSpan();

        var settings = CreateProcessSettingsForLayout(TextProcessSettings.FloatMax);
        processor.EnsureShaping(textSpan, settings);
    }

    /// <summary>
    /// Create settings for layout calculations.
    /// Uses maxFontSize when auto size is enabled for preferred size calculation.
    /// </summary>
    private TextProcessSettings CreateProcessSettingsForLayout(float width)
    {
        // For auto size, use maxFontSize to calculate preferred dimensions
        float effectiveFontSize = enableAutoSize ? maxFontSize : fontSize;

        return new TextProcessSettings
        {
            maxWidth = width,
            maxHeight = TextProcessSettings.FloatMax,
            fontSize = effectiveFontSize,
            baseDirection = baseDirection,
            enableWordWrap = enableWordWrap,
            horizontalAlignment = horizontalAlignment,
            verticalAlignment = verticalAlignment
        };
    }

    /// <summary>
    /// Get glyph scale for layout calculations.
    /// Uses maxFontSize when auto size is enabled.
    /// </summary>
    private float GetGlyphScaleForLayout()
    {
        var buf = CommonData.Current;
        float targetFontSize = enableAutoSize ? maxFontSize : fontSize;
        return buf.shapingFontSize > 0 ? targetFontSize / buf.shapingFontSize : 1f;
    }
}