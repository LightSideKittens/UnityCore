using System;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Настройки layout
/// </summary>
public struct LayoutSettings
{
    public float maxWidth;
    public float maxHeight;
    public float lineSpacing;
    public float defaultLineHeight;
    public HorizontalAlignment horizontalAlignment;
    public VerticalAlignment verticalAlignment;

    public static LayoutSettings Default => new()
    {
        maxWidth = TextProcessSettings.FloatMax,
        maxHeight = TextProcessSettings.FloatMax,
        lineSpacing = 0,
        defaultLineHeight = 20,
        horizontalAlignment = HorizontalAlignment.Left,
        verticalAlignment = VerticalAlignment.Top
    };
}

/// <summary>
/// Text Layout — позиционирование глифов.
/// Применяет alignment, line spacing, и т.д.
/// </summary>
public sealed class TextLayout
{
    // Settings
    private LayoutSettings settings;

    // Метрики шрифта для расчёта line height
    private float fontAscender;
    private float fontDescender;
    private float fontLineHeight;
    private float fontScale = 1f;
    private float glyphScale = 1f;

    public TextLayout()
    {
        settings = LayoutSettings.Default;
    }

    public TextLayout(LayoutSettings settings)
    {
        this.settings = settings;
    }

    /// <summary>
    /// Установить метрики шрифта для расчёта высоты строки.
    /// Должен быть вызван перед Layout().
    /// </summary>
    /// <param name="glyphScaleFactor">Коэффициент масштабирования для advance/offset глифов (currentFontSize / shapingFontSize)</param>
    public void SetFontMetrics(float ascender, float descender, float lineHeight, float scale, float glyphScaleFactor = 1f)
    {
        fontAscender = ascender;
        fontDescender = descender;
        fontLineHeight = lineHeight;
        fontScale = scale;
        glyphScale = glyphScaleFactor;
    }

    /// <summary>
    /// Установить настройки layout (размеры, выравнивание).
    /// </summary>
    public void SetLayoutSettings(LayoutSettings newSettings)
    {
        settings = newSettings;
    }

