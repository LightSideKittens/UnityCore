using System;
using System.Buffers;
using UnityEngine;

/// <summary>
/// Базовый класс для модификаторов, рисующих горизонтальные линии (underline, strikethrough).
/// Наследники переопределяют GetLineOffset для указания вертикального смещения линии.
/// Каждый наследник имеет свой статический буфер флагов.
/// </summary>
[Serializable]
public abstract class BaseLineModifier : IRenderModifier
{
    protected struct LineSegment
    {
        public float startX;
        public float endX;
        public float baselineY;
        public int cluster;
    }

    // Threshold for detecting line break (Y position change)
    private const float LineBreakThreshold = 5f;

    // Каждый наследник должен предоставить свои статические буферы
    protected abstract ref ArrayPoolBuffer<byte> FlagsBuffer { get; }
    protected abstract ref LineSegment[] LineSegments { get; }
    protected abstract ref int LineSegmentsCapacity { get; }
    protected abstract ref int LineSegmentCount { get; }
    protected abstract ref bool LinesDrawnThisFrame { get; }

    /// <summary>
    /// Возвращает вертикальное смещение линии относительно baseline.
    /// </summary>
    protected abstract float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale);

    // Debug flag - set to true to see underline/strikethrough processing
    public static bool DebugLogging = false;

    void IModifier.Apply(int start, int end, string parameter)
    {
        int cpCount = SharedTextBuffers.codepointCount;
        ref var buffer = ref FlagsBuffer;
        buffer.EnsureCapacity(cpCount);
        buffer.SetFlagRange(start, Math.Min(end, cpCount));

        if (DebugLogging)
            UnityEngine.Debug.Log($"[{GetType().Name}.Apply] start={start}, end={end}, cpCount={cpCount}");
    }

    void IModifier.Initialize(UniText uniText)
    {
        var gen = uniText.MeshGenerator;
        gen.OnRebuildStart += OnRebuildStart;
        gen.OnAfterGlyphs += OnAfterGlyphs;
    }

    void IModifier.Deinitialize(UniText uniText)
    {
        var gen = uniText.MeshGenerator;
        if (gen == null) return;
        gen.OnRebuildStart -= OnRebuildStart;
        gen.OnAfterGlyphs -= OnAfterGlyphs;
    }

    private void OnRebuildStart()
    {
        // Reset the flag once per rebuild (before any fonts are processed)
        LinesDrawnThisFrame = false;
    }

    void IModifier.Reset() => ResetState();

    protected void ResetState()
    {
        FlagsBuffer.Clear();
        LineSegmentCount = 0;
        LinesDrawnThisFrame = false;
    }

    private void AddSegment(float startX, float endX, float baselineY, int cluster)
    {
        ref var segments = ref LineSegments;
        ref int segmentsCap = ref LineSegmentsCapacity;
        ref int segmentCount = ref LineSegmentCount;

        // Ensure capacity for line segments
        if (segmentCount >= segmentsCap)
        {
            int newCap = segmentsCap * 2;
            var newBuffer = ArrayPool<LineSegment>.Shared.Rent(newCap);
            segments.AsSpan(0, segmentCount).CopyTo(newBuffer);
            ArrayPool<LineSegment>.Shared.Return(segments);
            segments = newBuffer;
            segmentsCap = newCap;
        }

        segments[segmentCount++] = new LineSegment
        {
            startX = startX,
            endX = endX,
            baselineY = baselineY,
            cluster = cluster
        };
    }

    private void OnAfterGlyphs()
    {
        // Only draw lines once per text rebuild (OnAfterGlyphs is called for each font)
        if (LinesDrawnThisFrame)
            return;
        LinesDrawnThisFrame = true;

        var fontAsset = UniTextMeshGenerator.currentFontAsset;
        if (fontAsset == null)
            return;

        ref var flagsBuffer = ref FlagsBuffer;

        // Early exit if no flags are set at all
        if (!flagsBuffer.HasAnyFlags())
            return;

        // Reset segment count for this pass
        LineSegmentCount = 0;

        float scale = UniTextMeshGenerator.scale;
        float offsetX = UniTextMeshGenerator.offsetX;
        float offsetY = UniTextMeshGenerator.offsetY;

        // Get ALL positioned glyphs from SharedTextBuffers (in logical order)
        var allGlyphs = SharedTextBuffers.positionedGlyphs;
        int glyphCount = SharedTextBuffers.positionedGlyphCount;

        if (glyphCount == 0)
            return;

        // Build line segments by iterating through ALL glyphs in logical order
        float lineStartX = 0, lineEndX = 0, lineBaselineY = 0;
        int lineStartCluster = 0;
        bool hasActiveLine = false;
        float defaultWidth = 10f * scale;

        for (int i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref allGlyphs[i];
            int cluster = glyph.cluster;
            bool hasFlag = flagsBuffer.HasFlag(cluster);

            // Skip expensive calculations if no flag and no active line
            if (!hasFlag && !hasActiveLine)
                continue;

            // Calculate glyph bounds
            float glyphX = offsetX + glyph.x;
            float baselineY = offsetY - glyph.y;

            // Estimate glyph width - use advance from next glyph or fixed estimate
            float glyphWidth = defaultWidth;
            if (i + 1 < glyphCount)
            {
                ref readonly var nextGlyph = ref allGlyphs[i + 1];
                // Only use if on same line (similar Y) - inline abs
                float yDiffNext = nextGlyph.y - glyph.y;
                if (yDiffNext < 0) yDiffNext = -yDiffNext;
                if (yDiffNext < LineBreakThreshold)
                {
                    float nextX = offsetX + nextGlyph.x;
                    glyphWidth = nextX - glyphX;
                    if (glyphWidth < 0) glyphWidth = -glyphWidth;
                    if (glyphWidth < 1f) glyphWidth = defaultWidth;
                }
            }

            float left = glyphX;
            float right = glyphX + glyphWidth;

            if (hasFlag)
            {
                if (!hasActiveLine)
                {
                    // Start new line segment
                    lineStartX = left;
                    lineEndX = right;
                    lineBaselineY = baselineY;
                    lineStartCluster = cluster;
                    hasActiveLine = true;
                }
                else
                {
                    // Check for line break (Y position changed significantly) - inline abs
                    float yDiff = baselineY - lineBaselineY;
                    if (yDiff < 0) yDiff = -yDiff;
                    if (yDiff > LineBreakThreshold)
                    {
                        // Line break detected - finish current segment and start new one
                        AddSegment(lineStartX, lineEndX, lineBaselineY, lineStartCluster);
                        lineStartX = left;
                        lineEndX = right;
                        lineBaselineY = baselineY;
                        lineStartCluster = cluster;
                    }
                    else
                    {
                        // Same line - extend current segment
                        // Handle RTL: line might extend to the left
                        if (left < lineStartX) lineStartX = left;
                        if (right > lineEndX) lineEndX = right;
                        if (baselineY < lineBaselineY) lineBaselineY = baselineY;
                    }
                }
            }
            else if (hasActiveLine)
            {
                AddSegment(lineStartX, lineEndX, lineBaselineY, lineStartCluster);
                hasActiveLine = false;
            }
        }

        // Finish last segment if active
        if (hasActiveLine)
        {
            AddSegment(lineStartX, lineEndX, lineBaselineY, lineStartCluster);
        }

        // Now draw all segments
        int segmentCount = LineSegmentCount;
        if (segmentCount == 0)
            return;

        Color32 defaultColor = UniTextMeshGenerator.currentDefaultColor;
        float lineOffset = GetLineOffset(fontAsset.FaceInfo, scale);

        var segments = LineSegments;
        for (int i = 0; i < segmentCount; i++)
        {
            ref var seg = ref segments[i];

            // TryGetColor is more efficient than HasColor + GetColor
            Color32 color = ColorModifier.TryGetColor(seg.cluster, out var customColor)
                ? customColor
                : defaultColor;

            LineRenderHelper.DrawLine(seg.startX, seg.endX, seg.baselineY, lineOffset, color);
        }
    }
}
