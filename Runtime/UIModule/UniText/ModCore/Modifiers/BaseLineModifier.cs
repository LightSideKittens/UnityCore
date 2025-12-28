using System;
using UnityEngine;


[Serializable]
public abstract class BaseLineModifier : BaseModifier
{
    protected struct LineSegment
    {
        public float startX;
        public float endX;
        public float baselineY;
        public Color32 color;
    }

    private const float LineBreakThreshold = 5f;

    protected byte[] flagsBuffer;

    private LineSegment[] lineSegments;
    private int lineSegmentsCapacity;
    private int lineSegmentCount;
    private bool linesDrawnThisFrame;


    protected abstract string AttributeKey { get; }

    protected abstract float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale);
    protected abstract void SetStaticBuffer(byte[] buffer);

    protected sealed override void CreateBuffers()
    {
        var cpCount = buffers.codepoints.count;
        flagsBuffer = buffers.GetOrCreateAttributeArray<byte>(AttributeKey, cpCount);
        SetStaticBuffer(flagsBuffer);

        lineSegments = UniTextArrayPool<LineSegment>.Rent(64);
        lineSegmentsCapacity = 64;
        lineSegmentCount = 0;
        linesDrawnThisFrame = false;
    }

    protected sealed override void Subscribe()
    {
        uniText.Rebuilding += OnRebuilding;
        var gen = uniText.MeshGenerator;
        gen.OnRebuildStart += OnRebuildStart;
        gen.OnAfterGlyphsPerFont += OnAfterGlyphs;
    }

    protected sealed override void Unsubscribe()
    {
        uniText.Rebuilding -= OnRebuilding;
        var gen = uniText.MeshGenerator;
        if (gen != null)
        {
            gen.OnRebuildStart -= OnRebuildStart;
            gen.OnAfterGlyphsPerFont -= OnAfterGlyphs;
        }
    }

    protected sealed override void ReleaseBuffers()
    {
        SetStaticBuffer(null);
        buffers.ReleaseAttributeArray(AttributeKey);
        flagsBuffer = null;

        if (lineSegments != null)
        {
            UniTextArrayPool<LineSegment>.Return(lineSegments);
            lineSegments = null;
        }
    }

    protected sealed override void ClearBuffers()
    {
        lineSegmentCount = 0;
        linesDrawnThisFrame = false;
    }

    protected sealed override void OnApply(int start, int end, string parameter)
    {
        var cpCount = buffers.codepoints.count;
        EnsureBufferCapacity(cpCount);
        flagsBuffer.SetFlagRange(start, Math.Min(end, cpCount));

        buffers.virtualCodepoints.Add('_');
    }

    protected void EnsureBufferCapacity(int required)
    {
        if (flagsBuffer == null || flagsBuffer.Length < required)
        {
            flagsBuffer = buffers.GrowAttributeArray<byte>(AttributeKey, required);
            SetStaticBuffer(flagsBuffer);
        }
    }

    private void OnRebuilding()
    {
        flagsBuffer = buffers.GetAttributeArray<byte>(AttributeKey);
        SetStaticBuffer(flagsBuffer);
    }

    private void OnRebuildStart()
    {
        linesDrawnThisFrame = false;
    }

    private void AddSegment(float startX, float endX, float baselineY, Color32 color)
    {
        if (lineSegmentCount >= lineSegmentsCapacity)
        {
            var newCap = lineSegmentsCapacity * 2;
            var newBuffer = UniTextArrayPool<LineSegment>.Rent(newCap);
            lineSegments.AsSpan(0, lineSegmentCount).CopyTo(newBuffer);
            UniTextArrayPool<LineSegment>.Return(lineSegments);
            lineSegments = newBuffer;
            lineSegmentsCapacity = newCap;
        }

        lineSegments[lineSegmentCount] = new LineSegment
        {
            startX = startX,
            endX = endX,
            baselineY = baselineY,
            color = color
        };
        lineSegmentCount++;
    }

    private void OnAfterGlyphs()
    {
        if (!isInitialized) return;
        if (linesDrawnThisFrame) return;
        linesDrawnThisFrame = true;

        var gen = UniTextMeshGenerator.Current;
        var fontAsset = gen.currentFont;
        if (fontAsset == null) return;

        if (!flagsBuffer.HasAnyFlags()) return;

        lineSegmentCount = 0;

        var scale = gen.scale;
        var offsetX = gen.offsetX;
        var offsetY = gen.offsetY;
        var defaultColor = gen.currentDefaultColor;

        var buf = buffers;
        var allGlyphs = buf.positionedGlyphs.data;
        var glyphCount = buf.positionedGlyphs.count;

        if (glyphCount == 0) return;

        float lineStartX = 0, lineEndX = 0, lineBaselineY = 0;
        Color32 lineColor = default;
        var hasActiveLine = false;
        var defaultWidth = 10f * scale;

        for (var i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref allGlyphs[i];
            var cluster = glyph.cluster;
            var hasFlag = flagsBuffer.HasFlag(cluster);

            if (!hasFlag && !hasActiveLine) continue;

            var glyphX = offsetX + glyph.x;
            var baselineY = offsetY - glyph.y;
            var glyphColor = ColorModifier.TryGetColor(cluster, out var customColor) ? customColor : defaultColor;

            float glyphWidth = 0;
            if (i + 1 < glyphCount)
            {
                ref readonly var nextGlyph = ref allGlyphs[i + 1];
                var yDiffNext = nextGlyph.y - glyph.y;
                if (yDiffNext < 0) yDiffNext = -yDiffNext;
                if (yDiffNext < LineBreakThreshold)
                {
                    var nextX = offsetX + nextGlyph.x;
                    glyphWidth = nextX - glyphX;
                    if (glyphWidth < 0) glyphWidth = -glyphWidth;
                }
            }

            if (glyphWidth < 1f)
            {
                var idx = glyph.shapedGlyphIndex;

                if ((uint)idx < (uint)buf.shapedGlyphs.count && buf.glyphDataCache != null)
                {
                    ref var cached = ref buf.glyphDataCache[idx];
                    if (cached.isValid)
                        glyphWidth = (cached.bearingX + cached.width) * scale;
                }

                if (glyphWidth < 1f && (uint)idx < (uint)buf.shapedGlyphs.count)
                    glyphWidth = buf.shapedGlyphs[idx].advanceX * scale;
                if (glyphWidth < 1f)
                    glyphWidth = defaultWidth;
            }

            var left = glyphX;
            var right = glyphX + glyphWidth;

            if (hasFlag)
            {
                if (!hasActiveLine)
                {
                    lineStartX = left;
                    lineEndX = right;
                    lineBaselineY = baselineY;
                    lineColor = glyphColor;
                    hasActiveLine = true;
                }
                else
                {
                    var yDiff = baselineY - lineBaselineY;
                    if (yDiff < 0) yDiff = -yDiff;
                    var colorChanged = lineColor.r != glyphColor.r || lineColor.g != glyphColor.g ||
                                       lineColor.b != glyphColor.b || lineColor.a != glyphColor.a;

                    if (yDiff > LineBreakThreshold || colorChanged)
                    {
                        AddSegment(lineStartX, lineEndX, lineBaselineY, lineColor);
                        lineStartX = left;
                        lineEndX = right;
                        lineBaselineY = baselineY;
                        lineColor = glyphColor;
                    }
                    else
                    {
                        if (left < lineStartX) lineStartX = left;
                        if (right > lineEndX) lineEndX = right;
                        if (baselineY < lineBaselineY) lineBaselineY = baselineY;
                    }
                }
            }
            else if (hasActiveLine)
            {
                AddSegment(lineStartX, lineEndX, lineBaselineY, lineColor);
                hasActiveLine = false;
            }
        }

        if (hasActiveLine)
            AddSegment(lineStartX, lineEndX, lineBaselineY, lineColor);

        if (lineSegmentCount == 0) return;

        var lineOffset = GetLineOffset(fontAsset.FaceInfo, scale);

        var fontProvider = uniText.FontProvider;
        for (var i = 0; i < lineSegmentCount; i++)
        {
            ref var seg = ref lineSegments[i];
            LineRenderHelper.DrawLine(fontProvider, seg.startX, seg.endX, seg.baselineY, lineOffset, seg.color);
        }
    }
}
