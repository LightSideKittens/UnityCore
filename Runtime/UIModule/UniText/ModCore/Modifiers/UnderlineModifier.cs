using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using UnityEngine;

/// <summary>
/// Модификатор Underline текста.
/// Рисует линию под текстом на уровне underlineOffset из шрифта.
/// </summary>
[Serializable]
public class UnderlineModifier : BaseLineModifier
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
        return faceInfo.underlineOffset * scale;
    }

    public static void ResetStatic()
    {
        flagsBuffer.Clear();
        lineSegmentCount = 0;
        linesDrawnThisFrame = false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasUnderline(int cluster) => flagsBuffer.HasFlag(cluster);

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
