using System;

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

        if (lines.IsEmpty)
        {
            return;
        }

        // fontLineHeight и fontAscender уже приходят с учётом scale из TMPFontProvider.GetLineMetrics()
        // Не умножаем на fontScale повторно!
        float computedLineHeight = fontLineHeight;
        if (computedLineHeight <= 0)
        {
            computedLineHeight = fontAscender - fontDescender;
        }
        if (computedLineHeight <= 0)
        {
            computedLineHeight = settings.defaultLineHeight;
        }

        // Ascender для первой строки (уже с учётом scale)
        float ascender = fontAscender;
        if (ascender <= 0) ascender = computedLineHeight * 0.8f;

        // Общая визуальная высота текста
        float totalTextHeight = ascender + (lines.Length - 1) * (computedLineHeight + settings.lineSpacing);

        // Начальная Y позиция с учётом вертикального выравнивания
        float y = ComputeTextStartY(totalTextHeight, settings) + ascender;
        float maxWidth = 0;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            // Вычисляем реальную ширину строки (сумма ширин всех runs)
            float lineWidth = 0;
            for (int r = 0; r < line.runCount; r++)
            {
                lineWidth += runs[line.runStart + r].width;
            }

            // Начальная X позиция с учётом base direction параграфа этой строки
            // Каждая строка использует baseLevel своего параграфа (UAX #9)
            var lineDirection = (line.paragraphBaseLevel & 1) == 1
                ? TextDirection.RightToLeft
                : TextDirection.LeftToRight;
            float lineStartX = ComputeLineStartX(lineWidth, lineDirection, settings);
            float x = lineStartX;

            // Позиционируем каждый run в строке
            for (int r = 0; r < line.runCount; r++)
            {
                var run = runs[line.runStart + r];
                var runGlyphs = glyphs.Slice(run.glyphStart, run.glyphCount);

                // Направление run
                if (run.direction == TextDirection.RightToLeft)
                {
                    // RTL: глифы внутри run идут справа налево
                    // Сначала переходим в конец run, потом размещаем глифы справа налево
                    float runEndX = x + run.width;

                    for (int g = 0; g < runGlyphs.Length; g++)
                    {
                        var glyph = runGlyphs[g];
                        runEndX -= glyph.advanceX;

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = glyph.glyphId,
                            x = runEndX + glyph.offsetX,
                            y = y + glyph.offsetY,
                            fontId = run.fontId,
                            attributeSnapshot = run.attributeSnapshot
                        };
                    }

                    // Переходим к следующему run
                    x += run.width;
                }
                else
                {
                    // LTR
                    for (int g = 0; g < runGlyphs.Length; g++)
                    {
                        var glyph = runGlyphs[g];

                        result[glyphCount++] = new PositionedGlyph
                        {
                            glyphId = glyph.glyphId,
                            x = x + glyph.offsetX,
                            y = y + glyph.offsetY,
                            fontId = run.fontId,
                            attributeSnapshot = run.attributeSnapshot
                        };

                        x += glyph.advanceX;
                    }
                }
            }

            if (lineWidth > maxWidth)
                maxWidth = lineWidth;

            // Следующая строка
            y += computedLineHeight + settings.lineSpacing;
        }

        width = maxWidth;
        height = totalTextHeight;
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
