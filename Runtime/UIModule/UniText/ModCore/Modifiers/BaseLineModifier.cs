using System;
using System.Buffers;
using UnityEngine;

/// <summary>
/// Базовый класс для модификаторов, рисующих горизонтальные линии (underline, strikethrough).
/// Наследники переопределяют GetLineOffset для указания вертикального смещения линии.
/// </summary>
[Serializable]
public abstract class BaseLineModifier : IModifier
{
    protected struct LineSegment
    {
        public float startX;
        public float endX;
        public float baselineY;
        public int cluster;
    }

    private const float LineBreakThreshold = 5f;

    // Наследник предоставляет доступ к статическим указателям (установленным через SetStaticBuffers)
    protected abstract ArrayPoolBuffer<byte> FlagsBuffer { get; }
    protected abstract LineSegment[] LineSegments { get; set; }
    protected abstract int LineSegmentsCapacity { get; set; }
    protected abstract int LineSegmentCount { get; set; }
    protected abstract bool LinesDrawnThisFrame { get; set; }

    protected abstract float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale);

    // Наследник создаёт instance буферы
    protected abstract void CreateInstanceBuffers();
    // Наследник записывает instance буферы в static поля
    protected abstract void SetStaticBuffers();
    // Наследник возвращает буферы в пул
    protected abstract void ReturnBuffersToPool();

    public static bool DebugLogging = false;

    void IModifier.Apply(int start, int end, string parameter)
    {
        int cpCount = SharedTextBuffers.Current.codepointCount;
        var buffer = FlagsBuffer;
        buffer.EnsureCapacity(cpCount);
        buffer.SetFlagRange(start, Math.Min(end, cpCount));

        if (DebugLogging)
            UnityEngine.Debug.Log($"[{GetType().Name}.Apply] start={start}, end={end}, cpCount={cpCount}");
    }

    void IModifier.Initialize(UniText uniText)
    {
        CreateInstanceBuffers();
        uniText.Rebuilding += OnRebuilding;
        var gen = uniText.MeshGenerator;
        gen.OnRebuildStart += OnRebuildStart;
        gen.OnAfterGlyphs += OnAfterGlyphs;
    }

    void IModifier.Deinitialize(UniText uniText)
    {
        uniText.Rebuilding -= OnRebuilding;
        var gen = uniText.MeshGenerator;
        if (gen != null)
        {
            gen.OnRebuildStart -= OnRebuildStart;
            gen.OnAfterGlyphs -= OnAfterGlyphs;
        }
        ReturnBuffersToPool();
    }

    private void OnRebuilding()
    {
        SetStaticBuffers();
    }

    private void OnRebuildStart()
    {
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
        var segments = LineSegments;
        int segmentsCap = LineSegmentsCapacity;
        int segmentCount = LineSegmentCount;

        if (segmentCount >= segmentsCap)
        {
            int newCap = segmentsCap * 2;
            var newBuffer = ArrayPool<LineSegment>.Shared.Rent(newCap);
            segments.AsSpan(0, segmentCount).CopyTo(newBuffer);
            ArrayPool<LineSegment>.Shared.Return(segments);
            LineSegments = newBuffer;
            LineSegmentsCapacity = newCap;
            segments = newBuffer;
        }

        segments[segmentCount] = new LineSegment
        {
            startX = startX,
            endX = endX,
            baselineY = baselineY,
            cluster = cluster
        };
        LineSegmentCount = segmentCount + 1;
    }

    private void OnAfterGlyphs()
    {
        if (LinesDrawnThisFrame)
            return;
        LinesDrawnThisFrame = true;

        var fontAsset = UniTextMeshGenerator.currentFontAsset;
        if (fontAsset == null)
            return;

        var flagsBuffer = FlagsBuffer;
        if (!flagsBuffer.HasAnyFlags())
            return;

        LineSegmentCount = 0;

        float scale = UniTextMeshGenerator.scale;
        float offsetX = UniTextMeshGenerator.offsetX;
        float offsetY = UniTextMeshGenerator.offsetY;

        var buf = SharedTextBuffers.Current;
        var allGlyphs = buf.positionedGlyphs;
        int glyphCount = buf.positionedGlyphCount;

        if (glyphCount == 0)
            return;

        float lineStartX = 0, lineEndX = 0, lineBaselineY = 0;
        int lineStartCluster = 0;
        bool hasActiveLine = false;
        float defaultWidth = 10f * scale;

        for (int i = 0; i < glyphCount; i++)
        {
            ref readonly var glyph = ref allGlyphs[i];
            int cluster = glyph.cluster;
            bool hasFlag = flagsBuffer.HasFlag(cluster);

            if (!hasFlag && !hasActiveLine)
                continue;

            float glyphX = offsetX + glyph.x;
            float baselineY = offsetY - glyph.y;

            float glyphWidth = defaultWidth;
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
                    if (glyphWidth < 1f) glyphWidth = defaultWidth;
                }
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
                    lineStartCluster = cluster;
                    hasActiveLine = true;
                }
                else
                {
                    float yDiff = baselineY - lineBaselineY;
                    if (yDiff < 0) yDiff = -yDiff;
                    if (yDiff > LineBreakThreshold)
                    {
                        AddSegment(lineStartX, lineEndX, lineBaselineY, lineStartCluster);
                        lineStartX = left;
                        lineEndX = right;
                        lineBaselineY = baselineY;
                        lineStartCluster = cluster;
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
                AddSegment(lineStartX, lineEndX, lineBaselineY, lineStartCluster);
                hasActiveLine = false;
            }
        }

        if (hasActiveLine)
        {
            AddSegment(lineStartX, lineEndX, lineBaselineY, lineStartCluster);
        }

        int segmentCount = LineSegmentCount;
        if (segmentCount == 0)
            return;

        Color32 defaultColor = UniTextMeshGenerator.currentDefaultColor;
        float lineOffset = GetLineOffset(fontAsset.FaceInfo, scale);

        var segments = LineSegments;
        for (int i = 0; i < segmentCount; i++)
        {
            ref var seg = ref segments[i];

            Color32 color = ColorModifier.TryGetColor(seg.cluster, out var customColor)
                ? customColor
                : defaultColor;

            LineRenderHelper.DrawLine(seg.startX, seg.endX, seg.baselineY, lineOffset, color);
        }
    }
}
