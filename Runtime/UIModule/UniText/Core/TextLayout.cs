using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

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
        maxWidth = float.MaxValue,
        maxHeight = float.MaxValue,
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
    public void SetFontMetrics(float ascender, float descender, float lineHeight, float scale)
    {
        fontAscender = ascender;
        fontDescender = descender;
        fontLineHeight = lineHeight;
        fontScale = scale;
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
            var line = lines[i];
            int runStart = line.runStart;
            int runCount = line.runCount;

            // Вычисляем реальную ширину строки
            float lineWidth = 0;
            for (int r = 0; r < runCount; r++)
                lineWidth += runs[runStart + r].width;

            // Начальная X позиция
            float x;
            if (hasFiniteWidth)
            {
                bool isRtlLine = (line.paragraphBaseLevel & 1) == 1;
                x = ComputeLineStartXFast(lineWidth, isRtlLine, availableWidth, hAlign);
            }
            else
            {
                x = 0;
            }

            // Позиционируем каждый run в строке
            for (int r = 0; r < runCount; r++)
            {
                var run = runs[runStart + r];
                int glyphStart = run.glyphStart;
                int glyphLen = run.glyphCount;

                // Defensive check: verify glyph indices are within bounds
                Debug.Assert(glyphStart >= 0 && glyphStart + glyphLen <= glyphs.Length,
                    $"Invalid glyph range: start={glyphStart}, count={glyphLen}, glyphs.Length={glyphs.Length}");

                int fontId = run.fontId;
                int attrSnapshot = run.attributeSnapshot;
                float currentY = y; // Cache y value

                if (run.direction == TextDirection.RightToLeft)
                {
                    // RTL: глифы внутри run идут справа налево
                    float runEndX = x + run.width;

                    for (int g = 0; g < glyphLen; g++)
                    {
                        var glyph = glyphs[glyphStart + g];
                        runEndX -= glyph.advanceX;

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = glyph.glyphId,
                            x = runEndX + glyph.offsetX,
                            y = currentY + glyph.offsetY,
                            fontId = fontId,
                            attributeSnapshot = attrSnapshot
                        };
                    }

                    x += run.width;
                }
                else
                {
                    // LTR - unrolled for common case of many glyphs
                    int g = 0;
                    int glyphEnd = glyphStart + glyphLen;

                    // Process 4 glyphs at a time for better cache performance
                    for (; g + 3 < glyphLen; g += 4)
                    {
                        var g0 = glyphs[glyphStart + g];
                        var g1 = glyphs[glyphStart + g + 1];
                        var g2 = glyphs[glyphStart + g + 2];
                        var g3 = glyphs[glyphStart + g + 3];

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = g0.glyphId,
                            x = x + g0.offsetX,
                            y = currentY + g0.offsetY,
                            fontId = fontId,
                            attributeSnapshot = attrSnapshot
                        };
                        x += g0.advanceX;

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = g1.glyphId,
                            x = x + g1.offsetX,
                            y = currentY + g1.offsetY,
                            fontId = fontId,
                            attributeSnapshot = attrSnapshot
                        };
                        x += g1.advanceX;

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = g2.glyphId,
                            x = x + g2.offsetX,
                            y = currentY + g2.offsetY,
                            fontId = fontId,
                            attributeSnapshot = attrSnapshot
                        };
                        x += g2.advanceX;

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = g3.glyphId,
                            x = x + g3.offsetX,
                            y = currentY + g3.offsetY,
                            fontId = fontId,
                            attributeSnapshot = attrSnapshot
                        };
                        x += g3.advanceX;
                    }

                    // Process remaining glyphs
                    for (; g < glyphLen; g++)
                    {
                        var glyph = glyphs[glyphStart + g];

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = glyph.glyphId,
                            x = x + glyph.offsetX,
                            y = currentY + glyph.offsetY,
                            fontId = fontId,
                            attributeSnapshot = attrSnapshot
                        };

                        x += glyph.advanceX;
                    }
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
    private static float ComputeLineStartXFast(float lineWidth, bool isRtlLine, float availableWidth, HorizontalAlignment alignment)
    {
        switch (alignment)
        {
            case HorizontalAlignment.Left:
                return isRtlLine ? availableWidth - lineWidth : 0;

            case HorizontalAlignment.Right:
                return isRtlLine ? 0 : availableWidth - lineWidth;

            case HorizontalAlignment.Center:
            default:
                return (availableWidth - lineWidth) * 0.5f;
        }
    }

    /// <summary>
    /// Вычислить начальную X позицию строки.
    /// Per-line alignment: каждая строка выравнивается по своему направлению.
    /// </summary>
    private static float ComputeLineStartX(float lineWidth, TextDirection lineDirection, LayoutSettings settings)
    {
        float availableWidth = settings.maxWidth;
        if (float.IsInfinity(availableWidth) || availableWidth <= 0)
        {
            return 0;
        }

        var alignment = settings.horizontalAlignment;

        switch (alignment)
        {
            case HorizontalAlignment.Left:
                // "Left" = естественное начало строки
                // LTR строка → прижата влево
                // RTL строка → прижата вправо
                return lineDirection == TextDirection.RightToLeft
                    ? availableWidth - lineWidth
                    : 0;

            case HorizontalAlignment.Right:
                // "Right" = противоположный край
                // LTR строка → прижата вправо
                // RTL строка → прижата влево
                return lineDirection == TextDirection.RightToLeft
                    ? 0
                    : availableWidth - lineWidth;

            case HorizontalAlignment.Center:
            default:
                return (availableWidth - lineWidth) / 2;
        }
    }

    /// <summary>
    /// Вычислить начальную Y позицию текста (vertical alignment)
    /// </summary>
    private static float ComputeTextStartY(float totalTextHeight, LayoutSettings settings)
    {
        float availableHeight = settings.maxHeight;
        if (float.IsInfinity(availableHeight) || availableHeight <= 0)
            return 0;

        switch (settings.verticalAlignment)
        {
            case VerticalAlignment.Middle:
                return (availableHeight - totalTextHeight) / 2;

            case VerticalAlignment.Bottom:
                return availableHeight - totalTextHeight;

            case VerticalAlignment.Top:
            default:
                return 0;
        }
    }
}
