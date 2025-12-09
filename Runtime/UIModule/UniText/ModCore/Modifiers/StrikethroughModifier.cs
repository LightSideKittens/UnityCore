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
    private static byte[] flags = RentCleared(256);
    private static int flagsCapacity = 256;
    private static LineSegment[] lineSegments = ArrayPool<LineSegment>.Shared.Rent(64);
    private static int lineSegmentsCapacity = 64;
    private static int lineSegmentCount;
    private static float lineStartX, lineEndX, lineBaselineY;
    private static int lineStartCluster;
    private static bool hasActiveLine;

    // Реализация abstract свойств
    protected override ref byte[] Flags => ref flags;
    protected override ref int FlagsCapacity => ref flagsCapacity;
    protected override ref LineSegment[] LineSegments => ref lineSegments;
    protected override ref int LineSegmentsCapacity => ref lineSegmentsCapacity;
    protected override ref int LineSegmentCount => ref lineSegmentCount;
    protected override ref float LineStartX => ref lineStartX;
    protected override ref float LineEndX => ref lineEndX;
    protected override ref float LineBaselineY => ref lineBaselineY;
    protected override ref int LineStartCluster => ref lineStartCluster;
    protected override ref bool HasActiveLine => ref hasActiveLine;

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
        flags.AsSpan(0, flagsCapacity).Clear();
        lineSegmentCount = 0;
        hasActiveLine = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasStrikethrough(int cluster)
    {
        return cluster >= 0 && cluster < flagsCapacity && flags[cluster] != 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        if (flags != null)
            ArrayPool<byte>.Shared.Return(flags);
        flags = RentCleared(256);
        flagsCapacity = 256;

        if (lineSegments != null)
            ArrayPool<LineSegment>.Shared.Return(lineSegments);
        lineSegments = ArrayPool<LineSegment>.Shared.Rent(64);
        lineSegmentsCapacity = 64;
        lineSegmentCount = 0;
        hasActiveLine = false;
    }
}