    /// <summary>
    /// Выполнить layout и записать результат в предоставленный буфер.
    /// </summary>
    public void Layout(
        ReadOnlySpan<TextLine> lines,
        ReadOnlySpan<ShapedRun> runs,
        ReadOnlySpan<ShapedGlyph> glyphs,
        PositionedGlyph[] result,
        ref int glyphCount,
        out float width,
        out float height)
    {
        glyphCount = 0;
        width = 0;
        height = 0;

        int lineCount = lines.Length;
        if (lineCount == 0)
            return;

        // fontLineHeight и fontAscender уже приходят с учётом scale из UniTextFontProvider.GetLineMetrics()
        float computedLineHeight = fontLineHeight;
        if (computedLineHeight <= 0)
            computedLineHeight = fontAscender - fontDescender;
        if (computedLineHeight <= 0)
            computedLineHeight = settings.defaultLineHeight;

        float ascender = fontAscender;
        if (ascender <= 0) ascender = computedLineHeight * 0.8f;

        float lineSpacing = settings.lineSpacing;
        float totalTextHeight = ascender + (lineCount - 1) * (computedLineHeight + lineSpacing);

        float y = ComputeTextStartY(totalTextHeight, settings) + ascender;
        float maxLineWidth = 0;

        // Cache settings for inner loops
        float availableWidth = settings.maxWidth;
        var hAlign = settings.horizontalAlignment;
        bool hasFiniteWidth = !float.IsInfinity(availableWidth) && availableWidth > 0;

        for (int i = 0; i < lineCount; i++)
        {
            ref readonly var line = ref lines[i];
            int runStart = line.runStart;
            int runCount = line.runCount;

            // Вычисляем реальную ширину строки (масштабируем если fontSize изменился)
            float lineWidth = 0;
            int runEnd = runStart + runCount;
            for (int r = runStart; r < runEnd; r++)
                lineWidth += runs[r].width * glyphScale;

            // Начальная X позиция
            float x;
            bool isRtlLine = (line.paragraphBaseLevel & 1) == 1;
            if (hasFiniteWidth)
            {
                x = ComputeLineStartX(lineWidth, isRtlLine, availableWidth, hAlign);
            }
            else
            {
                x = 0;
            }

            // Apply startMargin for hanging indent (lists, etc.)
            // Margin создаёт "зону" слева (LTR) или справа (RTL) от текста.
            // LineBreaker уже учёл margin при расчёте line breaking (effectiveMaxWidth = maxWidth - margin).
            // Здесь корректируем x для правильного визуального позиционирования.
            if (line.startMargin > 0 && hasFiniteWidth)
            {
                float margin = line.startMargin;
                if (isRtlLine)
                {
                    // RTL: start margin означает отступ справа (начало RTL текста)
                    if (hAlign == HorizontalAlignment.Left)
                    {
                        // RTL Left: текст слева, margin справа → сдвигаем текст влево
                        x -= margin;
                    }
                    else if (hAlign == HorizontalAlignment.Center)
                    {
                        // Центрируем в зоне [0, width - margin]
                        x = (availableWidth - margin - lineWidth) * 0.5f;
                    }
                    // Right: текст у правого края → margin уже учтён в line breaking
                }
                else
                {
                    // LTR: start margin означает отступ слева
                    if (hAlign == HorizontalAlignment.Left)
                    {
                        // LTR Left: текст начинается после margin
                        x += margin;
                    }
                    else if (hAlign == HorizontalAlignment.Center)
                    {
                        // Центрируем в зоне [margin, width]
                        x = margin + (availableWidth - margin - lineWidth) * 0.5f;
                    }
                    // Right: margin слева (пустая зона) → x не меняем
                }
            }

            // Позиционируем каждый run в строке
            for (int r = runStart; r < runEnd; r++)
            {
                ref readonly var run = ref runs[r];
                int glyphStart = run.glyphStart;
                int glyphLen = run.glyphCount;

                // Defensive check: verify glyph indices are within bounds (no string allocation)
                Debug.Assert(glyphStart >= 0 && glyphStart + glyphLen <= glyphs.Length);

                int fontId = run.fontId;
                int glyphEnd = glyphStart + glyphLen;

                // Convert run-local cluster to global codepoint index
                int clusterOffset = run.range.start;

                for (int g = glyphStart; g < glyphEnd; g++)
                {
                    ref readonly var glyph = ref glyphs[g];
                    result[glyphCount++] = new PositionedGlyph
                    {
                        glyphId = glyph.glyphId,
                        cluster = glyph.cluster + clusterOffset,
                        x = x + glyph.offsetX * glyphScale,
                        // HarfBuzz uses typographic coordinates (Y up), but our layout uses Y down
                        // So we need to subtract offsetY to move diacritics up
                        y = y - glyph.offsetY * glyphScale,
                        fontId = fontId
                    };
                    x += glyph.advanceX * glyphScale;
                }
            }

            if (lineWidth > maxLineWidth)
                maxLineWidth = lineWidth;

            y += computedLineHeight + lineSpacing;
        }

        width = maxLineWidth;
        height = totalTextHeight;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ComputeLineStartX(float lineWidth, bool isRtlLine, float availableWidth, HorizontalAlignment alignment)
    {
        return alignment switch
        {
            HorizontalAlignment.Left => isRtlLine ? availableWidth - lineWidth : 0,
            HorizontalAlignment.Right => isRtlLine ? 0 : availableWidth - lineWidth,
            _ => (availableWidth - lineWidth) * 0.5f
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float ComputeTextStartY(float totalTextHeight, LayoutSettings settings)
    {
        float availableHeight = settings.maxHeight;
        if (float.IsInfinity(availableHeight) || availableHeight <= 0)
            return 0;

        return settings.verticalAlignment switch
        {
            VerticalAlignment.Middle => (availableHeight - totalTextHeight) * 0.5f,
            VerticalAlignment.Bottom => availableHeight - totalTextHeight,
            _ => 0
        };
    }
}
