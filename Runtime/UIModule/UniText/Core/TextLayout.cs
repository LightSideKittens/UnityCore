using System;
using System.Runtime.CompilerServices;
using UnityEngine;


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


public sealed class TextLayout
{
    private LayoutSettings settings;

    private float fontAscender;
    private float fontDescender;
    private float fontLineHeight;
    private float glyphScale = 1f;

    public TextLayout()
    {
        settings = LayoutSettings.Default;
    }
    
    public void SetFontMetrics(float ascender, float descender, float lineHeight, float glyphScaleFactor = 1f)
    {
        fontAscender = ascender;
        fontDescender = descender;
        fontLineHeight = lineHeight;
        glyphScale = glyphScaleFactor;
    }


    public void SetLayoutSettings(LayoutSettings newSettings)
    {
        settings = newSettings;
    }


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

        var lineCount = lines.Length;
        if (lineCount == 0)
            return;

        var computedLineHeight = fontLineHeight;
        if (computedLineHeight <= 0)
            computedLineHeight = fontAscender - fontDescender;
        if (computedLineHeight <= 0)
            computedLineHeight = settings.defaultLineHeight;

        var ascender = fontAscender;
        if (ascender <= 0) ascender = computedLineHeight * 0.8f;
        var descender = fontDescender;
        if (descender >= 0) descender = -computedLineHeight * 0.2f;

        var lineSpacing = settings.lineSpacing;
        var totalTextHeight =
            UniTextFontProvider.CalculateTextHeight(ascender, descender, lineCount, computedLineHeight, lineSpacing);

        var y = ComputeTextStartY(totalTextHeight, settings) + ascender;
        float maxLineWidth = 0;

        var availableWidth = settings.maxWidth;
        var hAlign = settings.horizontalAlignment;
        var hasFiniteWidth = !float.IsInfinity(availableWidth) && availableWidth > 0;

        for (var i = 0; i < lineCount; i++)
        {
            ref readonly var line = ref lines[i];
            var runStart = line.runStart;
            var runCount = line.runCount;
            var runEnd = runStart + runCount;

            var lineWidth = line.width * glyphScale;

            float x;
            var isRtlLine = (line.paragraphBaseLevel & 1) == 1;
            if (hasFiniteWidth)
                x = ComputeLineStartX(lineWidth, isRtlLine, availableWidth, hAlign);
            else
                x = 0;

            if (line.startMargin > 0 && hasFiniteWidth)
            {
                var margin = line.startMargin * glyphScale;
                if (isRtlLine)
                {
                    if (hAlign == HorizontalAlignment.Left)
                        x -= margin;
                    else if (hAlign == HorizontalAlignment.Center) x = (availableWidth - margin - lineWidth) * 0.5f;
                }
                else
                {
                    if (hAlign == HorizontalAlignment.Left)
                        x += margin;
                    else if (hAlign == HorizontalAlignment.Center)
                        x = margin + (availableWidth - margin - lineWidth) * 0.5f;
                }
            }

            for (var r = runStart; r < runEnd; r++)
            {
                ref readonly var run = ref runs[r];
                var glyphStart = run.glyphStart;
                var glyphLen = run.glyphCount;

                Debug.Assert(glyphStart >= 0 && glyphStart + glyphLen <= glyphs.Length);

                var fontId = run.fontId;
                var glyphEnd = glyphStart + glyphLen;

                var clusterOffset = run.range.start;

                for (var g = glyphStart; g < glyphEnd; g++)
                {
                    ref readonly var glyph = ref glyphs[g];
                    result[glyphCount++] = new PositionedGlyph
                    {
                        glyphId = glyph.glyphId,
                        cluster = glyph.cluster + clusterOffset,
                        x = x + glyph.offsetX * glyphScale,
                        y = y - glyph.offsetY * glyphScale,
                        fontId = fontId,
                        shapedGlyphIndex = g
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
    private static float ComputeLineStartX(float lineWidth, bool isRtlLine, float availableWidth,
        HorizontalAlignment alignment)
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
        var availableHeight = settings.maxHeight;
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