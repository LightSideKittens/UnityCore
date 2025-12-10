using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Strikethrough текста.
/// Рисует линию через середину текста (strikethroughOffset или fallback на x-height).
/// </summary>
[Serializable]
public class StrikethroughModifier : BaseLineModifier
{
    // Статические буферы для этого модификатора
    private static ArrayPoolBuffer<byte> flagsBuffer = new(256);
    private static LineSegment[] lineSegments = ArrayPool<LineSegment>.Shared.Rent(64);
    private static int lineSegmentsCapacity = 64;
    private static int lineSegmentCount;
    private static bool linesDrawnThisFrame;

    // Реализация abstract свойств
    protected override ref ArrayPoolBuffer<byte> FlagsBuffer => ref flagsBuffer;
    protected override ref LineSegment[] LineSegments => ref lineSegments;
    protected override ref int LineSegmentsCapacity => ref lineSegmentsCapacity;
    protected override ref int LineSegmentCount => ref lineSegmentCount;
    protected override ref bool LinesDrawnThisFrame => ref linesDrawnThisFrame;

    protected override float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale)
    {
        // Strikethrough offset: если в шрифте 0, используем fallback на основе x-height
        if (faceInfo.strikethroughOffset != 0)
            return faceInfo.strikethroughOffset * scale;

        // Fallback: середина x-height (meanLine — верх строчных букв)
        float xHeightMid = faceInfo.meanLine > 0
            ? faceInfo.meanLine * 0.5f
            : faceInfo.ascentLine * 0.35f;
        return xHeightMid * scale;
    }

    public static void ResetStatic()
    {
        flagsBuffer.Clear();
        lineSegmentCount = 0;
        linesDrawnThisFrame = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasStrikethrough(int cluster) => flagsBuffer.HasFlag(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        flagsBuffer.Reset();

        if (lineSegments != null)
            ArrayPool<LineSegment>.Shared.Return(lineSegments);
        lineSegments = ArrayPool<LineSegment>.Shared.Rent(64);
        lineSegmentsCapacity = 64;
        lineSegmentCount = 0;
        linesDrawnThisFrame = false;
    }
}
