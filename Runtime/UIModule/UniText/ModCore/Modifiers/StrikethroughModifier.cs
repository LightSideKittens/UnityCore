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
    // Instance буферы (реальные данные)
    private ArrayPoolBuffer<byte> instanceFlagsBuffer;
    private LineSegment[] instanceLineSegments;
    private int instanceLineSegmentsCapacity;
    private int instanceLineSegmentCount;
    private bool instanceLinesDrawnThisFrame;

    // Статический указатель только для HasStrikethrough (внешний API)
    private static ArrayPoolBuffer<byte> currentFlagsBuffer;

    // Свойства работают с INSTANCE полями
    protected override ArrayPoolBuffer<byte> FlagsBuffer => instanceFlagsBuffer;
    protected override LineSegment[] LineSegments { get => instanceLineSegments; set => instanceLineSegments = value; }
    protected override int LineSegmentsCapacity { get => instanceLineSegmentsCapacity; set => instanceLineSegmentsCapacity = value; }
    protected override int LineSegmentCount { get => instanceLineSegmentCount; set => instanceLineSegmentCount = value; }
    protected override bool LinesDrawnThisFrame { get => instanceLinesDrawnThisFrame; set => instanceLinesDrawnThisFrame = value; }

    protected override float GetLineOffset(UnityEngine.TextCore.FaceInfo faceInfo, float scale)
    {
        if (faceInfo.strikethroughOffset != 0)
            return faceInfo.strikethroughOffset * scale;

        float xHeightMid = faceInfo.meanLine > 0
            ? faceInfo.meanLine * 0.5f
            : faceInfo.ascentLine * 0.35f;
        return xHeightMid * scale;
    }

    protected override void CreateInstanceBuffers()
    {
        instanceFlagsBuffer = new ArrayPoolBuffer<byte>(256);
        instanceLineSegments = ArrayPool<LineSegment>.Shared.Rent(64);
        instanceLineSegmentsCapacity = 64;
        instanceLineSegmentCount = 0;
        instanceLinesDrawnThisFrame = false;
    }

    protected override void SetStaticBuffers()
    {
        // Только для внешнего API (HasStrikethrough)
        currentFlagsBuffer = instanceFlagsBuffer;
    }

    protected override void ReturnBuffersToPool()
    {
        instanceFlagsBuffer?.ReturnToPool();
        instanceFlagsBuffer = null;
        if (instanceLineSegments != null)
        {
            ArrayPool<LineSegment>.Shared.Return(instanceLineSegments);
            instanceLineSegments = null;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasStrikethrough(int cluster) => currentFlagsBuffer != null && currentFlagsBuffer.HasFlag(cluster);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void OnDomainReload()
    {
        currentFlagsBuffer = null;
    }
}
