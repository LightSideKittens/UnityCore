using System;
using System.Buffers;
using UnityEngine;

/// <summary>
/// Base class for line-based modifiers (Underline, Strikethrough).
/// Uses shared flags buffer from CommonData with reference counting.
/// LineSegments are instance-specific (positions depend on each UniText's layout).
/// </summary>
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

    // Shared buffer from CommonData
    protected ArrayPoolBuffer<byte> flagsBuffer;

    // Instance-specific line segments (positions differ per UniText)
    private LineSegment[] lineSegments;
    private int lineSegmentsCapacity;
    private int lineSegmentCount;
    private bool linesDrawnThisFrame;

    /// <summary>Unique key for this attribute type. Use AttributeKeys constants.</summary>
    protected abstract string AttributeKey { get; }

    protected abstract float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale);
    protected abstract void SetStaticBuffer(ArrayPoolBuffer<byte> buffer);

    public static bool DebugLogging = false;

    protected sealed override void CreateBuffers()
    {
        // Acquire shared buffer from CommonData (increases refCount)
        flagsBuffer = CommonData.Current.AcquireAttribute<byte>(AttributeKey);
        SetStaticBuffer(flagsBuffer);

        // Instance-specific line segments
        lineSegments = ArrayPool<LineSegment>.Shared.Rent(64);
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
        // Release shared buffer reference (decreases refCount, returns to pool when 0)
        CommonData.Current?.ReleaseAttribute(AttributeKey);
        flagsBuffer = null;

        // Return instance-specific line segments
        if (lineSegments != null)
        {
            ArrayPool<LineSegment>.Shared.Return(lineSegments);
            lineSegments = null;
        }
    }

    // Clear is handled by CommonData.ClearAllAttributes() for flags
    protected sealed override void ClearBuffers()
    {
        lineSegmentCount = 0;
        linesDrawnThisFrame = false;
    }

    protected sealed override void ApplyModifier(int start, int end, string parameter)
    {
        int cpCount = CommonData.Current.codepointCount;
        flagsBuffer.EnsureCapacity(cpCount);
        flagsBuffer.SetFlagRange(start, Math.Min(end, cpCount));

        if (DebugLogging)
            UnityEngine.Debug.Log($"[{GetType().Name}.Apply] start={start}, end={end}, cpCount={cpCount}");
    }

    private void OnRebuilding()
    {
        // Update cached reference (without changing refCount)
        flagsBuffer = CommonData.Current.GetAttribute<byte>(AttributeKey);
        SetStaticBuffer(flagsBuffer);
    }

    private void OnRebuildStart() => linesDrawnThisFrame = false;

    private void AddSegment(float startX, float endX, float baselineY, Color32 color)
    {
        if (lineSegmentCount >= lineSegmentsCapacity)
        {
            int newCap = lineSegmentsCapacity * 2;
            var newBuffer = ArrayPool<LineSegment>.Shared.Rent(newCap);
            lineSegments.AsSpan(0, lineSegmentCount).CopyTo(newBuffer);
            ArrayPool<LineSegment>.Shared.Return(lineSegments);
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

        var fontAsset = UniTextMeshGenerator.currentFontAsset;
        if (fontAsset == null) return;

        if (!flagsBuffer.HasAnyFlags()) return;

        lineSegmentCount = 0;

        float scale = UniTextMeshGenerator.scale;
        float offsetX = UniTextMeshGenerator.offsetX;
        float offsetY = UniTextMeshGenerator.offsetY;
        Color32 defaultColor = UniTextMeshGenerator.currentDefaultColor;

        var buf = CommonData.Current;
        var allGlyphs = buf.positionedGlyphs;
        int glyphCount = buf.positionedGlyphCount;

        if (glyphCount == 0) return;

        float lineStartX = 0, lineEndX = 0, lineBaselineY = 0;
        Color32 lineColor = default;
        bool hasActiveLine = false;
        float defaultWidth = 10f * scale;

        for (int i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref allGlyphs[i];
            int cluster = glyph.cluster;
            bool hasFlag = flagsBuffer.HasFlag(cluster);

            if (!hasFlag && !hasActiveLine) continue;

            float glyphX = offsetX + glyph.x;
            float baselineY = offsetY - glyph.y;
            Color32 glyphColor = ColorModifier.TryGetColor(cluster, out var customColor) ? customColor : defaultColor;

            float glyphWidth = 0;
            if (i + 1 < glyphCount)
            {
                ref readonly var nextGlyph = ref allGlyphs[i + 1];
                float yDiffNext = nextGlyph.y - glyph.y;
                if (yDiffNext < 0) yDiffNext = -yDiffNext;
                if (yDiffNext < LineBreakThreshold)
                {
                    float nextX = offsetX + nextGlyph.x;
                    glyphWidth = nextX - glyphX;
                    if (glyphWidth < 0) glyphWidth = -glyphWidth;
                }
            }

            if (glyphWidth < 1f)
            {
                int idx = glyph.shapedGlyphIndex;

                if ((uint)idx < (uint)buf.shapedGlyphCount && buf.glyphDataCache != null)
                {
                    ref var cached = ref buf.glyphDataCache[idx];
                    if (cached.isValid)
                        glyphWidth = (cached.bearingX + cached.width) * scale;
                }

                if (glyphWidth < 1f && (uint)idx < (uint)buf.shapedGlyphCount)
                    glyphWidth = buf.shapedGlyphs[idx].advanceX * scale;
                if (glyphWidth < 1f)
                    glyphWidth = defaultWidth;
            }

            float left = glyphX;
            float right = glyphX + glyphWidth;

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
                    float yDiff = baselineY - lineBaselineY;
                    if (yDiff < 0) yDiff = -yDiff;
                    bool colorChanged = lineColor.r != glyphColor.r || lineColor.g != glyphColor.g ||
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

        float lineOffset = GetLineOffset(fontAsset.FaceInfo, scale);

        for (int i = 0; i < lineSegmentCount; i++)
        {
            ref var seg = ref lineSegments[i];
            LineRenderHelper.DrawLine(seg.startX, seg.endX, seg.baselineY, lineOffset, seg.color);
        }
    }
}
