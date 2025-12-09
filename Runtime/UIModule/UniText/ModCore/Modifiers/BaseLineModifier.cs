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

    // Каждый наследник должен предоставить свои статические буферы
    protected abstract ref byte[] Flags { get; }
    protected abstract ref int FlagsCapacity { get; }
    protected abstract ref LineSegment[] LineSegments { get; }
    protected abstract ref int LineSegmentsCapacity { get; }
    protected abstract ref int LineSegmentCount { get; }
    protected abstract ref float LineStartX { get; }
    protected abstract ref float LineEndX { get; }
    protected abstract ref float LineBaselineY { get; }
    protected abstract ref int LineStartCluster { get; }
    protected abstract ref bool HasActiveLine { get; }

    /// <summary>
    /// Возвращает вертикальное смещение линии относительно baseline.
    /// </summary>
    protected abstract float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale);

    void IModifier.Apply(int start, int end, string parameter)
    {
        int cpCount = SharedTextBuffers.codepointCount;

        ref var flags = ref Flags;
        ref int flagsCap = ref FlagsCapacity;

        // Ensure capacity
        if (cpCount > flagsCap)
        {
            var newBuffer = ArrayPool<byte>.Shared.Rent(cpCount);
            flags.AsSpan(0, flagsCap).CopyTo(newBuffer);
            newBuffer.AsSpan(flagsCap, cpCount - flagsCap).Clear();
            ArrayPool<byte>.Shared.Return(flags);
            flags = newBuffer;
            flagsCap = cpCount;
        }

        // Clamp to valid range
        if (start < 0) start = 0;
        if (end > cpCount) end = cpCount;

        // Set flag for range
        for (int i = start; i < end; i++)
            flags[i] = 1;
    }

    void IModifier.Initialize(UniText uniText)
    {
        var gen = uniText.MeshGenerator;
        gen.OnBeforeMesh += OnBeforeMesh;
        gen.OnGlyph += OnGlyph;
        gen.OnAfterGlyphs += OnAfterGlyphs;
    }

    void IModifier.Deinitialize(UniText uniText)
    {
        var gen = uniText.MeshGenerator;
        if (gen == null) return;
        gen.OnBeforeMesh -= OnBeforeMesh;
        gen.OnGlyph -= OnGlyph;
        gen.OnAfterGlyphs -= OnAfterGlyphs;
    }

    void IModifier.Reset() => ResetState();

    protected void ResetState()
    {
        Flags.AsSpan(0, FlagsCapacity).Clear();
        LineSegmentCount = 0;
        HasActiveLine = false;
    }

    private void OnBeforeMesh()
    {
        LineSegmentCount = 0;
        HasActiveLine = false;
    }

    private void OnGlyph()
    {
        int cluster = UniTextMeshGenerator.CurrentCluster;
        ref var flags = ref Flags;
        int flagsCap = FlagsCapacity;
        bool hasFlag = cluster >= 0 && cluster < flagsCap && flags[cluster] != 0;

        // Get glyph bounds from vertices (last 4 vertices: BL, TL, TR, BR)
        int baseIdx = UniTextMeshGenerator.VertexCount - 4;
        var verts = UniTextMeshGenerator.Vertices;
        float left = verts[baseIdx].x;
        float right = verts[baseIdx + 2].x;
        float baselineY = UniTextMeshGenerator.CurrentBaselineY;

        if (hasFlag)
        {
            if (!HasActiveLine)
            {
                LineStartX = left;
                LineEndX = right;
                LineBaselineY = baselineY;
                LineStartCluster = cluster;
                HasActiveLine = true;
            }
            else
            {
                LineEndX = right;
                LineBaselineY = Mathf.Min(LineBaselineY, baselineY);
            }
        }
        else if (HasActiveLine)
        {
            FinishLine();
        }
    }

    private void FinishLine()
    {
        if (!HasActiveLine)
            return;

        ref var segments = ref LineSegments;
        ref int segmentsCap = ref LineSegmentsCapacity;
        ref int segmentCount = ref LineSegmentCount;

        // Ensure capacity
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
            startX = LineStartX,
            endX = LineEndX,
            baselineY = LineBaselineY,
            cluster = LineStartCluster
        };

        HasActiveLine = false;
    }

    private void OnAfterGlyphs()
    {
        FinishLine();

        int segmentCount = LineSegmentCount;
        if (segmentCount == 0)
            return;

        var fontAsset = UniTextMeshGenerator.CurrentFontAsset;
        if (fontAsset == null)
            return;

        float scale = UniTextMeshGenerator.Scale;
        Color32 defaultColor = UniTextMeshGenerator.CurrentDefaultColor;
        float lineOffset = GetLineOffset(fontAsset.FaceInfo, scale);

        var segments = LineSegments;
        for (int i = 0; i < segmentCount; i++)
        {
            ref var seg = ref segments[i];

            Color32 color = ColorModifier.HasColor(seg.cluster)
                ? ColorModifier.GetColor(seg.cluster)
                : defaultColor;

            LineRenderHelper.DrawLine(seg.startX, seg.endX, seg.baselineY, lineOffset, color);
        }
    }

    protected static byte[] RentCleared(int size)
    {
        var arr = ArrayPool<byte>.Shared.Rent(size);
        arr.AsSpan(0, size).Clear();
        return arr;
    }
}
